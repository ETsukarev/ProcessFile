using System.Text;

namespace SignatureLib.Sources
{
    /// <summary>
    /// Source is string
    /// </summary>
    class StringSource : ISource
    {
        /// <summary>
        /// Content string
        /// </summary>
        private string _source;

        /// <summary>
        /// Size of portion block (ignored)
        /// </summary>
        private int _portionData;

        /// <summary>
        /// Flag that string is read
        /// </summary>
        private bool _isReadComlete;

        /// <summary>
        /// Disables creating instances via the constructor
        /// </summary>
        private StringSource()
        { }

        /// <summary>
        /// Method for creating instances
        /// </summary>
        /// <param name="source">Content string</param>
        /// <returns>Initialized string source</returns>
        public static StringSource GetInstance(string source)
        {
            StringSource strSource = new StringSource();
            strSource.Open(source);
            return strSource;
        }

        /// <summary>
        /// Initialization of data source
        /// </summary>
        /// <param name="source">Source name</param>
        /// <param name="portionData">Size of data block</param>
        public void Open(string source, int portionData=int.MaxValue)
        {
            _portionData = portionData;
            _source = source;
        }

        /// <summary>
        /// Get content string
        /// </summary>
        /// <param name="numberPortion">Size of block to read (ignored)</param>
        /// <returns>String as byte array</returns>
        public byte[] GetPortionData(out int numberPortion)
        {
            if (_isReadComlete)
            {
                numberPortion = 0;
                return new byte[]{};
            }

            byte[] portion = Encoding.UTF8.GetBytes(_source);
            numberPortion = portion.Length;
            _isReadComlete = true;
            return portion;
        }

        /// <summary>
        /// Data in source is over ?
        /// </summary>
        /// <returns>true - data in source is over </returns>
        public bool IsReadComplete()
        {
            return _isReadComlete;
        }

        /// <summary>
        /// Content string itself
        /// </summary>
        public string Name => _source;

        /// <summary>
        /// No resources need free ))
        /// </summary>
        public void Dispose()
        {}
    }
}
