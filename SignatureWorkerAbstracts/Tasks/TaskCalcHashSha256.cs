using System;
using System.Security.Cryptography;
using System.Threading;
using SignatureLib.Interfaces;
using SignatureLib.Sources;

namespace SignatureLib
{
    /// <summary>
    /// Task for calculate SHA256 on block of file
    /// </summary>
    public class TaskCalcHashSha256 : ITask
    {
        /// <summary>
        /// Instance for synchronization read data from file
        /// </summary>
        private static readonly TaskCalcHashSha256 Instance = new TaskCalcHashSha256();

        /// <summary>
        /// Disables creating instances via the constructor
        /// </summary>
        private TaskCalcHashSha256()
        { }

        /// <summary>
        /// Creator of instance TaskCalcHashSha256
        /// </summary>
        /// <returns>Instance of TaskCalcHashSha256</returns>
        public static TaskCalcHashSha256 GetInstance()
        {
            TaskCalcHashSha256 calcHashSha256 = new TaskCalcHashSha256();
            calcHashSha256.GenerateAction();
            return calcHashSha256;
        }

        /// <summary>
        /// Action to execute
        /// </summary>
        private Action _actionToRun;

        /// <summary>
        /// Source to read data
        /// </summary>
        public static ISource Source { get; set; }

        /// <summary>
        /// Result of executed Action
        /// </summary>
        private object _result;

        /// <summary>
        /// Init this task
        /// </summary>
        private void GenerateAction()
        {
            byte[] buffer;
            int blockNumber;

            lock (Instance)
                buffer = Source.GetPortionData(out blockNumber);

            Action calcHashFunc = () =>
            {
                int blockNum = blockNumber;
                byte[] block = buffer;

                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(block);

                    var hash = BitConverter.ToString(hashedBytes).Replace("-", string.Empty).ToLower();

                    string res = $"{blockNum} ->{block.Length} ->{Thread.CurrentThread.ManagedThreadId} ->{hash}";

                    Result = TaskWriteObjectToFile.GetInstance(StringSource.GetInstance(res));
                    ActionCompleted?.Invoke(this);
                }
            };
            ActionToRun = calcHashFunc;
        }

        /// <summary>
        /// Action to run
        /// </summary>
        public Action ActionToRun
        {
            get => _actionToRun;
            set => _actionToRun = value;
        }

        public Action<ITask> ActionCompleted { get; set; }

        /// <summary>
        /// Result of processed task
        /// if Result is ITask, it's means insert result to TaskQueue
        /// </summary>
        public object Result
        {
            get => _result;
            private set => _result = value;
        }
    }
}
