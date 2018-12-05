using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SignatureLib.Interfaces;

namespace SignatureLibAsync
{
    /// <summary>
    /// Class for Async accumulation and run tasks
    /// </summary>
    public class TaskQueueAsync
    {
        private readonly ConcurrentQueue<ITask> _tasks = new ConcurrentQueue<ITask>();

        private readonly ConcurrentDictionary<string, long> _statistic = new ConcurrentDictionary<string, long>();

        private volatile bool _runTaskQueue;

        private readonly Stopwatch _stopWatch;

        private readonly ManualResetEvent manualResetEventStopQueue;

        public Exception Error { get; }

        /// <summary>
        /// Сonstructor
        /// </summary>
        public TaskQueueAsync()
        {
            _runTaskQueue = true;
            _stopWatch = Stopwatch.StartNew();
            manualResetEventStopQueue = new ManualResetEvent(false);
        }

        /// <summary>
        /// Add task for processing
        /// </summary>
        /// <param name="task">Task for processing</param>
        /// <returns></returns>
        public void AddTask(ITask task)
        {
            Task.Run(() => _tasks.Enqueue(task));
        }

        /// <summary>
        /// Get task from queue
        /// </summary>
        /// <returns></returns>
        private async Task<ITask> GetTask() => await Task.Run(() => _tasks.TryDequeue(out var resTask) ? resTask : null);

        /// <summary>
        /// Run task
        /// </summary>
        /// <param name="task">Task for processing</param>
        /// <returns></returns>
        private async Task RunTask(ITask task)
        {
            await Task.Run(task.ActionToRun);
            IncrementStatistics(task);

            if (task.Result is ITask taskResult)
                AddTask(taskResult);
        }

        /// <summary>
        /// Increment statistic for concrete task type
        /// </summary>
        /// <param name="task"></param>
        private void IncrementStatistics(ITask task)
        {
            if (!_statistic.ContainsKey(task.GetType().Name))
               _statistic.TryAdd(task.GetType().Name, 1);
            else
               _statistic[task.GetType().Name]++;
        }

        /// <summary>
        /// Get of statistic execute task
        /// </summary>
        /// <param name="time">Time for processing all tasks</param>
        /// <param name="statistic">Statistic of executing: key - Name of task type; value - count tasks executed</param>
        public void GetStatistics(out TimeSpan time, out Dictionary<string, long> statistic)
        {
            time = _stopWatch?.Elapsed ?? TimeSpan.Zero;
            statistic = _statistic.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        /// <summary>
        /// General loop for processing tasks
        /// </summary>
        public async Task GeneralLoop()
        {
            while (_runTaskQueue || _tasks.Count > 0)
            {
                var newTask = await GetTask();
                if (newTask != null)
                    await RunTask(newTask);
            }
            _stopWatch.Stop();
            manualResetEventStopQueue.Set();
        }

        /// <summary>
        /// Stop general loop
        /// </summary>
        public async Task StopLoop()
        {
            _runTaskQueue = false;
            await Task.Run(() => { manualResetEventStopQueue.WaitOne(); });
        }
    }
}
