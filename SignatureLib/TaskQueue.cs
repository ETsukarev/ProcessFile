using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SignatureWorkerCommonTypes.Interfaces;

namespace SignatureLib
{
    /// <summary>
    /// Queue of tasks to process them
    /// </summary>
    class TaskQueue : ITaskQueue, IDisposable
    {
        /// <summary>
        /// It's not null if error occurred
        /// </summary>
        private Exception _error;

        /// <summary>
        /// Class to store thread and it's signal event
        /// </summary>
        private class ThreadWithSignalEvent
        {
            /// <summary>
            /// Signal event
            /// </summary>
            internal AutoResetEvent Sync { get; }

            /// <summary>
            /// Thread to process tasks
            /// </summary>
            internal Thread WorkerThread { get; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="ev">Signal event for thread</param>
            /// <param name="t">Thread to process tasks</param>
            internal ThreadWithSignalEvent(AutoResetEvent ev, Thread t)
            {
                Sync = ev;
                WorkerThread = t;
            }
        }

        /// <summary>
        /// List of worker threads and it's signal events
        /// </summary>
        private readonly List<ThreadWithSignalEvent> _listThreads = new List<ThreadWithSignalEvent>();

        /// <summary>
        /// Queue of tasks
        /// </summary>
        private readonly Queue<ITask> QueueInputTasks = new Queue<ITask>();

        /// <summary>
        /// true - allow add tasks; false - not
        /// </summary>
        private volatile bool _allowTasks;

        /// <summary>
        /// true - stop processing tasks, if error occured
        /// </summary>
        private bool _stopProcessingIfError;

        /// <summary>
        /// Get time
        /// </summary>
        private Stopwatch _stopwatch;

        /// <summary>
        /// To store: string - name type of task, long - count executed tasks
        /// </summary>
        private Dictionary<string, long> _statistic = new Dictionary<string, long>();

        /// <summary>
        /// Init of processing threads
        /// </summary>
        /// <param name="stopProcessingIfError">true - stop processing tasks, if error occured</param>
        public void Init(bool stopProcessingIfError = true)
        {
            foreach (var resetEvent in Enumerable.Range(1, GetNumberThreads).Select(i => new AutoResetEvent(false)))
                _listThreads.Add(new ThreadWithSignalEvent(resetEvent, new Thread(() => ProcessTasksQueue(resetEvent)) { IsBackground = true }));

            _stopProcessingIfError = stopProcessingIfError;
            _allowTasks = true;
            StartTimeLine();

            foreach (var t in _listThreads)
                t.WorkerThread.Start();
        }

        /// <summary>
        /// Start time to process
        /// </summary>
        private void StartTimeLine()
        {
            if (_stopwatch == null)
                _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Stop time to process
        /// </summary>
        private void StopTimeLine()
        {
            _stopwatch?.Stop();
        }

        /// <summary>
        /// Get count threads to process (half of all cores in system)
        /// </summary>
        private static int GetNumberThreads => Environment.ProcessorCount / 2;

        /// <summary>
        /// Thread function to process task
        /// </summary>
        /// <param name="eventGetData"></param>
        private void ProcessTasksQueue(AutoResetEvent eventGetData)
        {
            while (_allowTasks || 0 != QueueInputTasks.Count)
                try
                {
                    ITask processTask = null;

                    if (0 == QueueInputTasks.Count)
                        eventGetData.WaitOne();
                    else
                        lock (QueueInputTasks)
                            if (QueueInputTasks.Count > 0)
                                processTask = QueueInputTasks.Dequeue();

                    processTask?.ActionToRun?.Invoke();
                    IncrementStatistics(processTask);

                    if (processTask?.Result is ITask task)
                        AddTask(task, GetNumberThreads, false);
                }
                catch (Exception ex)
                {
                    if (_stopProcessingIfError)
                    {
                        _error = ex;
                        _allowTasks = false;
                    }
                }
        }

        /// <summary>
        /// Adding task for processing
        /// </summary>
        /// <param name="task">Task for processing</param>
        public void AddTask(ITask task)
        {
            AddTask(task, GetNumberThreads);
        }

        /// <summary>
        /// Adding task for processing in not buffered mode
        /// </summary>
        /// <param name="task">Task for processing</param>
        public void AddNotBufferedTask(ITask task)
        {
            AddTask(task, 0);
        }

        /// <summary>
        /// Add task to queue
        /// </summary>
        /// <param name="task">Task for processing</param>
        /// <param name="bufferCount">Size buffer to fire processing</param>
        /// <param name="outerTask">Inner/Outer task</param>
        private void AddTask(ITask task, int bufferCount, bool outerTask = true)
        {
            if (outerTask)
                if (!_allowTasks)
                    return;

            lock (QueueInputTasks)
                QueueInputTasks.Enqueue(task);

            if (QueueInputTasks.Count > bufferCount)
                SetSignalToThreads();
        }

        /// <summary>
        /// Asynchronous stop processing
        /// </summary>
        public void StopQueue()
        {
            _allowTasks = false;

            SetSignalToThreads();
        }

        /// <summary>
        /// Set signals to each thread
        /// </summary>
        private void SetSignalToThreads()
        {
            foreach (var t in _listThreads)
                t.Sync.Set();
        }

        /// <summary>
        /// Synchronous waiting complete processing
        /// </summary>
        public void WaitComplete()
        {
            foreach (var t in _listThreads)
                t.WorkerThread.Join();

            StopTimeLine();
        }

        /// <summary>
        /// Get of statistic execute task
        /// </summary>
        /// <param name="time">Time for processing all tasks</param>
        /// <param name="statistic">Statistic of executing: key - Name of task type; value - count tasks executed</param>
        public void GetStatistics(out TimeSpan time, out Dictionary<string, long> statistic)
        {
            time = _stopwatch?.Elapsed ?? TimeSpan.Zero;

            statistic = _statistic;
        }

        /// <summary>
        /// Increment for concrete task type
        /// </summary>
        /// <param name="task"></param>
        public void IncrementStatistics(ITask task)
        {
            if  (task == null)
                return;

            lock (_statistic)
            {
                if (!_statistic.ContainsKey(task.GetType().Name))
                    _statistic.Add(task.GetType().Name, 1);
                else
                    _statistic[task.GetType().Name]++;
            }
        }

        /// <summary>
        /// Error of processing, if it is
        /// null - if no error happened
        /// </summary>
        public Exception Error => _error;

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_listThreads != null)
                    foreach (var t in _listThreads)
                        t.Sync.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
