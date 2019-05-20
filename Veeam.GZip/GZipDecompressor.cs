using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Veeam.GZip.Base;
using Veeam.GZip.Buffers;
using Veeam.GZip.Events;
using ErrorEventArgs = Veeam.GZip.Events.ErrorEventArgs;

namespace Veeam.GZip
{
    /// <summary>
    /// GZip main implementation.
    /// </summary>
    internal class GZipDecompressor : GZipBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Veeam.GZip.GZip"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public GZipDecompressor(GZipOptions options) : base(options) { }

        /// <summary>
        /// Read this instance.
        /// </summary>
        protected override void ReadHeader(FileStream fs)
        {
            try
            {
                // get magic1
                var id1 = fs.ReadByte();

                // get magic2
                var id2Arr = new byte[sizeof(int)];
                fs.Read(id2Arr, 0, id2Arr.Length);

                var id2 = BitConverter.ToInt32(id2Arr, 0);

                if (id1 != ID1 || id2 != ID2) throw new InvalidDataException();

                // get uncompressed file length from the header
                var fileLengthSize = new byte[sizeof(int)];
                fs.Read(fileLengthSize, 0, fileLengthSize.Length);

                _uncompressedFileLength = BitConverter.ToInt32(fileLengthSize, 0);

                // get uncompressed buffer size from the header
                var bufferSize = new byte[sizeof(int)];
                fs.Read(bufferSize, 0, bufferSize.Length);

                Options.BufferSize = BitConverter.ToInt32(bufferSize, 0);
            }
            catch (Exception ex)
            {
                _isCancelled = true;
                OnError(new ErrorEventArgs(ex));
            }
            finally
            {
                // we completed read the file
                _readCompletedEvent.Set();
            }
        }

        /// <summary>
        /// Reads the chunk header.
        /// </summary>
        /// <returns>The chunk header.</returns>
        /// <param name="fs">Fs.</param>
        protected override int ReadChunkHeader(FileStream fs)
        {
            // read block size
            var blockSize = new byte[sizeof(int)];
            fs.Read(blockSize, 0, blockSize.Length);

            // get the length of the compressed block
            return BitConverter.ToInt32(blockSize, 0);
        }

        /// <summary>
        /// Compress this instance.
        /// </summary>
        protected override void DeCompress(InBuffer buffer)
        {
            byte[] decompressedBuffer = new byte[Options.BufferSize];

            using (MemoryStream compressedStream = new MemoryStream(buffer.Buffer))
            {
                using (var gzStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    int readSize = gzStream.Read(decompressedBuffer, 0, decompressedBuffer.Length);

                    byte[] readBuffer = new byte[readSize];
                    Array.Copy(decompressedBuffer, readBuffer, readBuffer.Length);

                    lock (_outBuffer)
                    {
                        _outBuffer.Add(buffer.Id, new OutBuffer(buffer.Id, readBuffer, 0, 0));

                        // if our process it inrease ram limit then wait for output buffer erise
                        if (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 > Options.MemoryLimit)
                            Monitor.Wait(_outBuffer);
                    }
                }
            }
        }
    }
}
