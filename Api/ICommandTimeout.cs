using System;

namespace MovingSpirit.Api
{
    public interface ICommandTimeout
    {
        TimeSpan TimeSpan { get; }
    }
}
