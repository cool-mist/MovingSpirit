using System;

namespace MovingSpirit.Api
{

    public interface ITaskResponse<T> where T : class
    {
        public T Result { get; }

        public ITaskStatistics Stats { get; }

    }

    public interface ITaskStatistics
    {
        public TimeSpan ExecutionTime { get; }

        public bool TimedOut { get; }

        public bool Succeeded { get; }
    }
}
