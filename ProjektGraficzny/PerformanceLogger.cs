using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fractals;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace ProjektGraficzny
{
    public class PerformanceLogger
    {

        private readonly Stopwatch stopWatch;

        private List<PerformanceLoggerEntry> entries;

        private FractalType currentFractalType;
        private DrawingMode currentDrawingMode;

        private MainWindow mainWindow;

        public PerformanceLogger(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;

            stopWatch = new Stopwatch();
            entries = new List<PerformanceLoggerEntry>();
        }

        public void Start(FractalType fractalType, DrawingMode drawingMode)
        {
            stopWatch.Restart();

            this.currentFractalType = fractalType;
            this.currentDrawingMode = drawingMode;
        }

        public void Stop(long totalIterations)
        {
            stopWatch.Stop();

            int timeTicks = (int)stopWatch.ElapsedTicks;

            PerformanceLoggerEntry newEntry = new PerformanceLoggerEntry(currentFractalType, currentDrawingMode, totalIterations, timeTicks);
            entries.Add(newEntry);

            Console.WriteLine(totalIterations + " in " + timeTicks + " ticks, rate: " + ((double)totalIterations/timeTicks));

            UpdatePerformanceText(newEntry);

            //mainWindow.SendMessageToClient(0, newEntry.ToString());
        }

        private void UpdatePerformanceText(PerformanceLoggerEntry entry)
        {
            var frameMs = (double) entry.timeTicks / TimeSpan.TicksPerMillisecond;
            var framesPerSecond = (int)Math.Round(1000.0 / frameMs);

            mainWindow.PerformanceText.Text =
                $"{entry.fractalType} - {entry.drawingMode}\nFrame time: {frameMs}ms\nFPS: {framesPerSecond}\nIterations: {entry.totalIterations.ToString("N0",System.Globalization.CultureInfo.GetCultureInfo("de"))}\nIterations per tick: {Math.Round( (double) entry.totalIterations / entry.timeTicks) }";
        }

        public void WriteLogsToService()
        {
            String s = "";
            for (int a = 0; a < 2; a++)
            {
                FractalType fractalType = (FractalType) a;
                for (int b = 0; b < 3; b++)
                {
                    DrawingMode drawingMode = (DrawingMode)b;

                    int count = 0;
                    ulong totalIterations = 0;
                    ulong totalTicks = 0;

                    foreach (var entry in entries)
                    {
                        if (entry.drawingMode == drawingMode && entry.fractalType == fractalType)
                        {
                            // pomin pierwszy rekord GPU
                            if (drawingMode == DrawingMode.Gpu && count == 0)
                            {
                                count++;
                                continue;
                            }

                            totalIterations += (ulong) entry.totalIterations;
                            totalTicks += (ulong) entry.timeTicks;

                            count++;
                        }
                    }

                    if (drawingMode == DrawingMode.Gpu)
                        count--;

                    if (count > 0)
                    {
                        double performance = (double) totalIterations / totalTicks;
                        s += $"Fraktal: {fractalType}, Tryb: {drawingMode}, Współczynnik wydajności: {performance}#";
                    }
                }
            }

            if (s != "")
            {
                mainWindow.SendMessageToClient(1, s);
            }
        }

        // mode:
        // 0 = regular
        // 1 = compressed
        // 2 = encrypted
        public void ExportLogsToCsv(int mode = 0)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "csv files (*.csv)|*.csv";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            try
            {
                if (saveFileDialog1.ShowDialog().Value)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        WriteLogsToStream(mode, myStream);
                        // Code to write the stream goes here.
                        myStream.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas zapisywania pliku: " + ex.Message);
            }   
        }

        private void WriteLogsToStream(int mode, Stream stream)
        {
            string s = $"Fraktal{Settings.csvSeparator}Tryb{Settings.csvSeparator}Iteracje{Settings.csvSeparator}Czas (ticks)\n";

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string pubkey = rsa.ToXmlString(false);

            foreach (var entry in entries)
            {
                string ss = entry.ToString() + "\n";
       
                if (mode == 2)
                {
                    // szyfrowanie kazdej linijki
                    byte[] bytes = RSAEncrypt(Encoding.UTF8.GetBytes(ss), pubkey);
                    ss = Encoding.UTF8.GetString(bytes);
                }

                s += ss;
            }

            switch (mode)
            {
                case 0: // regular
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(s);

                    stream.Write(bytes, 0, bytes.Length);
                    break;
                }
                case 1: // compressed
                {
                    byte[] bytes = Zip(s);

                    stream.Write(bytes, 0, bytes.Length);
                    break;
                }              
                case 2: // encrypted
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(s);

                    stream.Write(bytes, 0, bytes.Length);
                    break;
                }
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[src.Length];

            int cnt;
            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        // Kompresja string -> Gzip -> B64 string
        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var inputStream = new MemoryStream(bytes))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        inputStream.CopyTo(gZipStream);
                        //CopyTo(inputStream, gZipStream);
                    }

                    byte[] outputArray = outputStream.ToArray();
                    string encodedArray = Convert.ToBase64String(outputArray);

                    //Console.WriteLine(Encoding.UTF8.GetString(outputArray));
                    //Console.WriteLine(encodedArray);

                    return Encoding.UTF8.GetBytes(encodedArray);
                }
            }
        }

        // Dekompresja B64 string -> Gzip -> string
        public static string Unzip(byte[] bytes)
        {
            using (var inputStream = new MemoryStream(bytes))
            {
                using (var outputStream = new MemoryStream())
                {
                    byte[] encodedArray = inputStream.ToArray();
                    string encodedString = Encoding.UTF8.GetString(encodedArray);
                    byte[] decodedArray = Convert.FromBase64String(encodedString);

                    using (var decodedStream = new MemoryStream(decodedArray))
                    {
                        //Console.WriteLine(Encoding.UTF8.GetString(decodedArray));
                        //Console.WriteLine(encodedString);

                        using (var gZipStream = new GZipStream(decodedStream, CompressionMode.Decompress))
                        {
                            gZipStream.CopyTo(outputStream);
                            gZipStream.Close();
                        }

                    }

                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
        }

        public static byte[] RSAEncrypt(byte[] plaintext, string destKey)
        {
            byte[] encryptedData;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(destKey);
            encryptedData = rsa.Encrypt(plaintext, true);
            rsa.Dispose();
            return encryptedData;
        }

        public static byte[] RSADecrypt(byte[] ciphertext, string srcKey)
        {
            byte[] decryptedData;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(srcKey);
            decryptedData = rsa.Decrypt(ciphertext, true);
            rsa.Dispose();
            return decryptedData;
        }

    }
}
