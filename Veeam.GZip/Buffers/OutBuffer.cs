using System;
namespace Veeam.GZip.Buffers
{
    /// <summary>
    /// GZip buffer.
    /// </summary>
    public class OutBuffer
    {
        #region [ Private Fields ]
        /// <summary>
        /// The identifier.
        /// </summary>
        private long _id;

        /// <summary>
        /// The buffer.
        /// </summary>
        private byte[] _buffer;

        /// <summary>
        /// The crc32.
        /// </summary>
        private uint _crc32;

        /// <summary>
        /// The isize.
        /// </summary>
        private uint _isize;

        #endregion [ Private Fields ]

        #region [ Public Properties ]
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public long Id => _id;
        /// <summary>
        /// Gets the buffer.
        /// </summary>
        /// <value>The buffer.</value>
        public byte[] Buffer => _buffer;

        /// <summary>
        /// Gets the crc32.
        /// </summary>
        /// <value>The crc32.</value>
        public uint Crc32 => _crc32;

        /// <summary>
        /// Gets the IS ize.
        /// </summary>
        /// <value>The IS ize.</value>
        public uint ISize => _isize;

        #endregion [ Public Properties ]

        public OutBuffer(long id, byte[] buffer, uint crc32, uint isize)
        {
            _id = id;
            _buffer = buffer;
            _crc32 = crc32;
            _isize = isize;
        }
    }
}
