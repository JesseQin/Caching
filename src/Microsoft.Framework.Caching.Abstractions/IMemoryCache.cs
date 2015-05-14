// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Framework.Caching.Memory
{
    public interface IMemoryCache : IDisposable
    {
        IEntryLink CreateLinkingScope();

        /// <summary>
        /// Create or overwrite an entry in the cache.
        /// </summary>
        /// <param name="key">A string identifying the entry. This is case sensitive.</param>
        /// <param name="value">The value to be cached.</param>
        /// <param name="link">An entry link used to collect trigger and expiration information from this entry
        /// for use in a parent entry.</param>
        /// <param name="options">The <see cref="CacheEntryOptions"/>.</param>
        /// <returns>The object that was cached.</returns>
        object Set(string key, object value, IEntryLink link, CacheEntryOptions options);

        /// <summary>
        /// Gets the item associated with this key if present.
        /// </summary>
        /// <param name="key">A string identifying the requested entry.</param>
        /// <param name="value">The located value or null.</param>
        /// <returns>True if the key was found.</returns>
        bool TryGetValue(string key, out object value);

        /// <summary>
        /// Removes the object associated with the given key.
        /// </summary>
        /// <param name="key">A string identifying the entry. This is case sensitive.</param>
        void Remove(string key);
    }
}