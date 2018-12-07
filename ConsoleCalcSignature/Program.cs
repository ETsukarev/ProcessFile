using System;
using System.Threading;
using System.Threading.Tasks;
using SignatureLib;
using SignatureLibAsync;
using SignatureWorkerCommonTypes;
using SignatureWorkerCommonTypes.Interfaces;

namespace ConsoleCalcSignature
{
    /// <summary>
    /// Class for Entry point to programme
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point to app
        /// </summary>
        /// <param name="args">inputFile, sizeOfBlock</param>
        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length < 3)
                    throw new ApplicationException("You must set input file, size of block in bytes and version processor (v1 or v2)");

                Console.WriteLine();
                Console.WriteLine("Input parameters:");
                var i = 0;
                foreach (var arg in args)
                    Console.WriteLine($"Argument_{++i}: {arg}");

                using (var signer = new Signer())
                {
                    signer.Init(args[0], args[1], args[2]);
                    await signer.RunSign();

                    //do
                    //{
                    //} while (!signer.WaitComplete(TimeSpan.FromSeconds(1)));
                    signer.WaitComplete(int.MaxValue);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Class wrapper to call SignWorker
    /// </summary>
    internal class Signer : IDisposable
    {
        private const string Version1 = "v1";

        private const string Version2 = "v2";

        /// <summary>
        /// Processor of signatures builded on Thread and classes .Net Framework 3.5
        /// </summary>
        private ISignWorker SignatureWorker { get; set; }

        /// <summary>
        /// Event signal to stop processing
        /// </summary>
        private readonly ManualResetEvent _eventStop = new ManualResetEvent(false);

        ///// <summary>
        ///// Processor of signatures builded on Task, async/await
        ///// </summary>
        private ISignWorkerAsync _signWorkerAsync;

        private string _version;

        /// <summary>
        /// Initialization of processing
        /// </summary>
        /// <param name="inputFile">Input file to process</param>
        /// <param name="blockSize">Size of block to process</param>
        /// <param name="version">Version of processing worker. "v1" - old classes for multithreading(.Net 3.5); "v2"- Task, async/await</param>
        internal void Init(string inputFile, string blockSize, string version = Version2)
        {
            if (version.Equals(Version2))
            {
                _signWorkerAsync = new SignWorkerAsync();
                _signWorkerAsync.Init(inputFile, blockSize);
                _signWorkerAsync.FileProcessCompleted += CompletedProcessed;
            }
            else if (version.Equals(Version1))
            {
                SignatureWorker = new SignWorker();
                SignatureWorker.Init(inputFile, blockSize);
                SignatureWorker.FileProcessCompleted += CompletedProcessed;
            }
            else
                throw new NotSupportedException("Version of processor not support! You can choose between v1 and v2 versions.");

            _version = version;
        }

        /// <summary>
        /// Start processing
        /// </summary>
        internal async Task RunSign()
        {
            if (_version.Equals(Version2))
                await _signWorkerAsync.Run();
            else if (_version.Equals(Version1)) 
                    await Task.Run(() => SignatureWorker.Run());
        }

        /// <summary>
        /// Callback to signal that processing tasks is over
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Statistic of processing tasks</param>
        private void CompletedProcessed(object sender, SignWorkerCompletedAbstractArgs args)
        {
            Console.WriteLine($"Overall time to calc SHA256: {args.TimeProcessing}");
            Console.WriteLine($"Overall blocks calculated: {args.CountBlocks}");
            if (args.ErrorSignWorker == null && args.ErrorTaskQueue == null)
                Console.WriteLine("Input file successfully processed !");
            else
            {
                Console.WriteLine("Error processing input file:");
                Console.WriteLine(args.ErrorSignWorker != null ? args.ErrorSignWorker.ToString() : string.Empty);
                Console.WriteLine(args.ErrorTaskQueue != null ? args.ErrorTaskQueue.ToString() : string.Empty);
            }

            _eventStop.Set();
        }

        /// <summary>
        /// Synchronously wait to complete processing
        /// </summary>
        /// <param name="timeToWait">Time wait to complete</param>
        /// <returns>true - processing is completed; false - doesn't completed</returns>
        internal bool WaitComplete(int timeToWait)
        {
            return _eventStop.WaitOne(timeToWait);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (SignatureWorker is IDisposable)
                ((IDisposable)SignatureWorker).Dispose();

            if (_signWorkerAsync is IDisposable)
                ((IDisposable)_signWorkerAsync).Dispose();
        }
    }
}
