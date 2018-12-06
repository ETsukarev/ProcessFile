using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SignatureLib.Interfaces;
using SignatureWorkerAbstracts;
using SignatureLib;
using SignatureLib.Sources;

namespace SignatureLibAsync
{
    public class SignWorkerAsync : ISignWorker, IDisposable
    {
        private int _sizeBlock;
        private const int DefaultSizeBlock = 10000;

        private ISource _fileSource;

        private TaskQueueAsync _taskQueue;

        public void Init(string inputFile, string sizeBlock, string fileResult = "SignatureBlocks.txt")
        {
            _sizeBlock = int.TryParse(sizeBlock, out var result) ? result : DefaultSizeBlock;

            _fileSource = new FileSource();
            _fileSource.Open(inputFile, _sizeBlock);
            TaskCalcHashSha256.Source = _fileSource;

            TaskWriteObjectToFile.FileName = Path.Combine(Environment.CurrentDirectory, fileResult);

            _taskQueue = new TaskQueueAsync();
        }

        public async Task Run()
        {
            Task taskResult = null;
            try
            {
                taskResult = Task.Run(() =>_taskQueue.GeneralLoop());

                await Task.Run(() =>_taskQueue.AddTask(TaskWriteObjectToFile.GetInstance(StringSource.GetInstance($"Calc SHA256 for each block of file: {_fileSource.Name}"))));
                await Task.Run(() =>_taskQueue.AddTask(TaskWriteObjectToFile.GetInstance(StringSource.GetInstance(string.Empty))));

                while (!_fileSource.IsReadComplete())
                    _taskQueue.AddTask(TaskCalcHashSha256.GetInstance());
            }
            catch (Exception ex)
            {
                Error = ex;
            }
            finally
            {
                _taskQueue.StopLoop();
                taskResult?.Wait();
                _taskQueue.GetStatistics(out TimeSpan time, out Dictionary<string, long> stat);
                OnSendCompleted(new SignWorkerAsyncCompletedArgs(time, stat, Error, _taskQueue.Error));
            }
        }

        public event EventHandler<SignWorkerCompletedAbstractArgs> FileProcessCompleted;

        public Exception Error { get; private set; }
       
        /// <summary>
        /// Fire event handler for FileProcessCompleted
        /// </summary>
        /// <param name="args">arguments</param>
        private void OnSendCompleted(SignWorkerCompletedAbstractArgs args)
        {
            EventHandler<SignWorkerCompletedAbstractArgs> handler = FileProcessCompleted;
            handler?.Invoke(this, args);
        }

        public void Dispose()
        {
            ((IDisposable)TaskCalcHashSha256.Source).Dispose();
        }

        void ISignWorker.Run()
        {
            throw new NotImplementedException();
        }
    }
}
