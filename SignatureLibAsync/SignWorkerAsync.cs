using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SignatureWorkerCommonTypes;
using SignatureWorkerCommonTypes.Interfaces;
using SignatureWorkerCommonTypes.Sources;
using SignatureWorkerCommonTypes.Tasks;

namespace SignatureLibAsync
{
    /// <summary>
    /// Class for reading data from sources and add it's for processing
    /// </summary>
    public class SignWorkerAsync : ISignWorkerAsync, IDisposable
    {
        #region Private_fields

        private const int DefaultSizeBlock = 10000;

        private int _sizeBlock;

        private ISource _fileSource;

        private TaskQueueAsync _taskQueue;

        #endregion

        #region Public_methods

        /// <summary>
        /// Initialization of instance this class
        /// </summary>
        /// <param name="inputFile">Input file</param>
        /// <param name="sizeBlock">Size of block for processing</param>
        /// <param name="fileResult">Name of file with result</param>
        public void Init(string inputFile, string sizeBlock, string fileResult = "SignatureBlocks.txt")
        {
            _sizeBlock = int.TryParse(sizeBlock, out var result) ? result : DefaultSizeBlock;

            _fileSource = new FileSource();
            _fileSource.Open(inputFile, _sizeBlock);
            TaskCalcHashSha256.Source = _fileSource;

            TaskWriteObjectToFile.FileName = Path.Combine(Environment.CurrentDirectory, fileResult);

            _taskQueue = new TaskQueueAsync();
        }

        /// <summary>
        /// Loop for generate and add tasks for processing
        /// </summary>
        /// <returns>Task</returns>
        public async Task Run()
        {
            Task taskResult = null;
            try
            {
                taskResult = Task.Run(() => _taskQueue.GeneralLoop());

                _taskQueue.AddTask(TaskWriteObjectToFile.GetInstance(StringSource.GetInstance($"Calc SHA256 for each block of file: {_fileSource.Name}")));
                _taskQueue.AddTask(TaskWriteObjectToFile.GetInstance(StringSource.GetInstance(string.Empty)));

                while (!_fileSource.IsReadComplete())
                    _taskQueue.AddTaskAsync(TaskCalcHashSha256.GetInstance());
            }
            catch (Exception ex)
            {
                Error = ex;
            }
            finally
            {
                _taskQueue.StopLoop();
                try
                {
                    await taskResult;
                }
                catch (Exception ex)
                {
                    Error = ex;
                }
                _taskQueue.GetStatistics(out TimeSpan time, out Dictionary<string, int> stat);
                OnSendCompleted(new SignWorkerAsyncCompletedArgs(time, stat, Error, _taskQueue.Error));
            }
        }

        /// <summary>
        /// Event for signalling complete processing
        /// </summary>
        public event EventHandler<SignWorkerCompletedAbstractArgs> FileProcessCompleted;

        /// <summary>
        /// Exception in time of working
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Resource disposing
        /// </summary>
        public void Dispose()
        {
            (TaskCalcHashSha256.Source as IDisposable).Dispose();
        }

        #endregion

        #region Private_methods

        /// <summary>
        /// Fire event handler for FileProcessCompleted
        /// </summary>
        /// <param name="args">arguments</param>
        private void OnSendCompleted(SignWorkerCompletedAbstractArgs args)
        {
            EventHandler<SignWorkerCompletedAbstractArgs> handler = FileProcessCompleted;
            handler?.Invoke(this, args);
        }

        #endregion
    }
}
