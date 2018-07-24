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

            //mainWindow.SendMessageToClient(0, newEntry.ToString());
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

            foreach (var entry in entries)
            {
                s += entry.ToString() + "\n";
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
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    string pubkey = rsa.ToXmlString(false);

                    byte[] bytes = RSAEncrypt(Encoding.UTF8.GetBytes(s), pubkey);

                    stream.Write(bytes, 0, bytes.Length);
                    break;
                }
            }
        }

        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
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
