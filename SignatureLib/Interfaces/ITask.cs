using System;

namespace SignatureLib
{
    /// <summary>
    /// Interface of processing task
    /// </summary>
    internal interface ITask
    {
        /// <summary>
        /// Action to run
        /// </summary>
        Action ActionToRun { get; }

        /// <summary>
        /// Result of processed task
        /// if Result is ITask, it's means insert result to TaskQueue
        /// </summary>
        object Result { get; }
    }
}
