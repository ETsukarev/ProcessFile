using System;
using System.Collections.Generic;

namespace SignatureWorkerCommonTypes.Interfaces
{
    /// <summary>
    /// Interface for adding tasks, processing them, getting status processing
    /// </summary>
    public interface ITaskQueue
    {
        /// <summary>
        /// Initialization of TaskQueue
        /// </summary>
        /// <param name="stopProcessingIfError">Stopping processing tasks if was error occurried</param>
        void Init(bool stopProcessingIfError);

        /// <summary>
        /// Adding task for processing
        /// </summary>
        /// <param name="task">Task for processing</param>
        void AddTask(ITask task);

        /// <summary>
        /// Adding task for processing in not buffered mode
        /// </summary>
        /// <param name="task">Task for processing</param>
        void AddNotBufferedTask(ITask task);

        /// <summary>
        /// Asynchronous stop processing
        /// </summary>
        void StopQueue();

        /// <summary>
        /// Synchronous waiting complete processing
        /// </summary>
        void WaitComplete();

        /// <summary>
        /// Get of statistic execute task
        /// </summary>
        /// <param name="time">Time for processing all tasks</param>
        /// <param name="statistic">Statistic of executing: key - Name of task type; value - count tasks executed</param>
        void GetStatistics(out TimeSpan time, out Dictionary<string, long> statistic);

        /// <summary>
        /// Error of processing, if it is
        /// null - if no error happened
        /// </summary>
        Exception Error { get; }
    }
}
