using System;

namespace MovingSpirit.Api.Impl
{
    internal class CommandTimeout : ICommandTimeout
    {
        internal CommandTimeout(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
        }

        public TimeSpan TimeSpan { get; }
    }
}
