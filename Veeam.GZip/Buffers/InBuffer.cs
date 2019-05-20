using System;
namespace Veeam.GZip.Buffers
{
    public class InBuffer
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

        #endregion [ Public Properties ]

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Veeam.GZip.Buffers.InBuffer"/> class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="buffer">Buffer.</param>
        public InBuffer(long id, byte[] buffer)
        {
            _id = id;
            _buffer = buffer;
        }
    }
}
