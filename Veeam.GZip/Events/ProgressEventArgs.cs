using System;
namespace Veeam.GZip.Events
{
    /// <summary>
    /// Progress event arguments.
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the progress.
        /// </summary>
        /// <value>The progress.</value>
        public long Progress { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Veeam.GZip.Events.ProgressEventArgs"/> class.
        /// </summary>
        /// <param name="progress">Progress.</param>
        public ProgressEventArgs(long current, long total)
        {
            Progress = current * 100 / total;
        }
    }
}
