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
        private string _fileName;
        private const int DefaultSizeBlock = 10000;

        private ISource _fileSource;

        private TaskQueueAsync _taskQueue;

        public SignWorkerAsync()
        {
        }

        public void Init(string inputFile, string sizeBlock, string fileResult = "SignatureBlocks.txt")
        {
            _fileName = inputFile;

            _sizeBlock = int.TryParse(sizeBlock, out var result) ? result : DefaultSizeBlock;

            _fileSource = new FileSource();
            _fileSource.Open(inputFile, _sizeBlock);
            TaskCalcHashSha256.Source = _fileSource;

            TaskWriteObjectToFile.FileName = Path.Combine(Environment.CurrentDirectory, fileResult);

            _taskQueue = new TaskQueueAsync();
        }

        public async void Run()
        {
            Task taskResult = null;
            try
            {
                taskResult = Task.Run(() =>_taskQueue.GeneralLoop());

                await Task.Run(() =>_taskQueue.AddTask(TaskWriteObjectToFile.GetInstance(StringSource.GetInstance($"Calc SHA256 for each block of file: {_fileSource.Name}"))));
                await Task.Run(() =>_taskQueue.AddTask(TaskWriteObjectToFile.GetInstance(StringSource.GetInstance(string.Empty))));

                while (!_fileSource.IsReadComplete())
                    _taskQueue.AddTask(TaskCalcHashSha256.GetInstance());

                await _taskQueue.StopLoop();
            }
            catch (Exception ex)
            {
                await _taskQueue.StopLoop();
                Error = ex;
            }
            finally
            {
                _taskQueue.GetStatistics(out TimeSpan time, out Dictionary<string, long> stat);
                OnSendCompleted(new SignWorkerAsyncCompletedArgs(time, stat, Error, _taskQueue.Error));
                taskResult?.Wait();
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
            //_taskQueue.StopLoop();
        }

    }
}
