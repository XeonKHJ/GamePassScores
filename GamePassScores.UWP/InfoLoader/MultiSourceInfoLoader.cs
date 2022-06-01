using GamePassScores.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace GamePassScores.UWP.InfoLoader
{
    internal class MultiSourceInfoLoader : CachedGeneralInfoLoader
    {
        public Dictionary<Uri, InfoProviderContext> InfoProviderUrls { get; } = new Dictionary<Uri, InfoProviderContext>();
        public MultiSourceInfoLoader(string fileName, Dictionary<Uri, InfoProviderContext> infoProviderUrls) : base(fileName)
        {
            foreach (var pair in infoProviderUrls)
            {
                InfoProviderUrls.Add(pair.Key, pair.Value);
            }
        }

        protected async override Task<List<Game>> LoadInfoFromCacheAsync()
        {
            var downloadedJsonFile = new FileInfo(Path.Combine(ApplicationData.Current.LocalFolder.Path, _fileName));
            StorageFile jsonFile = null;
            if (downloadedJsonFile.Exists)
            {
                jsonFile = await StorageFile.GetFileFromPathAsync(ApplicationData.Current.LocalFolder.Path + "\\games.json");
            }
            else
            {
                jsonFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + _fileName));
            }

            var jsonString = await FileIO.ReadTextAsync(jsonFile);
            List<Game> games = new List<Game>();
            try
            {
                games = JsonConvert.DeserializeObject<List<Game>>(jsonString);
            }
            catch (Exception exception)
            {
                throw exception;
                System.Diagnostics.Debug.WriteLine(exception);
            }

            return games;
        }

        protected override async Task DownloadAndCacheInfo()
        {
            await GetInfoAsync();
        }

        private async Task GetInfoAsync()
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

        private bool _isFileFetched = false;
        private StorageFile _file = null;
        public async void GetInfo(Uri uri, System.Threading.Mutex fileAccessMutex, Semaphore forLoopSemaphore)
        {
            forLoopSemaphore.WaitOne();
            System.Diagnostics.Debug.WriteLine("正在请求{0}。", uri);
            try
            {
                var httpClient = new Windows.Web.Http.HttpClient();

                IBuffer buffer = await httpClient.GetBufferAsync(uri).AsTask().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine("shiiiiiiiiiiiiiit");
                fileAccessMutex.WaitOne();
                if (buffer != null && !_isFileFetched)
                {
                    _isFileFetched = true;
                    fileAccessMutex.ReleaseMutex();
                    _file = await ApplicationData.Current.LocalFolder.CreateFileAsync(_fileName, CreationCollisionOption.ReplaceExisting);
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

        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}
