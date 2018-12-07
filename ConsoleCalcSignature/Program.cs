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
                if (2 != args.Length)
                    throw new ApplicationException("You must set input file and size of block in bytes !");

                Console.WriteLine();
                Console.WriteLine("Input parameters:");
                var i = 0;
                foreach (var arg in args)
                    Console.WriteLine($"Argument_{++i}: {arg}");

                using (var signer = new Signer())
                {
                    signer.Init(args[0], args[1]);
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
        /// <summary>
        /// Instance of Signworker
        /// </summary>
        private ISignWorker SignatureWorker { get; }

        /// <summary>
        /// Event signal to stop processing
        /// </summary>
        private readonly ManualResetEvent _eventStop = new ManualResetEvent(false);

        ///// <summary>
        ///// Обработчик сигнатур построенный на async/await
        ///// </summary>
        private SignWorkerAsync signWorkerAsync;// = new SignWorkerAsync();

        /// <summary>
        /// Constructor
        /// </summary>
        internal Signer()
        {
            //SignatureWorker = new SignWorker();
            signWorkerAsync = new SignWorkerAsync();
        }

        /// <summary>
        /// Initialization of processing
        /// </summary>
        /// <param name="inputFile">Input file to process</param>
        /// <param name="blockSize">Size of block to process</param>
        internal void Init(string inputFile, string blockSize)
        {
            //SignatureWorker.Init(inputFile, blockSize);
            //SignatureWorker.FileProcessCompleted += CompletedProcessed;
            signWorkerAsync.Init(inputFile, blockSize);
            signWorkerAsync.FileProcessCompleted += CompletedProcessed;
        }

        /// <summary>
        /// Start processing
        /// </summary>
        internal async Task RunSign()
        {
            //SignatureWorker.Run();
            await signWorkerAsync.Run();
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

            if (signWorkerAsync is IDisposable)
                ((IDisposable)signWorkerAsync).Dispose();
        }
    }
}
