using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace GamePassScores.UWP
{
    public class InfoProviderContext
    {
        public bool IsCompressed { set; get; } = true;
    }
    public class InfoFetcher
    {
        public Dictionary<Uri, InfoProviderContext> InfoProviderUrls { get; } = new Dictionary<Uri, InfoProviderContext>();
        public InfoFetcher(Dictionary<Uri, InfoProviderContext> infoProviderUrls)
        {
            foreach(var pair in infoProviderUrls)
            {
                InfoProviderUrls.Add(pair.Key, pair.Value);
            }
        }

        public async Task GetInfoAsync()
        {
            var httpClient = new Windows.Web.Http.HttpClient();

            try
            {
                IBuffer buffer = null;
                StorageFile file = null;

                foreach (var uri in InfoProviderUrls.Keys)
                {
                    try
                    {
                        buffer = await httpClient.GetBufferAsync(uri);
                        if (buffer != null)
                        {
                            file = await ApplicationData.Current.LocalFolder.CreateFileAsync("games.json", CreationCollisionOption.ReplaceExisting);
                            byte[] compressedData = new byte[buffer.Length];

                            if(InfoProviderUrls[uri].IsCompressed)
                            {
                                using (DataReader dataReader = DataReader.FromBuffer(buffer))
                                {
                                    dataReader.ReadBytes(compressedData);
                                    string infos = Unzip(compressedData);
                                    await FileIO.WriteTextAsync(file, infos);
                                }
                            }
                            else
                            {
                                await FileIO.WriteBufferAsync(file, buffer);
                            }
                            System.Diagnostics.Debug.WriteLine("从源{0}更新成功。", uri.AbsoluteUri);
                        }
                        else
                        {
                            throw new Exception();
                        }

                        break;
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Fetching from source {0} failed!", uri.AbsoluteUri);
                        if(uri == InfoProviderUrls.Keys.Last())
                        {
                            throw ex;
                        }
                    }
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
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
