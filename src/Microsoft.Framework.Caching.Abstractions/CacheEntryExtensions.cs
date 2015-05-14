// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Internal;

namespace Microsoft.Framework.Caching.Memory
{
    public static class CacheEntryExtensions
    {
        /// <summary>
        /// Sets the priority for keeping the cache entry in the cache during a memory pressure triggered cleanup.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="priority"></param>
        public static CacheEntryOptions SetPriority(this CacheEntryOptions options, CacheItemPriority priority)
        {
            options.Priority = priority;
            return options;
        }

        /// <summary>
        /// Expire the cache entry if the given event occurs.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="trigger"></param>
        public static CacheEntryOptions AddExpirationTrigger(
            this CacheEntryOptions options,
            [NotNull] IExpirationTrigger trigger)
        {
            options.Triggers.Add(trigger);
            return options;
        }

        /// <summary>
        /// Sets an absolute expiration time, relative to now.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="relative"></param>
        public static CacheEntryOptions SetAbsoluteExpiration(this CacheEntryOptions options, TimeSpan relative)
        {
            options.AbsoluteExpirationRelativeToNow = relative;
            return options;
        }

        /// <summary>
        /// Sets an absolute expiration date for the cache entry.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="absolute"></param>
        public static CacheEntryOptions SetAbsoluteExpiration(this CacheEntryOptions options, DateTimeOffset absolute)
        {
            options.AbsoluteExpiration = absolute;
            return options;
        }

        /// <summary>
        /// Sets how long the cache entry can be inactive (e.g. not accessed) before it will be removed.
        /// This will not extend the entry lifetime beyond the absolute expiration (if set).
        /// </summary>
        /// <param name="options"></param>
        /// <param name="offset"></param>
        public static CacheEntryOptions SetSlidingExpiration(this CacheEntryOptions options, TimeSpan offset)
        {
            options.SlidingExpiration = offset;
            return options;
        }

        /// <summary>
        /// The given callback will be fired after the cache entry is evicted from the cache.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public static CacheEntryOptions RegisterPostEvictionCallback(
            this CacheEntryOptions options,
            [NotNull] PostEvictionDelegate callback)
        {
            return options.RegisterPostEvictionCallback(callback, state: null);
        }

        /// <summary>
        /// The given callback will be fired after the cache entry is evicted from the cache.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public static CacheEntryOptions RegisterPostEvictionCallback(
            this CacheEntryOptions options,
            [NotNull] PostEvictionDelegate callback,
            object state)
        {
            options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration()
            {
                EvictionCallback = callback,
                State = state
            });
            return options;
        }

        /// <summary>
        /// Adds inherited trigger and absolute expiration information.
        /// </summary>
        /// <param name="link"></param>
        public static CacheEntryOptions AddEntryLink(this CacheEntryOptions options, IEntryLink link)
        {
            foreach (var trigger in link.Triggers)
            {
                options.AddExpirationTrigger(trigger);
            }

            if (link.AbsoluteExpiration.HasValue)
            {
                options.SetAbsoluteExpiration(link.AbsoluteExpiration.Value);
            }
            return options;
        }
    }
}
