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
using System.Threading;

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
            foreach (var pair in infoProviderUrls)
            {
                InfoProviderUrls.Add(pair.Key, pair.Value);
            }
        }

        bool _isFileFetched = false;
        StorageFile _file;
        public async void GetInfo(Uri uri, System.Threading.Mutex fileAccessMutex, Semaphore forLoopSemaphore)
        {
            forLoopSemaphore.WaitOne();
            System.Diagnostics.Debug.WriteLine("正在请求{0}。", uri);
            try
            {
                var httpClient = new Windows.Web.Http.HttpClient();

                IBuffer buffer = await httpClient.GetBufferAsync(uri).AsTask().ConfigureAwait(false) ;
                System.Diagnostics.Debug.WriteLine("shiiiiiiiiiiiiiit");
                fileAccessMutex.WaitOne();
                if (buffer != null && !_isFileFetched)
                {
                    _isFileFetched = true;
                    fileAccessMutex.ReleaseMutex();
                    _file = await ApplicationData.Current.LocalFolder.CreateFileAsync("games.json", CreationCollisionOption.ReplaceExisting);
                    byte[] compressedData = new byte[buffer.Length];

                    if (InfoProviderUrls[uri].IsCompressed)
                    {
                        using (DataReader dataReader = DataReader.FromBuffer(buffer))
                        {
                            dataReader.ReadBytes(compressedData);
                            string infos = Unzip(compressedData);
                            await FileIO.WriteTextAsync(_file, infos);
                        }
                    }
                    else
                    {
                        await FileIO.WriteBufferAsync(_file, buffer);
                    }
                    System.Diagnostics.Debug.WriteLine("从源{0}更新成功。", uri.AbsoluteUri);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("没有从源{0}更新。", uri.AbsoluteUri);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("lalalerror。", uri.AbsoluteUri);
            }
            System.Diagnostics.Debug.WriteLine("退出请求{0}。", uri);
            forLoopSemaphore.Release();
        }


        public async Task GetInfoAsync()
        {
            try
            {
                bool isFileRetreived = false;
                System.Threading.Mutex fileAccessMutex = new System.Threading.Mutex();
                Semaphore requestThreadPool = new Semaphore(InfoProviderUrls.Keys.Count, InfoProviderUrls.Keys.Count);


                    foreach (var uri in InfoProviderUrls.Keys)
                    {
                        GetInfo(uri, fileAccessMutex, requestThreadPool);
                    }

                await Task.Run(() =>
                {
                    requestThreadPool.WaitOne();
                });
                
                System.Diagnostics.Debug.WriteLine("outsidetheloop.");
                if (_file == null)
                {
                    throw new Exception("Refresh failed.");
                }
            }
            catch (Exception ex)
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
