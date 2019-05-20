using System;
using Veeam.GZip.Interface;

namespace Veeam.GZip
{
    public static class GZipArchive
    {
        /// <summary>
        /// Create the specified archive.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="options">Options.</param>
        public static IGZipArchive Create(GZipOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.Mode == System.IO.Compression.CompressionMode.Compress)
                return new GZipCompressor(options);
            if (options.Mode == System.IO.Compression.CompressionMode.Decompress)
                return new GZipDecompressor(options);

            throw new ArgumentException(nameof(options.Mode));
        }
    }
}
