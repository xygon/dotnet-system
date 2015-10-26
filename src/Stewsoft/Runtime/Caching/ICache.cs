using System;
using System.Security.Cryptography.X509Certificates;

namespace Stewsoft.Runtime.Caching
{
    /// <summary>
    /// Interface for a generic cache
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Set a value in the cache, replacing any value for the specified <paramref name="key"/> if one already exists.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to be stored in the cache.</param>
        /// <param name="ttlSeconds">The "time-to-live" for the cache item (in seconds).</param>
        void Set<TKey, TValue>(TKey key, TValue value, int? ttlSeconds);

        /// <summary>
        /// Get the value associated with the <paramref name="key"/>, calling the optional <paramref name="missing"/> function if no entry with the <paramref name="key"/> is found.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="missing">A function that provides the value of the <paramref name="key"/> has no entry in the cache.</param>
        /// <returns>The value associated with the <paramref name="key"/> or the value returned by the <paramref name="missing"/> function if there is no value for the <see cref="key"/> in the cache.</returns>
        /// <returns>
        /// The value for the <paramref name="key"/>.
        /// This will either be the existing value for the key if there is already an entry for that key, 
        /// or the new value obtained by calling the <paramref name="add"/> function (which will NOT have been added to the cache).
        /// </returns>
        TValue Get<TKey, TValue>(TKey key, Func<TKey, TValue> missing = null);

        /// <summary>
        /// Get the value associated with the <paramref name="key"/>, calling the <paramref name="add"/> function if no entry with the <paramref name="key"/> is found.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="add">A function that provides the value of the <paramref name="key"/> has no entry in the cache.</param>
        /// <param name="ttlSeconds">The "time-to-live" for the cache item (in seconds).</param>
        /// <returns>
        /// The value for the <paramref name="key"/>.
        /// This will either be the existing value for the key if there is already an entry for that key, 
        /// or the new value obtained by calling the <paramref name="add"/> function (which will have been added to the cache).
        /// </returns>
        TValue GetOrAdd<TKey, TValue>(TKey key, Func<TKey, TValue> add, int? ttlSeconds);

        /// <summary>
        /// Attempts to get the value for the <paramref name="key"/> from the cache.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value obtained from the cache.</param>
        /// <returns><c>true</c> if and entry is found; otherwise <c>false</c>.</returns>
        bool TryGetValue<TKey, TValue>(TKey key, out TValue value);

        /// <summary>
        /// Adds an entry to the cahce if no entry for the <paramref name="key"/> exists; otherwise updates the entry with the result of the <paramref name="update"/> function.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="add">A function that provides the value of the <paramref name="key"/> has no entry in the cache.</param>
        /// <param name="update">A function that provides the new value of the <paramref name="key"/>.</param>
        /// <param name="ttlSeconds">The "time-to-live" for the cache item (in seconds).</param>
        /// <returns>
        /// The resulting value of the key.
        /// This will either be the result of calling <paramref name="add"/> if the key was not already in the cache, or <paramref name="update"/> if the key was already in the cache.
        /// </returns>
        TValue AddOrUpdate<TKey, TValue>(TKey key, Func<TKey, TValue> add, Func<TKey, TValue, TValue> update,
            int? ttlSeconds);
    }
}