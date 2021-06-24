using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MovingSpirit.Api.Impl
{
    internal class TaskExecutor
    {
        internal static async Task<ITaskResponse<T>> ExecuteAsync<T>(Func<Task<T>> task) where T : class
        {
            var Watch = new Stopwatch();
            var succeeded = false;
            var timedout = false;
            var executionTime = TimeSpan.FromSeconds(0);
            var result = default(T);
            Watch.Start();
            try
            {
                result = await task.Invoke();
                succeeded = true;
            }
            catch (TaskCanceledException)
            {
                succeeded = false;
                timedout = true;
            }
            catch (Exception)
            {
                succeeded = false;
            }
            finally
            {
                Watch.Stop();
                executionTime = Watch.Elapsed;
            }

            return new TaskResponse<T>()
            {
                Result = result,
                Stats = new TaskStatistics()
                {
                    Succeeded = succeeded,
                    TimedOut = timedout,
                    ExecutionTime = executionTime
                }
            };
        }
    }

    internal class TaskResponse<T> : ITaskResponse<T> where T : class
    {
        public T Result { get; set; }

        public ITaskStatistics Stats { get; set; }

    }

    internal class TaskStatistics : ITaskStatistics
    {
        public TimeSpan ExecutionTime { get; set; }

        public bool TimedOut { get; set; }

        public bool Succeeded { get; set; }
    }
}
