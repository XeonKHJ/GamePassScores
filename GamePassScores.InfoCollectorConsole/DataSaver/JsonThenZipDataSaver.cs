using GamePassScores.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamePassScores.InfoCollectorConsole.DataSaver
{
    /// <summary>
    /// This saver save games' info data into json file and then compresse it into a zip file.
    /// The original json file remains.
    /// </summary>
    internal class JsonThenZipDataSaver : IDataSaver
    {
        private string _jsonFilePath;
        private string _zipFilePath;
        public JsonThenZipDataSaver(string jsonFilePath, string zipFilePath)
        {
            _jsonFilePath = jsonFilePath;
            _zipFilePath = zipFilePath;
        }
        public async Task SaveAsync(IList<Game> games)
        {
            var fileContent = JsonConvert.SerializeObject(games);

            Console.WriteLine("Start processing repo.");

            Console.WriteLine("New info file path is {0}", _jsonFilePath);
            Console.WriteLine("New compressed info file path is {0}", _zipFilePath);

            await File.WriteAllTextAsync(_jsonFilePath, fileContent);
            var compressedSerializeGames = Zip(fileContent);
            await File.WriteAllBytesAsync(_zipFilePath, compressedSerializeGames);
        }

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
        private static byte[] Zip(string str)
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

        private static string Unzip(byte[] bytes)
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
    }
}
