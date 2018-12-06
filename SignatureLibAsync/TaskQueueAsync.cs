﻿using System;
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
        #region Private_fields

        private readonly ConcurrentQueue<ITask> _tasks = new ConcurrentQueue<ITask>();

        private readonly ConcurrentDictionary<string, int> _statistic = new ConcurrentDictionary<string, int>();

        private volatile bool _runTaskQueue;

        private readonly Stopwatch _stopWatch;

        private volatile int _countTasks;

        private volatile int _countTasksRunning;

        #endregion

        #region Properties

        public Exception Error { get; private set; }

        #endregion

        #region Public_methods

        /// <summary>
        /// Сonstructor
        /// </summary>
        public TaskQueueAsync()
        {
            _runTaskQueue = true;
            _stopWatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Add task for processing async
        /// </summary>
        /// <param name="task">Task for processing</param>
        /// <returns>Task</returns>
        public Task AddTaskAsync(ITask task)
        {
            var result = Task.Run(() => AddTaskInner(task));
            return result;
        }

        /// <summary>
        /// Add task for processing sync
        /// </summary>
        /// <param name="task">Task for processing</param>
        /// <returns></returns>
        public void AddTask(ITask task)
        {
            AddTaskInner(task);
        }

        /// <summary>
        /// Get of statistic execute task
        /// </summary>
        /// <param name="time">Time for processing all tasks</param>
        /// <param name="statistic">Count of executed tasks: key - Name of task type; value - count tasks executed</param>
        public void GetStatistics(out TimeSpan time, out Dictionary<string, int> statistic)
        {
            time = _stopWatch?.Elapsed ?? TimeSpan.Zero;
            statistic = _statistic.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        /// <summary>
        /// General loop for processing tasks
        /// </summary>
        public async Task GeneralLoop()
        {
            try
            {
                while (_runTaskQueue || _countTasksRunning > 0)
                {
                    var newTask = await GetTask();
                    if (newTask != null)
                        RunTask(newTask);
                }
            }
            catch (Exception ex)
            {
                Error = ex;
            }
            finally
            {
                _stopWatch.Stop();
            }
        }

        /// <summary>
        /// Stop general loop
        /// </summary>
        public void StopLoop()
        {
            _runTaskQueue = false;
        }

        #endregion

        #region Private_methods

        /// <summary>
        /// Add task for processing (called by public methods)
        /// </summary>
        /// <param name="task">Task for processing</param>
        /// <returns></returns>
        private void AddTaskInner(ITask task)
        {
            Interlocked.Increment(ref _countTasks);
            _tasks.Enqueue(task);
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
        private void RunTask(ITask task)
        {
            Interlocked.Increment(ref _countTasksRunning);
            Task.Run(() => {
                task.ActionCompleted = IncrementStatistics;
                task.ActionToRun.Invoke();

                if (task.Result is ITask taskResult)
                    AddTask(taskResult);
            });
        }

        /// <summary>
        /// Increment statistic for concrete task type
        /// </summary>
        /// <param name="task"></param>
        private void IncrementStatistics(ITask task)
        {
            if (!_statistic.ContainsKey(task.GetType().Name))
            {
                lock (_statistic)
                    if (!_statistic.ContainsKey(task.GetType().Name))
                        _statistic.TryAdd(task.GetType().Name, 1);
                    else
                        _statistic[task.GetType().Name]++;
            }
            else
                lock (_statistic)
                    _statistic[task.GetType().Name]++;

            Interlocked.Decrement(ref _countTasksRunning);
        }

        #endregion
    }
}
