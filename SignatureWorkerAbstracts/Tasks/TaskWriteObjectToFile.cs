using System;
using System.Text;
using SignatureWorkerCommonTypes.Interfaces;

namespace SignatureWorkerCommonTypes.Tasks
{
    /// <summary>
    /// Task for write result to file
    /// </summary>
    public class TaskWriteObjectToFile : ITask
    {
        /// <summary>
        /// Instance for synchronization writing to file
        /// </summary>
        private static readonly TaskWriteObjectToFile Instance = new TaskWriteObjectToFile();

        /// <summary>
        /// Action to execute
        /// </summary>
        private Action _actionToRun;

        /// <summary>
        /// Result of executed Action
        /// </summary>
        private object _result;

        /// <summary>
        /// Source to read data
        /// </summary>
        private ISource _source;

        /// <summary>
        /// File name for write data
        /// </summary>
        private static string _fileName;

        /// <summary>
        /// Disables creating instances via the constructor
        /// </summary>
        private TaskWriteObjectToFile()
        { }

        /// <summary>
        /// Creator of instance TaskWriteObjectToFile
        /// </summary>
        /// <param name="source">String source</param>
        /// <returns>Instance of TaskWriteObjectToFile</returns>
        public static TaskWriteObjectToFile GetInstance(ISource source)
        {
            TaskWriteObjectToFile taskWriteToFile = new TaskWriteObjectToFile {_source = source};
            taskWriteToFile.GenerateAction();
            return taskWriteToFile;
        }

        /// <summary>
        /// File name write result
        /// </summary>
        public static string FileName
        {
            get => _fileName;
            set => _fileName = value;
        }

        /// <summary>
        /// Init this task
        /// </summary>
        private void GenerateAction()
        {
            byte[] buffer = _source.GetPortionData(out int blockNumber);

            Action calcHashFunc = () =>
            {
                byte[] block = buffer;

                lock (Instance)
                    using (var sw = System.IO.File.AppendText(_fileName))
                        sw.WriteLine(Encoding.UTF8.GetString(block));

                ActionCompleted?.Invoke(this);

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
            set => _result = value;
        }
    }
}
