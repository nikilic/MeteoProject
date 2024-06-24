namespace Projekat3;

public static class Cache {
    private static readonly ReaderWriterLockSlim CacheLock = new();
    private static readonly Dictionary<string, CacheItem> CacheDict = new();

    public static bool Contains(string key){
        CacheLock.EnterReadLock();
        try{
            if (CacheDict.TryGetValue(key, out var cacheItem)){
                if (cacheItem.ExpirationTime < DateTime.Now){
                    CacheDict.Remove(key);
                    return false;
                }
                return true;
            }
            return false;
        }
        finally{
            CacheLock.ExitReadLock();
        }
    }
    
    public static DailyWeather? ReadFromCache(string key){
        CacheLock.EnterReadLock();
        try{
            if (CacheDict.TryGetValue(key, out var cacheItem)){
                if (cacheItem.ExpirationTime < DateTime.Now){
                    CacheDict.Remove(key);
                    return null;
                }
                return cacheItem.Value;
            }
            return null;
        }
        finally{
            CacheLock.ExitReadLock();
        }
    }

    public static void WriteToCache(string key, DailyWeather? value){
        CacheLock.EnterWriteLock();
        try{
            var expirationTime = DateTime.Now.AddHours(1);
            CacheDict[key] = new CacheItem(value, expirationTime);
        }
        finally{
            CacheLock.ExitWriteLock();
        }
    }
    
    private class CacheItem {
        public DailyWeather? Value { get; }
        public DateTime ExpirationTime { get; }

        public CacheItem(DailyWeather? value, DateTime expirationTime){
            Value = value;
            ExpirationTime = expirationTime;
        }
    }
}