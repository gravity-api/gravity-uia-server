/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * 2019-02-07
 *    - modify: better xml comments
 */
using System.Collections.Generic;

namespace UiaDriverServer.Extensions
{
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// add or replace existing key/value pair
        /// </summary>
        /// <typeparam name="TKey">key type to add or replace</typeparam>
        /// <typeparam name="TValue">value type to add or replace</typeparam>
        /// <param name="d">dictionary on which to perform add/replace</param>
        /// <param name="key">key to add or replace</param>
        /// <param name="value">value to add or replace</param>
        /// <returns>update dictionary</returns>
        public static IDictionary<TKey, TValue> AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> d, TKey key, TValue value)
        {
            if (d.ContainsKey(key))
            {
                d[key] = value;
                return d;
            }
            d.Add(key, value);
            return d;
        }
    }
}