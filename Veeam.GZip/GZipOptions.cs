using System;
using System.IO;
using System.IO.Compression;

namespace Veeam.GZip
{
    /// <summary>
    /// GZip options.
    /// </summary>
    public class GZipOptions
    {
        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        public CompressionMode? Mode { get; set; }

        /// <summary>
        /// Gets or sets the input file.
        /// </summary>
        /// <value>The input file.</value>
        public string InputFile { get; set; }

        /// <summary>
        /// Gets or sets the output file.
        /// </summary>
        /// <value>The output file.</value>
        public string OutputFile { get; set; }

        /// <summary>
        /// Gets or sets the output file.
        /// </summary>
        /// <value>The output file.</value>
        public int BufferSize { get; set; } = 1024 * 1024; // 1 MB

        /// <summary>
        /// Gets or sets the memory limit.
        /// </summary>
        /// <value>The memory limit.</value>
        public long MemoryLimit { get; set; } = 500 * 1024 * 1024; // 500 MB

        /// <summary>
        /// Froms the arguments.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static GZipOptions FromArgs(string[] args)
        {
            var options = new GZipOptions();

            if (args.Length == 0 || !Enum.TryParse(args[0], out CompressionMode mode))
                throw new ArgumentException(string.Empty, nameof(options.Mode));

            options.Mode = mode;

            if (args.Length == 1 || !File.Exists(args[1]))
                throw new ArgumentException(string.Empty, nameof(options.InputFile));

            options.InputFile = args[1];

            if (args.Length == 2 || string.IsNullOrWhiteSpace(args[2]))
                throw new ArgumentException(string.Empty, nameof(options.OutputFile));

            options.OutputFile = args[2];

            return options;
        }

    }
}
