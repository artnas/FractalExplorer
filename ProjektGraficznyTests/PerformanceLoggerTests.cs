using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjektGraficzny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProjektGraficzny.Tests
{
    [TestClass()]
    public class PerformanceLoggerTests
    {
        [TestMethod()]
        public void EncryptionTest()
        {
            string originalString = "Test 123 ABC";

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
}