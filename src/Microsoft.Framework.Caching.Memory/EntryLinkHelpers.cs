// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
#if DNXCORE50
using System.Threading;
#elif NET45 || DNX451 || DNXCORE50
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#endif

namespace Microsoft.Framework.Caching.Memory
{
    public static class EntryLinkHelpers
    {
#if DNXCORE50
        private static readonly AsyncLocal<IEntryLink> _contextLink = new AsyncLocal<IEntryLink>();

        public static IEntryLink ContextLink
        {
            get { return _contextLink.Value; }
            set { _contextLink.Value = value; }
        }
#elif NET45 || DNX451
        private const string ContextLinkDataName = "klr.host.EntryLinkHelpers.ContextLink";

        public static IEntryLink ContextLink
        {
            get
            {
                var handle = CallContext.LogicalGetData(ContextLinkDataName) as ObjectHandle;

                if (handle == null)
                {
                    return null;
                }

                return handle.Unwrap() as IEntryLink;
            }
            set
            {
                CallContext.LogicalSetData(ContextLinkDataName, new ObjectHandle(value));
            }
        }
#else
        public static IEntryLink ContextLink
        {
            get { return null; }
            set { throw new NotImplementedException(); }
        }
#endif
        internal static IEntryLink FlowContext(this IEntryLink newLink)
        {
            var priorLink = ContextLink;
            ContextLink = newLink;
            return new ContextLinkReverter(newLink, priorLink);
        }

        private class ContextLinkReverter : IEntryLink
        {
            private readonly IEntryLink _newLink;
            private readonly IEntryLink _priorLink;
            private bool _disposed;

            public ContextLinkReverter(IEntryLink newLink, IEntryLink priorLink)
            {
                _newLink = newLink;
                _priorLink = priorLink;
            }

            public DateTimeOffset? AbsoluteExpiration { get { return _newLink.AbsoluteExpiration; } }

            public IEnumerable<IExpirationTrigger> Triggers { get { return _newLink.Triggers; } }

            public void AddExpirationTriggers(IList<IExpirationTrigger> triggers)
            {
                _newLink.AddExpirationTriggers(triggers);
            }

            public void SetAbsoluteExpiration(DateTimeOffset absoluteExpiration)
            {
                _newLink.SetAbsoluteExpiration(absoluteExpiration);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    ContextLink = _priorLink;
                }
            }
        }
    }
}
