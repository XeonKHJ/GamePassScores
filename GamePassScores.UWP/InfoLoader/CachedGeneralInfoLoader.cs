﻿using GamePassScores.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace GamePassScores.UWP.InfoLoader
{
    abstract internal class CachedGeneralInfoLoader : IInfoLoader
    {
        private Mutex _mutex;
        protected string _fileName;
        protected CachedGeneralInfoLoader(string fileName)
        {
            _fileName = fileName;
            _mutex = new Mutex();
        }
        /// <summary>
        /// Load info data from cache file.
        /// </summary>
        /// <returns>Null if there is no cache</returns>
        abstract protected Task<List<Game>> LoadInfoFromCacheAsync();
        abstract protected Task DownloadAndCacheInfo();

        /// <summary>
        /// This functon loads the data from cache, if cache doesn't exsit, then it will load data from internet and save it as cache.
        /// Thread-safe function.
        /// </summary>
        /// <returns>A list of games</returns>
        public async Task<List<Game>> LoadAsync()
        {
            _mutex.WaitOne();
            List<Game> games = null;

            // Check local cache.
            try
            {
                games = await LoadInfoFromCacheAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            // if loading from cache failed, then refresh data and retrive it.
            if (games == null)
            {
                try
                {
                    await DownloadAndCacheInfo();
                    games = await LoadInfoFromCacheAsync();
                }
                catch(Exception ex)
                {
                    _mutex.ReleaseMutex();
                    throw ex;
                }
            }
            _mutex.ReleaseMutex();
            return games;
        }

        /// <summary>
        /// Get info online and cache the info.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Game>> RefreshAsync()
        {
            _mutex.WaitOne();
            List<Game> games = new List<Game>();
            try
            {
                await DownloadAndCacheInfo();
                games = await LoadInfoFromCacheAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            _mutex.ReleaseMutex();
            return games;
        }
    }
}
