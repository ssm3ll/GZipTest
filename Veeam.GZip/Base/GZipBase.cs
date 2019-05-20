using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Veeam.GZip.Buffers;
using Veeam.GZip.Events;
using Veeam.GZip.Interface;
using ErrorEventArgs = Veeam.GZip.Events.ErrorEventArgs;

namespace Veeam.GZip.Base
{
    /// <summary>
    /// GZip main implementation.
    /// </summary>
    internal abstract class GZipBase : IGZipArchive
    {
        /// <summary>
        /// The identifier 1.
        /// </summary>
        protected const byte ID1 = 0x50;
        /// <summary>
        /// The identifier 2.
        /// </summary>
        protected const int ID2 = 0x4545414d;

        #region [ Events ]

        /// <summary>
        /// Occurs when on progress.
        /// </summary>
        public event EventHandler<ProgressEventArgs> Progress;
        /// <summary>
        /// Occurs when on complete.
        /// </summary>
        public event EventHandler Completed;
        /// <summary>
        /// Occurs when on error.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        #endregion [ Events ]

        #region [ Protected ]

        /// <summary>
        /// The is cancelled.
        /// </summary>
        protected bool _isCancelled;

        /// <summary>
        /// The threads.
        /// </summary>
        protected Thread[] _workThreads;

        /// <summary>
        /// The read thread.
        /// </summary>
        protected Thread _readThread;

        /// <summary>
        /// The write thread.
        /// </summary>
        protected Thread _writeThread;

        /// <summary>
        /// The in buffer.
        /// </summary>
        protected Queue<InBuffer> _inBuffer;

        /// <summary>
        /// The out buffer.
        /// </summary>
        protected Dictionary<long,OutBuffer> _outBuffer;

        /// <summary>
        /// The read completed event.
        /// </summary>
        protected ManualResetEvent _readCompletedEvent;

        /// <summary>
        /// The write completed event.
        /// </summary>
        protected ManualResetEvent _writeCompletedEvent;

        /// <summary>
        /// The length of the uncompressed file.
        /// </summary>
        protected long _uncompressedFileLength;

