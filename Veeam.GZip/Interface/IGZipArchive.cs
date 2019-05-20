using System;
using Veeam.GZip.Events;

namespace Veeam.GZip.Interface
{
    /// <summary>
    /// GZip archive.
    /// </summary>
    public interface IGZipArchive
    {
        event EventHandler<ProgressEventArgs> Progress;
        /// <summary>
        /// Occurs when on complete.
        /// </summary>
        event EventHandler Completed;
        /// <summary>
        /// Occurs when on error.
        /// </summary>
        event EventHandler<ErrorEventArgs> Error;
        /// <summary>
        /// Process this instance.
        /// </summary>
        /// <returns>The process.</returns>
        int Process();
        /// <summary>
        /// Cancel this instance.
        /// </summary>
        void Cancel();
    }
}
