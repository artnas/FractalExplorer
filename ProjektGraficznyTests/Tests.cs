using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjektGraficzny;
using Fractals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Alea;
using Alea.Parallel;
using NamedPipeWrapper;

namespace Tests
{
    [TestClass()]
    public class PerformanceLoggerTests
    {
        [TestMethod()]
        public void EncryptionTest()
        {
            // dlugosc stringa nie moze byc zbyt duza
            string originalString = "qwerty1234567890";

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string pubkey = rsa.ToXmlString(false);
            string prikey = rsa.ToXmlString(true);

            byte[] encrypted = PerformanceLogger.RSAEncrypt(Encoding.Unicode.GetBytes(originalString), pubkey);
            byte[] decrypted = PerformanceLogger.RSADecrypt(encrypted, prikey);

            string processedString = Encoding.Unicode.GetString(decrypted);

            Assert.AreEqual(originalString, processedString);
        }

        [TestMethod()]
        public void CompressionTest()
        {
            string originalString = "qwerty1234567890";
            byte[] bytes = Encoding.UTF8.GetBytes(originalString);

            byte[] compressed = PerformanceLogger.Zip(originalString);
            byte[] decompressed = Encoding.UTF8.GetBytes(PerformanceLogger.Unzip(compressed));

            //Console.WriteLine(Encoding.UTF8.GetString(bytes));
            //Console.WriteLine(Encoding.UTF8.GetString(decompressed));

            CollectionAssert.AreEqual(bytes, decompressed);
        }
    }

    [TestClass()]
    public class FractalTests
    {
        [TestMethod()]
        public void JuliaFractalTest()
        {
            byte[] a = new byte[400 * 300 * 4];
            byte[] b = new byte[400 * 300 * 4];
            byte[] c = new byte[400 * 300 * 4];

            Utils.BuildIterationColorsCache();

            Julia juliaFractal_A = new Julia(a, 400, 300, 60);
            juliaFractal_A.DrawOnSingleThread(0, 0, 1);

            Julia juliaFractal_B = new Julia(b, 400, 300, 60);
            juliaFractal_B.DrawOnMultipleThreads(0, 0, 1);

            CollectionAssert.AreEqual(a, b);

            Julia juliaFractal_C = new Julia(c, 400, 300, 60);
            juliaFractal_C.DrawOnGpu(0, 0, 1);
      
            CollectionAssert.AreEqual(a, c);
        }

        [TestMethod()]
        public void MandelbrotFractalTest()
        {
            byte[] a = new byte[400 * 300 * 4];
            byte[] b = new byte[400 * 300 * 4];
            byte[] c = new byte[400 * 300 * 4];

            Utils.BuildIterationColorsCache();

            Mandelbrot mandelbrotFractal_A = new Mandelbrot(a, 400, 300, 60);
            mandelbrotFractal_A.DrawOnSingleThread(0, 0, 1);

            Mandelbrot mandelbrotFractal_B = new Mandelbrot(b, 400, 300, 60);
            mandelbrotFractal_B.DrawOnMultipleThreads(0, 0, 1);

            CollectionAssert.AreEqual(a, b);

            Mandelbrot mandelbrotFractal_C = new Mandelbrot(c, 400, 300, 60);
            mandelbrotFractal_C.DrawOnGpu(0, 0, 1);
            
            CollectionAssert.AreEqual(a, c);
        }
    }

    [TestClass()]
    public class NamedPipeTests
    {
        [TestMethod()]
        public void NamedPipeTest()
        {
            string a = "abc";
            string b = "123";

            // serwer wysyla do klienta "abc"
            // klient weryfikuje, ze otrzymal "abc"
            // klient odsyla do serwera "123"
            // serwer weryfikuje, ze otrzymal "123"

            var pipeServer = new NamedPipeServer<PipeMessage>("testtesttest");         

            pipeServer.ClientMessage += delegate (NamedPipeConnection<PipeMessage, PipeMessage> conn, PipeMessage message)
            {
                Assert.AreEqual(b, message.content);
            };

            pipeServer.Start();

            var pipeClient = new NamedPipeClient<PipeMessage>("testtesttest");

            pipeClient.ServerMessage += delegate (NamedPipeConnection<PipeMessage, PipeMessage> conn, PipeMessage message)
            {
                Assert.AreEqual(a, message.content);

                pipeClient.PushMessage(new PipeMessage() { messageType = 0, content = b });
            };

            pipeClient.Start();

            pipeServer.PushMessage(new PipeMessage() { messageType = 0, content = a });
        }
    }
}