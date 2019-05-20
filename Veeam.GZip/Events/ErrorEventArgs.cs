using System;
namespace Veeam.GZip.Events
{
    /// <summary>
    /// Error event arguments.
    /// </summary>
    public class ErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Veeam.GZip.Events.ErrorEventArgs"/> class.
        /// </summary>
        /// <param name="exception">Exception.</param>
        public ErrorEventArgs(Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            Message = exception?.Message;
        }
    }
}
