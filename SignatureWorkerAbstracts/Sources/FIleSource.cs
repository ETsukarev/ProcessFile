using System;
using System.IO;
using System.Net;
using SignatureLib.Interfaces;

namespace SignatureLib
{
    /// <summary>
    /// Source is file
    /// </summary>
    public class FileSource : ISource
    {
        /// <summary>
        /// Stream associated with file
        /// </summary>
        private FileStream _fsInputFile;

        /// <summary>
        /// Reader associated with file
        /// </summary>
        private BinaryReader _binaryReader;

        /// <summary>
        /// Size of portion data
        /// </summary>
        private int _portionData;

        /// <summary>
        /// Flag end of file
        /// </summary>
        private volatile bool _isReadComplete;

        /// <summary>
        /// Number of read data block
        /// </summary>
        private int _portion;

        /// <summary>
        /// Name of source (usully file name)
        /// </summary>
        private string _name;

        /// <summary>
        /// Total bytes read
        /// </summary>
        private long _overallRead;

        /// <summary>
        /// Initialization source
        /// </summary>
        /// <param name="source">File name</param>
        /// <param name="portionData">Size of block to read from</param>
        public void Open(string source, int portionData)
        {
            if (portionData < 1)
                throw new ArgumentException("Size of block can't less than 1");
            if (!File.Exists(source))
                throw new ArgumentException($"File {source} not exist !");

            _portionData = portionData;
            _fsInputFile = new FileStream(source, FileMode.Open);
            _binaryReader = new BinaryReader(_fsInputFile);
            _name = source;
        }

        /// <summary>
        /// Get portion data
        /// </summary>
        /// <param name="numberPortion">Size of block to read</param>
        /// <returns>Block data</returns>
        public byte[] GetPortionData(out int numberPortion)
        {
            byte[] bytes = _binaryReader.ReadBytes(_portionData);
            _overallRead += bytes.Length;
            if (bytes.Length != _portionData || _overallRead == _fsInputFile.Length)
                _isReadComplete = true;

            numberPortion = ++_portion;
            return bytes;
        }

        /// <summary>
        /// Data in source is over ?
        /// </summary>
        /// <returns>true - data in source is over </returns>
        public bool IsReadComplete()
        {
           return _isReadComplete;
        }

        /// <summary>
        /// Name of Source
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// File handles dispose
        /// </summary>
        public void Dispose()
        {
            _binaryReader?.Close();
            _fsInputFile?.Dispose();
        }
    }
}
