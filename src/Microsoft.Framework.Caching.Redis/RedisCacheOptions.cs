// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Caching.Memory.Infrastructure;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.Framework.Caching.Redis
{
    public class RedisCacheOptions : IOptions<RedisCacheOptions>
    {
        public ISystemClock Clock { get; set; }

        public string Configuration { get; set; }

        public string InstanceName { get; set; }

        RedisCacheOptions IOptions<RedisCacheOptions>.Options
        {
            get { return this; }
        }

        RedisCacheOptions IOptions<RedisCacheOptions>.GetNamedOptions(string name)
        {
            return this;
        }
    }
}