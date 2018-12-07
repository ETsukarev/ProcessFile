using System;

namespace SignatureWorkerCommonTypes.Interfaces
{
    /// <summary>
    /// Interface of data source
    /// </summary>
    public interface ISource : IDisposable
    {
        /// <summary>
        /// Initialization of data source
        /// </summary>
        /// <param name="source">Source name</param>
        /// <param name="portionData">Size of data block</param>
        void Open(string source, int portionData);

        /// <summary>
        /// Get of data portion
        /// </summary>
        /// <returns>Array of data</returns>
        byte[] GetPortionData(out int numberPortion);

        /// <summary>
        /// Data in source is over ?
        /// </summary>
        /// <returns>true - data in source is over </returns>
        bool IsReadComplete();

        /// <summary>
        /// Name of Source
        /// </summary>
        string Name { get; }
    }
}
