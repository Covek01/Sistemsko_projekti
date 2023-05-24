using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Runtime;
using System.Net;
using System.Runtime.Caching;

namespace Projekat_2
{
    public class CacheLocked
    {
        public MemoryCache cache;
        private CacheItemPolicy policy;
        public ReaderWriterLockSlim cacheLocker;
        
        public CacheLocked(double seconds = 120.0)
        {
            cache = new MemoryCache("File cache");
            policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(seconds);

            cacheLocker = new ReaderWriterLockSlim();
        }

        public void AddSafe(string fileName, object buffer)
        {
            cacheLocker.EnterWriteLock();
            try
            {
                CacheItem item = new CacheItem(fileName, buffer);

                cache.Add(item, policy);
            }
            finally
            {
                cacheLocker.ExitWriteLock();
            }
           
        }

        public object ReadSafe(string key)
        {
            object buffer;
            cacheLocker.EnterReadLock();
            try
            {
                buffer = cache[key];
            }
            finally
            {
                cacheLocker.ExitReadLock();
            }

            return buffer;
        }

        public bool Contains(string key)
        {
            return cache.Contains(key);
        }
    }
}