        #endregion [ Protected ]


        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public GZipOptions Options { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Veeam.GZip.GZip"/> class.
        /// </summary>
        /// <param name="options">Options.</param>
        protected GZipBase(GZipOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            _inBuffer = new Queue<InBuffer>();
            _outBuffer = new Dictionary<long, OutBuffer>();

            _readCompletedEvent = new ManualResetEvent(false);
            _writeCompletedEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// Cancel this instance.
        /// </summary>
        public void Cancel()
        {
            _isCancelled = true;

            _readThread?.Join();

            _writeThread?.Join();

            foreach (var workThread in _workThreads)
                workThread?.Join();
        }

        /// <summary>
        /// Process this instance.
        /// </summary>
        /// <returns>The process.</returns>
        public int Process()
        {
            // init work threads array based on current CPU
            _workThreads = new Thread[Environment.ProcessorCount];

            // init read file thread
            _readThread = new Thread(Read);

            // init write file thread
            _writeThread = new Thread(Write);

            // start read thread
            _readThread.Start();

            // init compress/decompress threads
            for (int i = 0; i < _workThreads.Length; i++)
            {
                _workThreads[i] = new Thread(ProcessChunks);
                _workThreads[i].Start();
            }

            // start write thread
            _writeThread.Start();

            // wait for write to the dest file
            _writeCompletedEvent.WaitOne();

            // send Complete if process hasn't been canceled
            if (!_isCancelled)
                OnCompleted(new EventArgs());

            return _isCancelled ? 1 : 0;
        }

        /// <summary>
        /// Ons the error.
        /// </summary>
        /// <param name="e">E.</param>
        protected virtual void OnError(ErrorEventArgs e) => Error?.Invoke(this, e);
        /// <summary>
        /// Ons the progress.
        /// </summary>
        /// <param name="e">E.</param>
        protected virtual void OnProgress(ProgressEventArgs e) => Progress?.Invoke(this, e);

        /// <summary>
        /// Ons the completed.
        /// </summary>
        /// <param name="e">E.</param>
        protected virtual void OnCompleted(EventArgs e) => Completed?.Invoke(this, e);

        /// <summary>
        /// Read this instance.
        /// </summary>
        protected void Read()
        {
            try
            {
                int pos = 0;
                using (FileStream fs = new FileStream(Options.InputFile, FileMode.Open, FileAccess.Read))
                {
                    // check/read header
                    ReadHeader(fs);

                    while (fs.Position < fs.Length)
                    {
                        var chunkLength = ReadChunkHeader(fs);

                        var buffer = new byte[chunkLength];
                        fs.Read(buffer, 0, buffer.Length);

                        lock (_inBuffer)
                        {
                            _inBuffer.Enqueue(new InBuffer(pos, buffer));
                        }

                        lock(_outBuffer)
                        {
                            // if our process it inrease ram limit then wait for output buffer erise
                            if (System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 > Options.MemoryLimit)
                                Monitor.Wait(_outBuffer);
                        }

                        pos++;
                    }
                }
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
        /// Write this instance.
        /// </summary>
        protected void Write()
        {
            try
            {
                int pos = 0;

                using (FileStream fs = new FileStream(Options.OutputFile, FileMode.Create, FileAccess.Write))
                {
                    // write header if required
                    WriteHeader(fs);

                    // try to write to result file during read process
                    while (!_readCompletedEvent.WaitOne(0, false) || _workThreads.Any(th => !th.Join(0)))
                    {
                        OutBuffer buffer;

                        lock (_outBuffer)
                        {
                            if (_outBuffer.Count == 0)
                            {
                                GC.Collect();
                                Monitor.PulseAll(_outBuffer);
                                continue;
                            }

                            if (!_outBuffer.TryGetValue(pos, out buffer))
                                continue;
                        }

                        WriteChunkHeader(fs, buffer);

                        // write compressed block
                        fs.Write(buffer.Buffer, 0, buffer.Buffer.Length);

                        WriteChunkFooter(fs, buffer);

                        lock (_outBuffer)
                        {
                            _outBuffer.Remove(pos);
                        }

                        pos++;

                        // display progress
                        OnProgress(new ProgressEventArgs(pos, _uncompressedFileLength / Options.BufferSize));
                    }

                    // wait for all compress threads to complete the operation (just to be sure)
                    for (int i = 0; i < _workThreads.Length; i++)
                        _workThreads[i].Join();

                    // write the rest after we complete compressing
                    while (_outBuffer.Count > 0)
                    {
                        OutBuffer buffer;

                        lock (_outBuffer)
                        {
                            if (_outBuffer.Count == 0)
                            {
                                GC.Collect();
                                Monitor.PulseAll(_outBuffer);
                                continue;
                            }

                            if (!_outBuffer.TryGetValue(pos, out buffer))
                                continue;
                        }

                        WriteChunkHeader(fs, buffer);

                        // write compressed block
                        fs.Write(buffer.Buffer, 0, buffer.Buffer.Length);

                        WriteChunkFooter(fs, buffer);

                        lock (_outBuffer)
                        {
                            _outBuffer.Remove(pos);
                        }

                        pos++;

                        // display progress
                        OnProgress(new ProgressEventArgs(pos, _uncompressedFileLength / Options.BufferSize));
                    }
                }
            }
            catch (Exception ex)
            {
                _isCancelled = true;
                OnError(new ErrorEventArgs(ex));
            }
            finally
            {
                // we completed write the file
                _writeCompletedEvent.Set();
            }
        }

        /// <summary>
        /// Processes the chunks.
        /// </summary>
        protected void ProcessChunks()
        {
            try
            {
                while (!_isCancelled)
                {
                    InBuffer buffer;
                    byte[] decompressedBuffer = new byte[Options.BufferSize];

                    lock (_inBuffer)
                    {
                        if (_inBuffer.Count == 0)
                        {
                            if (_readCompletedEvent.WaitOne(0, false)) break;
                            continue;
                        }

                        buffer = _inBuffer.Dequeue();
                    }

                    DeCompress(buffer);
                }
            }
            catch (Exception ex)
            {
                _isCancelled = true;
                OnError(new ErrorEventArgs(ex));
            }
        }


        /// <summary>
        /// Compress this instance.
        /// </summary>
        protected abstract void DeCompress(InBuffer buffer);

        /// <summary>
        /// Reads the header.
        /// </summary>
        /// <param name="fs">Fs.</param>
        protected virtual void ReadHeader(FileStream fs) { }

        /// <summary>
        /// Writes the header.
        /// </summary>
        /// <param name="fs">Fs.</param>
        protected virtual void WriteHeader(FileStream fs) { }

        /// <summary>
        /// Reads the chunk header.
        /// </summary>
        /// <returns>The chunk header.</returns>
        /// <param name="fs">Fs.</param>
        protected abstract int ReadChunkHeader(FileStream fs);

        /// <summary>
        /// Writes the body.
        /// </summary>
        /// <param name="fs">Fs.</param>
        protected virtual void WriteChunkHeader(FileStream fs, OutBuffer buffer) { }

        /// <summary>
        /// Writes the chunk footer.
        /// </summary>
        /// <param name="fs">Fs.</param>
        protected virtual void WriteChunkFooter(FileStream fs, OutBuffer buffer) { }
    }
}
