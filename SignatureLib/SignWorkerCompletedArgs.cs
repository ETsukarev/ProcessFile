using System;
using System.Collections.Generic;

namespace SignatureLib
{
    /// <summary>
    /// Argument class for passing final statistics
    /// </summary>
    public class SignWorkerCompletedArgs : EventArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timeProcessing">Overall time execute all tasks</param>
        /// <param name="stat">Statistic of executed tasks</param>
        /// <param name="errorSignWorker">Error of SignWorker, null if no error</param>
        /// <param name="errorTaskQueue">Error of TaskQueue, null if no error</param>
        public SignWorkerCompletedArgs(TimeSpan timeProcessing, Dictionary<string, long> stat, Exception errorSignWorker, Exception errorTaskQueue)
        {
            ErrorTaskQueue = errorTaskQueue;
            ErrorSignWorker = errorSignWorker;
            CountBlocks = stat;
            TimeProcessing = timeProcessing;
        }

        /// <summary>
        /// Statistic of processed blocks
        /// </summary>
        public Dictionary<string, long> CountBlocks { get; }

        /// <summary>
        /// Overall time processing
        /// </summary>
        public TimeSpan TimeProcessing { get; }

        /// <summary>
        /// Error of SignWorker, null if no error
        /// </summary>
        public Exception ErrorSignWorker { get; }

        /// <summary>
        /// Error of TaskQueue, null if no error
        /// </summary>
        public Exception ErrorTaskQueue { get; }
    }
}
