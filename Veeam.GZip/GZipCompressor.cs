using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Diagnostics;
using Veeam.GZip.Base;
using Veeam.GZip.Buffers;
using Veeam.GZip.Events;
using Veeam.GZip.Helpers;
using ErrorEventArgs = Veeam.GZip.Events.ErrorEventArgs;
using System.Threading;

namespace Veeam.GZip
{
    /// <summary>
    /// GZip main implementation.
    /// </summary>
    internal class GZipCompressor : GZipBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Veeam.GZip.GZip"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        public GZipCompressor(GZipOptions options) : base(options) { }

        /// <summary>
        /// Read this instance.
        /// </summary>
        protected override void ReadHeader(FileStream fs)
        {
            _uncompressedFileLength = fs.Length;
        }

        /// <summary>
        /// Reads the chunk header.
        /// </summary>
        /// <returns>The chunk header.</returns>
        /// <param name="fs">Fs.</param>
        protected override int ReadChunkHeader(FileStream fs)
        {
            return Options.BufferSize;
        }

        /// <summary>
        /// Compress this instance.
        /// </summary>
        protected override void DeCompress(InBuffer buffer)
        {
            using (MemoryStream compressedStream = new MemoryStream())
            {
                using (var gzStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    gzStream.Write(buffer.Buffer, 0, buffer.Buffer.Length);
                }

                var crc32Hash = new Crc32();
                crc32Hash.Update(buffer.Buffer);

                var isize = (uint)(buffer.Buffer.Length & 0xffffffff);
                var crc32 = (uint)(crc32Hash.Value & 0xffffffff);

                var outBuffer = new OutBuffer(buffer.Id, compressedStream.ToArray(), crc32, isize);

                lock (_outBuffer)
                {
                    _outBuffer.Add(buffer.Id, outBuffer);

                    // if our process it inrease ram limit then wait for output buffer erise
                    if (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 > Options.MemoryLimit)
                        Monitor.Wait(_outBuffer);
                }
            }
        }

        /// <summary>
        /// Writes the header.
        /// </summary>
        /// <param name="fs">Fs.</param>
        protected override void WriteHeader(FileStream fs)
        {
            // set magic1
            fs.WriteByte(ID1);

            // set magic2
            fs.Write(BitConverter.GetBytes(ID2), 0, sizeof(int));

            var fileInfo = new FileInfo(Options.InputFile);

            // write uncompressed file length from the header
            fs.Write(BitConverter.GetBytes(fileInfo.Length), 0, sizeof(long));

            // write uncompressed chunks buffer size to header
            fs.Write(BitConverter.GetBytes(Options.BufferSize), 0, sizeof(int));
        }

        /// <summary>
        /// Writes the chunk header.
        /// </summary>
        /// <param name="fs">Fs.</param>
        /// <param name="buffer">Buffer.</param>
        protected override void WriteChunkHeader(FileStream fs, OutBuffer buffer)
        {
#if RFC1952
            // Initi GZip Header
            var header = new GZipHeader
            {
                ID1 = 0x1f,
                ID2 = 0x8b,
                CM = 8, // deflate
                FLG = GZipFlags.None,
                MTIME = 0,
                XFL = 0,
                OS = GZipOS.Unknown
            };

            // convert header to byte array
            byte[] headerArr = Interop.SrtructToArray(header);

            // write header to file (let use same header for each member)
            fs.Write(headerArr, 0, headerArr.Length);
#else
            // write compressed size
            fs.Write(BitConverter.GetBytes(buffer.Buffer.Length), 0, sizeof(int));
#endif
        }

        /// <summary>
        /// Writes the chunk footer.
        /// </summary>
        /// <param name="fs">Fs.</param>
        /// <param name="buffer">Buffer.</param>
        protected override void WriteChunkFooter(FileStream fs, OutBuffer buffer)
        {
#if RFC1952
                        var footer = new GZipFooter
                        {
                            Crc32 = buffer.Crc32,
                            ISize = buffer.ISize
                        };

                        byte[] footerArr = Interop.SrtructToArray(footer);

                        // write gzip footer
                        fs.Write(footerArr, 0, footerArr.Length);
#endif
        }
    }
}
