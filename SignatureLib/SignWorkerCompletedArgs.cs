using SignatureWorkerAbstracts;
using System;
using System.Collections.Generic;

namespace SignatureLib
{
    /// <summary>
    /// Argument class for passing final statistics
    /// </summary>
    public class SignWorkerCompletedArgs : SignWorkerCompletedAbstractArgs
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
            _stat = stat;
            TimeProcessing = timeProcessing;
        }

        /// <summary>
        /// Statistic of processed blocks
        /// </summary>
        private Dictionary<string, long> _stat { get; }

        /// <summary>
        /// Count blocks in source file
        /// </summary>
        public override long CountBlocks
        {
            get
            {
                if (_stat.ContainsKey(nameof(TaskCalcHashSha256)))
                    return _stat[nameof(TaskCalcHashSha256)];
                return 0;
            }
        }
    }
}
