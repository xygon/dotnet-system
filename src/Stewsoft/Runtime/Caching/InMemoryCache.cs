using System;
using System.Collections.Concurrent;

namespace Stewsoft.Runtime.Caching
{
    public abstract class InMemoryCache : ICache
    {
        public interface ICacheItem
        {
            TValue GetValue<TValue>();
            bool HasExpired();
        }

        private readonly ConcurrentDictionary<object, ICacheItem> _data = new ConcurrentDictionary<object, ICacheItem>();
        private readonly Func<object, int, ICacheItem> _cacheItemCreator;
        private readonly int _defaultTtlSeconds;

        protected InMemoryCache(Func<object, int, ICacheItem> cacheItemCreator) : this(cacheItemCreator, 0)
        {
            
        }
        protected InMemoryCache(Func<object,int,ICacheItem> cacheItemCreator, int defaultTtlSeconds)
        {
            if (cacheItemCreator==null) throw new ArgumentNullException(nameof(cacheItemCreator));

            _cacheItemCreator = cacheItemCreator;
            _defaultTtlSeconds = defaultTtlSeconds > 0 ? defaultTtlSeconds : int.MaxValue;

            // TODO: Start thread to periodically evict expired entries
        }

        public void Set<TKey, TValue>(TKey key, TValue value, int? ttlSeconds)
        {
            var item = _cacheItemCreator(value, ttlSeconds ?? _defaultTtlSeconds);
            _data.AddOrUpdate(key, item, (k, v) => item);
        }

        public TValue Get<TKey, TValue>(TKey key, Func<TKey, TValue> missing = null)
        {
            TValue value;
            if (TryGetValue(key, out value))
            {
                return value;
            }
            return missing != null ? missing(key) : default(TValue);
        }

        public TValue GetOrAdd<TKey, TValue>(TKey key, Func<TKey, TValue> add, int? ttlSeconds)
        {
            var created = false;
            var item = _data.GetOrAdd(key, k =>
            {
                created = true;
                return _cacheItemCreator(add((TKey) k), ttlSeconds ?? _defaultTtlSeconds);
            });
            if (created || !item.HasExpired())
            {
                return item.GetValue<TValue>();
            }
            item = _cacheItemCreator(add(key), ttlSeconds ?? _defaultTtlSeconds);
            item = _data.AddOrUpdate(key, item, (k, v) => item);
            return item.GetValue<TValue>();
        }

        public bool TryGetValue<TKey, TValue>(TKey key, out TValue value)
        {
            ICacheItem item;
            if (_data.TryGetValue(key, out item) && !item.HasExpired())
            {
                value = item.GetValue<TValue>();
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue AddOrUpdate<TKey, TValue>(TKey key, Func<TKey, TValue> add, Func<TKey, TValue, TValue> update,
            int? ttlSeconds)
        {
            var item = _data.AddOrUpdate(key,
                k => _cacheItemCreator(add((TKey) k), ttlSeconds ?? _defaultTtlSeconds),
                (k, v) =>
                    v.HasExpired()
                        ? _cacheItemCreator(add((TKey) k), ttlSeconds ?? _defaultTtlSeconds)
                        : _cacheItemCreator(update((TKey) k, v.GetValue<TValue>()), ttlSeconds ?? _defaultTtlSeconds));
            return item.GetValue<TValue>();
        }


    }
}