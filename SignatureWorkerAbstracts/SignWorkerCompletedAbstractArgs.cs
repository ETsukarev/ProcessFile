using System;

namespace SignatureWorkerCommonTypes
{
    public abstract class SignWorkerCompletedAbstractArgs : EventArgs
    {
        /// <summary>
        /// Overall time processing
        /// </summary>
        public TimeSpan TimeProcessing { get; set; }

        /// <summary>
        /// Error of SignWorker, null if no error
        /// </summary>
        public Exception ErrorSignWorker { get; set; }

        /// <summary>
        /// Error of TaskQueue, null if no error
        /// </summary>
        public Exception ErrorTaskQueue { get; set; }

        /// <summary>
        /// Count blocks in source file
        /// </summary>
        public abstract long CountBlocks { get; }

    }
}
