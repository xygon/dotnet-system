using System;

namespace Stewsoft.Runtime.Caching
{
    public sealed class InMemeoryReferenceCache : InMemoryCache
    {
        public InMemeoryReferenceCache() :
            base((value, ttlSeconds) => new CacheItem(value, ttlSeconds))
        {

        }

        class CacheItem : ICacheItem
        {
            public CacheItem(object value, int ttlSeconds)
            {
                _value = value;
                _expiryTime = DateTime.UtcNow + TimeSpan.FromSeconds(ttlSeconds);
            }

            private readonly object _value;
            private readonly DateTime _expiryTime;

            public TValue GetValue<TValue>()
            {
                return (TValue)_value;
            }

            public bool HasExpired()
            {
                return DateTime.UtcNow > _expiryTime;
            }
        }

    }
}