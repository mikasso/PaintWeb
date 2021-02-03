using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RawStorageLibrary;
namespace Paintweb
{
    public static class LineDataController 
    {
        private const string FilePath = "../../../lines_data.dat";
        private static ByteRawStorage storage = new ByteRawStorage(FilePath, JSONSize, 1000);
        private static readonly char [] buffer = new char [JSONSize];
        const int JSONSize = 150;
        public static void SaveLine(string msg)
        {
            for(int i=0;i<JSONSize;i++)
            {
                buffer[i] = ' ';
            }

            msg.ToCharArray().CopyTo(buffer,0);
            storage.Add(Encoding.UTF8.GetBytes(buffer));
        }

        public static IEnumerable<string> GetData(string user)
        {
            string jsonText;
            foreach(var bytes in storage.GetAllData(JSONSize))
            {
                jsonText = Encoding.UTF8.GetString(bytes);
                yield return jsonText.Trim();
            }
        }

        public static void ClearData(string user)
        {
            storage = new ByteRawStorage(FilePath, JSONSize, 10000);
        }

    }
}
