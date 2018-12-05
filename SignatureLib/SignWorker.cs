//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading;
//using SignatureLib.Interfaces;
//using SignatureLib.Sources;
//using SignatureWorkerAbstracts;


//namespace SignatureLib
//{
//    /// <summary>
//    /// Class for sign blocks of input file
//    /// </summary>
//    public sealed class SignWorker : ISignWorker, IDisposable
//    {
//        /// <summary>
//        /// Thread for read blocks from file and send its to process
//        /// </summary>
//        private Thread _genThread;

//        /// <summary>
//        /// Queue for processing tasks
//        /// </summary>
//        private ITaskQueue _taskQueue;

//        /// <summary>
//        /// File source associated with input file
//        /// </summary>
//        private ISource _fileSource;

//        /// <summary>
//        /// Exception if occurred
//        /// </summary>
//        private Exception _error;

//        /// <summary>
//        /// Initialization
//        /// </summary>
//        /// <param name="inputFile">Input file</param>
//        /// <param name="sizeBlock">Size of block</param>
//        /// <param name="fileResult">File of result</param>
//        public void Init(string inputFile, string sizeBlock, string fileResult = "SignatureBlocks.txt")
//        {
//            if (! int.TryParse(sizeBlock, out int sizeOfBlock))
//                throw new ArgumentException("Size of block must be integer value");

//            _fileSource = new FileSource();
//            _fileSource.Open(inputFile, sizeOfBlock);
//            TaskCalcHashSha256.Source = _fileSource;

//            TaskWriteObjectToFile.FileName = Path.Combine(Environment.CurrentDirectory, fileResult);

//            _taskQueue = new TaskQueue();
//            _taskQueue.Init(true);
//        }

//        /// <summary>
//        /// Asynchronously run processing the file
//        /// </summary>
//        public void Run()
//        {
//            _genThread = new Thread(ThreadFunction) {IsBackground = true};
//            _genThread.Start();
//        }

//        /// <summary>
//        /// Function for read blocks from file and send its to process
//        /// </summary>
//        private void ThreadFunction()
//        {
//            try
//            {
//                WriteHeadersToResult();

//                while (! _fileSource.IsReadComplete() && _taskQueue.Error == null)
//                    _taskQueue.AddTask(TaskCalcHashSha256.GetInstance());

//                _taskQueue.StopQueue();
//                _taskQueue.WaitComplete();
//            }
//            catch (Exception ex)
//            {
//                Error = ex;
//                _taskQueue.StopQueue();
//            }
//            finally
//            {
//                _taskQueue.GetStatistics(out TimeSpan time, out Dictionary<string, long> stat);
//                OnSendCompleted(new SignWorkerCompletedArgs(time, stat, Error, _taskQueue.Error));
//            }
//        }

//        /// <summary>
//        /// Write header to result file
//        /// </summary>
//        private void WriteHeadersToResult()
//        {
//            _taskQueue.AddNotBufferedTask(TaskWriteObjectToFile.GetInstance(StringSource.GetInstance($"Calc SHA256 for each block of file: {_fileSource.Name}")));
//            _taskQueue.AddNotBufferedTask(TaskWriteObjectToFile.GetInstance(StringSource.GetInstance(string.Empty)));
//        }

//        /// <summary>
//        /// Event of complete processing file
//        /// </summary>
//        public event EventHandler<SignWorkerCompletedAbstractArgs> FileProcessCompleted;

//        /// <summary>
//        /// Fire event handler for FileProcessCompleted
//        /// </summary>
//        /// <param name="args">arguments</param>
//        private void OnSendCompleted(SignWorkerCompletedAbstractArgs args)
//        {
//            EventHandler<SignWorkerCompletedAbstractArgs> handler = FileProcessCompleted;
//            handler?.Invoke(this, args);
//        }

//        /// <summary>
//        /// Get error if occurred, else null
//        /// </summary>
//        public Exception Error
//        {
//            get => _error;
//            private set => _error = value;
//        }

//        /// <summary>
//        /// Dispose resources
//        /// </summary>
//        public void Dispose()
//        {
//            _fileSource?.Dispose();
//            if (_taskQueue is IDisposable)
//                ((IDisposable)_taskQueue).Dispose();

//            _genThread?.Join();
//        }

//        Task ISignWorker.Run()
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
