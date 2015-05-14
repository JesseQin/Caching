// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Framework.Caching.Memory.Infrastructure;
using Xunit;

namespace Microsoft.Framework.Caching.Memory
{
    public class EntryLinkExpirationTests
    {
        private IMemoryCache CreateCache()
        {
            return CreateCache(new SystemClock());
        }

        private IMemoryCache CreateCache(ISystemClock clock)
        {
            return new MemoryCache(new MemoryCacheOptions()
            {
                Clock = clock,
                CompactOnMemoryPressure = false,
            });
        }

        [Fact]
        public void GetWithLinkPopulatesTriggers()
        {
            var cache = CreateCache();
            var obj = new object();
            string key = "myKey";
            string key1 = "myKey1";

            var link = new EntryLink();

            var trigger = new TestTrigger() { ActiveExpirationCallbacks = true };
            cache.Set(key, obj, link, new CacheEntryOptions().AddExpirationTrigger(trigger));

            Assert.Equal(1, link.Triggers.Count());
            Assert.Null(link.AbsoluteExpiration);

            //cache.Set(key: key1, value: obj, options: new CacheEntryOptions().AddEntryLink(link));
        }

        [Fact]
        public void GetWithLinkPopulatesAbsoluteExpiration()
        {
            var cache = CreateCache();
            var obj = new object();
            string key = "myKey";
            string key1 = "myKey1";

            var link = new EntryLink();

            var trigger = new TestTrigger() { ActiveExpirationCallbacks = true };
            var time = new DateTimeOffset(2051, 1, 1, 1, 1, 1, TimeSpan.Zero);
            cache.Set(key, obj, link, new CacheEntryOptions().SetAbsoluteExpiration(time));

            Assert.Equal(0, link.Triggers.Count());
            Assert.NotNull(link.AbsoluteExpiration);
            Assert.Equal(time, link.AbsoluteExpiration);

            //cache.Set(key1, obj, new CacheEntryOptions().AddEntryLink(link));
        }

        [Fact]
        public void TriggerExpiresLinkedEntry()
        {
            var cache = CreateCache();
            var obj = new object();
            string key = "myKey";
            string key1 = "myKey1";

            var link = new EntryLink();

            var trigger = new TestTrigger() { ActiveExpirationCallbacks = true };
            cache.Set(key, obj, link, new CacheEntryOptions().AddExpirationTrigger(trigger));

            cache.Set(key1, obj, new CacheEntryOptions().AddEntryLink(link));

            Assert.StrictEqual(obj, cache.Get(key));
            Assert.StrictEqual(obj, cache.Get(key1));

            trigger.Fire();

            object value;
            Assert.False(cache.TryGetValue(key1, out value));
            Assert.False(cache.TryGetValue(key, out value));
        }

        [Fact]
        public void AbsoluteExpirationWorksAcrossLink()
        {
            var clock = new TestClock();
            var cache = CreateCache(clock);
            var obj = new object();
            string key = "myKey";
            string key1 = "myKey1";

            var link = new EntryLink();

            var trigger = new TestTrigger() { ActiveExpirationCallbacks = true };
            cache.Set(key, obj, link, new CacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(5)));

            cache.Set(key1, obj, new CacheEntryOptions().AddEntryLink(link));

            Assert.StrictEqual(obj, cache.Get(key));
            Assert.StrictEqual(obj, cache.Get(key1));

            clock.Add(TimeSpan.FromSeconds(10));

            object value;
            Assert.False(cache.TryGetValue(key1, out value));
            Assert.False(cache.TryGetValue(key, out value));
        }

        [Fact]
        public void GetWithImplicitLinkPopulatesTriggers()
        {
            var cache = CreateCache();
            var obj = new object();
            string key = "myKey";
            string key1 = "myKey1";

            Assert.Null(EntryLinkHelpers.ContextLink);

            IEntryLink link;
            using (link = cache.CreateLinkingScope())
            {
                Assert.StrictEqual(link, EntryLinkHelpers.ContextLink);
                var trigger = new TestTrigger() { ActiveExpirationCallbacks = true };
                cache.Set(key, obj, new CacheEntryOptions().AddExpirationTrigger(trigger));
            }

            Assert.Null(EntryLinkHelpers.ContextLink);

            Assert.Equal(1, link.Triggers.Count());
            Assert.Null(link.AbsoluteExpiration);

            cache.Set(key1, obj, new CacheEntryOptions().AddEntryLink(link));
        }

