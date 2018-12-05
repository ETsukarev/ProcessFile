﻿using System;

namespace SignatureLib.Interfaces
{
    /// <summary>
    /// Interface of processing task
    /// </summary>
    public interface ITask
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