using System;

namespace SignatureLib
{
    /// <summary>
    /// Interface to process input file
    /// </summary>
    public interface ISignWorker
    {
        /// <summary>
        /// Inialization of Worker
        /// </summary>
        /// <param name="inputFile">Input file to process</param>
        /// <param name="sizeBlock">Size of block to process</param>
        /// <param name="fileResult">File with results</param>
        void Init(string inputFile, string sizeBlock, string fileResult = "SignatureBlocks.txt");

        /// <summary>
        /// Asynchronously run processing the file
        /// </summary>
        void Run();

        /// <summary>
        /// Event of complete processing file
        /// </summary>
        event EventHandler<SignWorkerCompletedArgs> FileProcessCompleted;

        /// <summary>
        /// Get error if occurred, else null
        /// </summary>
        Exception Error { get; }
    }
}