        [Fact]
        public void LinkContextsCanNest()
        {
            var cache = CreateCache();
            var obj = new object();
            string key = "myKey";
            string key1 = "myKey1";

            Assert.Null(EntryLinkHelpers.ContextLink);

            System.Diagnostics.Debugger.Launch();

            IEntryLink link1;
            IEntryLink link2;
            using (link1 = cache.CreateLinkingScope())
            {
                Assert.StrictEqual(link1, EntryLinkHelpers.ContextLink);

                using (link2 = cache.CreateLinkingScope())
                {
                    Assert.StrictEqual(link2, EntryLinkHelpers.ContextLink);

                    var trigger = new TestTrigger() { ActiveExpirationCallbacks = true };
                    cache.Set(key, obj, new CacheEntryOptions().AddExpirationTrigger(trigger));
                }

                Assert.StrictEqual(link1, EntryLinkHelpers.ContextLink);
            }

            Assert.Null(EntryLinkHelpers.ContextLink);

            Assert.Equal(0, link1.Triggers.Count());
            Assert.Null(link1.AbsoluteExpiration);
            Assert.Equal(1, link2.Triggers.Count());
            Assert.Null(link2.AbsoluteExpiration);

            cache.Set(key1, obj, new CacheEntryOptions().AddEntryLink(link2));
        }

        [Fact]
        public void NestedLinkContextsCanAggregate()
        {
            var clock = new TestClock();
            var cache = CreateCache(clock);
            var obj = new object();
            string key1 = "myKey1";
            string key2 = "myKey2";
            string key3 = "myKey3";

            var trigger2 = new TestTrigger() { ActiveExpirationCallbacks = true };
            var trigger3 = new TestTrigger() { ActiveExpirationCallbacks = true };

            IEntryLink link1 = null;
            IEntryLink link2 = null;
            //cache.GetOrSet(key1, context1 =>
            //{
            //    using (link1 = cache.CreateLinkingScope())
            //    {
            //        cache.GetOrSet(key2, context2 =>
            //        {
            //            context2.AddExpirationTrigger(trigger2);
            //            context2.SetAbsoluteExpiration(TimeSpan.FromSeconds(10));

            //            using (link2 = cache.CreateLinkingScope())
            //            {
            //                cache.GetOrSet(key3, context3 =>
            //                {
            //                    context3.AddExpirationTrigger(trigger3);
            //                    context3.SetAbsoluteExpiration(TimeSpan.FromSeconds(15));
            //                    return obj;
            //                });
            //            }
            //            context2.AddEntryLink(link2);
            //            return obj;
            //        });
            //    }
            //    context1.AddEntryLink(link1);
            //    return obj;
            //});

            using (link1 = cache.CreateLinkingScope())
            {
                cache.Set(key1, obj, new CacheEntryOptions().AddEntryLink(link1));

                using (link2 = cache.CreateLinkingScope())
                {
                    cache.Set(key2, obj, new CacheEntryOptions()
                        .AddExpirationTrigger(trigger2)
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(10))
                        .AddEntryLink(link2));

                    cache.Set(key3, obj, new CacheEntryOptions()
                        .AddExpirationTrigger(trigger3)
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(15)));
                }
            }

            Assert.Equal(2, link1.Triggers.Count());
            Assert.NotNull(link1.AbsoluteExpiration);
            Assert.Equal(clock.UtcNow + TimeSpan.FromSeconds(10), link1.AbsoluteExpiration);

            Assert.Equal(1, link2.Triggers.Count());
            Assert.NotNull(link2.AbsoluteExpiration);
            Assert.Equal(clock.UtcNow + TimeSpan.FromSeconds(15), link2.AbsoluteExpiration);

            cache.Set(key1, obj, new CacheEntryOptions().AddEntryLink(link2));
        }
    }
}