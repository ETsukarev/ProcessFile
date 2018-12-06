using System;
using System.Threading.Tasks;
using SignatureWorkerAbstracts;

namespace SignatureLib.Interfaces
{
    /// <summary>
    /// Interface to process input file
    /// </summary>
    public interface ISignWorkerAsync
    {
        /// <summary>
        /// Initialization of Worker
        /// </summary>
        /// <param name="inputFile">Input file to process</param>
        /// <param name="sizeBlock">Size of block to process</param>
        /// <param name="fileResult">File with results</param>
        void Init(string inputFile, string sizeBlock, string fileResult = "SignatureBlocks.txt");

        /// <summary>
        /// Asynchronously run processing the file
        /// </summary>
        Task Run();

        /// <summary>
        /// Event of complete processing file
        /// </summary>
        event EventHandler<SignWorkerCompletedAbstractArgs> FileProcessCompleted;

        /// <summary>
        /// Get error if occurred, else null
        /// </summary>
        Exception Error { get; }
    }
}
