using System.Collections.Generic;

namespace OpenQA.Selenium.Extensions
{
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// apply or replace a single key/value pair in the current IDictionary implementation
        /// </summary>
        /// <typeparam name="TKey">key type</typeparam>
        /// <typeparam name="TValue">value type</typeparam>
        /// <param name="dictionary">key/value pairs (collection) to add/replace the new entry to</param>
        /// <param name="key">key to add/replace</param>
        /// <param name="value">value to add/replace</param>
        public static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            // if key exists - override
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }

            // create new if not exists
            dictionary.Add(key, value);
        }
    }
}
