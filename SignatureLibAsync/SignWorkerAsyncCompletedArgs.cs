using System;
using System.Collections.Generic;
using SignatureWorkerCommonTypes;
using SignatureWorkerCommonTypes.Tasks;

namespace SignatureLibAsync
{
    /// <summary>
    /// Class for getting attributes of completed processed tasks
    /// </summary>
    public class SignWorkerAsyncCompletedArgs : SignWorkerCompletedAbstractArgs
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timeProcessing">Overall time of processing</param>
        /// <param name="stat">Statistic: Key: task_type; Value: count_processed_task_type</param>
        /// <param name="errorSignWorker">Error in SignWorker</param>
        /// <param name="errorTaskQueue">Error in TaskQueue</param>
        public SignWorkerAsyncCompletedArgs(TimeSpan timeProcessing, Dictionary<string, int> stat, Exception errorSignWorker, Exception errorTaskQueue)
        {
            ErrorTaskQueue = errorTaskQueue;
            ErrorSignWorker = errorSignWorker;
            _stat = stat;
            TimeProcessing = timeProcessing;
        }

        /// <summary>
        /// Statistic: Key: task_type; Value: count_processed_task_type
        /// </summary>
        private Dictionary<string, int> _stat { get; }

        /// <summary>
        /// Count processed tasks of TaskCalcHashSha256 type
        /// </summary>
        public override long CountBlocks => _stat.ContainsKey(nameof(TaskCalcHashSha256)) ? _stat[nameof(TaskCalcHashSha256)] : 0;
    }
}
