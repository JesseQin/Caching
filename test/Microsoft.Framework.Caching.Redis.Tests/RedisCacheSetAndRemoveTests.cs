// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using Microsoft.Framework.Caching.Distributed;
using Xunit;

namespace Microsoft.Framework.Caching.Redis
{
    // TODO: Disabled due to CI failure
    // public
    class RedisCacheSetAndRemoveTests
    {
        [Fact]
        public void GetMissingKeyReturnsFalseOrNull()
        {
            var cache = RedisTestConfig.CreateCacheInstance(GetType().Name);
            string key = "non-existent-key";

            var result = cache.Get(key);
            Assert.Null(result);

            var found = cache.TryGetValue(key, out result);
            Assert.False(found);
        }

        [Fact]
        public void SetAndGetReturnsObject()
        {
            var cache = RedisTestConfig.CreateCacheInstance(GetType().Name);
            var value = new byte[1];
            string key = "myKey";

            cache.Set(key, value);

            var result = cache.Get(key);
            Assert.Equal(value, result);
        }

        [Fact]
        public void SetAndGetWorksWithCaseSensitiveKeys()
        {
            var cache = RedisTestConfig.CreateCacheInstance(GetType().Name);
            var value = new byte[1];
            string key1 = "myKey";
            string key2 = "Mykey";

            cache.Set(key1, value);

            var result = cache.Get(key1);
            Assert.Equal(value, result);

            result = cache.Get(key2);
            Assert.Null(result);
        }

        [Fact]
        public void SetAlwaysOverwrites()
        {
            var cache = RedisTestConfig.CreateCacheInstance(GetType().Name);
            var value1 = new byte[1] { 1 };
            string key = "myKey";

            cache.Set(key, value1);
            var result = cache.Get(key);
            Assert.Equal(value1, result);

            var value2 = new byte[1] { 2 };
            cache.Set(key, value2);
            result = cache.Get(key);
            Assert.Equal(value2, result);
        }

        [Fact]
        public void RemoveRemoves()
        {
            var cache = RedisTestConfig.CreateCacheInstance(GetType().Name);
            var value = new byte[1];
            string key = "myKey";

            cache.Set(key, value);
            var result = cache.Get(key);
            Assert.Equal(value, result);

            cache.Remove(key);
            result = cache.Get(key);
            Assert.Null(result);
        }
    }
}