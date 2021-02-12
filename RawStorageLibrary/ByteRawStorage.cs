using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RawStorageLibrary
{
    /// <summary>
    /// Allows to storage binary buffors in operation memory and if overload then saved it in File on the disk.
    /// </summary>
    public class ByteRawStorage : IDisposable
    {
        private byte[] RawDataBuffor;
        private readonly string FileDataPath;
        private readonly int Count;
        public bool IsFileTemporary { get; set; }
        private readonly int SizeOfOneBuffor;
        private int Amount { get; set; }
        private int NextIndex { get => Amount * SizeOfOneBuffor; }

        public ByteRawStorage(string path, int sizeOfOneBuffor, int count, bool isFileTemporary = true)
        {
            this.IsFileTemporary = isFileTemporary;
            this.SizeOfOneBuffor = sizeOfOneBuffor;
            this.Count = count;
            FileDataPath = path;
            File.Create(FileDataPath).Close();
            RawDataBuffor = new byte[SizeOfOneBuffor * Count];
        }
        public void Dispose()
        {
            if(IsFileTemporary)
                File.Delete(FileDataPath);
        }

        /// <summary>
        /// Add "data" buffor to storage. It may cause saving data buffor on disk.
        /// </summary>
        /// <param name="data"> bytes buffor to add. Must have size equal SizeOfOneBuffor </param>
        public void Add(byte[] data)
        {
            if (data.Length != SizeOfOneBuffor)
                throw new Exception("Not eqaul size to SizeOfOneBuffor \n Are you trying to add diffrent type of buffor?");
            if (Amount >= Count)
            {
                SaveDataOnDisk();
                Amount = 0;
            }
            data.CopyTo(RawDataBuffor, NextIndex);
            Amount++;
        }

        /// <summary>
        /// Retrieves all data saved in storage (disk memory and RAM).
        /// </summary>
        /// <param name="size">Size of buffor, in which data will be returned </param>
        /// <returns> Byte arrays of data from storage </returns>
        public IEnumerable<byte[]> GetAllData(int size)
        {
            foreach (var v in GetAllDataFromDisk(size))
                if(v != null && v.Length != 0)
                    yield return v;
            int offset = 0;
            for (offset = 0; offset < NextIndex; offset += size)
            {
                yield return new ArraySegment<byte>(RawDataBuffor, offset, size).ToArray();
            }
            //Return last slice
            offset = offset - size;
            int rest = NextIndex - offset;
            if (rest > 0 && offset > 0)
                yield return new ArraySegment<byte>(RawDataBuffor, offset, rest).ToArray();
        }

        private IEnumerable<byte[]> GetAllDataFromDisk(int size)
        {
            FileStream fs = new FileStream(FileDataPath, FileMode.Open);
            fs.Seek(0, SeekOrigin.Begin);
            byte[] buffor = new byte[size];
            int multipleOfBufSize = (int)fs.Length / size;
            int offset = 0;
            for (offset = 0; offset < multipleOfBufSize * size; offset += size)
            {
                try
                {
                    fs.Read(buffor, 0, size);
                }
                catch (Exception) 
                { 
                    fs.Close();
                    yield break;
                    throw;
                }
                yield return buffor;
            }
            int rest = (int)fs.Length - offset;
            buffor = new byte[rest];
            fs.Read(buffor, 0, rest);
            yield return buffor;
            fs.Close();
        }
        private void SaveDataOnDisk()
        {
            FileStream fs = new FileStream(FileDataPath, FileMode.Append);
            try
            {
                fs.Write(RawDataBuffor,0,RawDataBuffor.Length);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                fs.Close();
            }
        }
    }
}
