using Microsoft.VisualStudio.TestTools.UnitTesting;
using RawStorageLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace RawStorageLibrary.Tests
{
    [TestClass()]
    public class ByteRawStorageTests
    {
        private static Random random = new Random();
    

        [TestMethod()]
        public void ByteRawStorageTest()
        {
            const string path = "temp_1";
            ByteRawStorage storage = new ByteRawStorage(path, 100, 100, true);
            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod()]
        public void DisposeTest()
        {
            const string path = "temp_2";
            ByteRawStorage storage = new ByteRawStorage(path, 100, 100, true);
            storage.Dispose();
            Assert.IsFalse(File.Exists(path));
        }

        [TestMethod()]
        public void AddGetTestOnlyRam()
        {
            const string path = "temp_3";
            string text = "Hello World!";
            byte[] data = Encoding.UTF8.GetBytes(text);
            ByteRawStorage storage = new ByteRawStorage(path, data.Length, 3, true);
            for (int i = 0; i < 3; i++)
                storage.Add(data);
            foreach (var d in storage.GetAllData(data.Length))
            {
                Assert.IsTrue(d.SequenceEqual(data));
            }
        }

        [TestMethod()]
        public void AddGetTestDiskAndRam()
        {
            const string path = "temp_4";
            string text = "Hello World!";
            byte[] data = Encoding.UTF8.GetBytes(text);
            ByteRawStorage storage = new ByteRawStorage(path, data.Length, 1, true);
            for (int i = 0; i < 3; i++)
                storage.Add(data);
            foreach (var d in storage.GetAllData(data.Length))
            {
                Assert.IsTrue(d.SequenceEqual(data));
            }
        }

    }
}