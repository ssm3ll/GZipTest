using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using Veeam.GZip;

namespace Veeam.GZipTest
{
    class Program
    {
        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        /// <returns>The exit code that is given to the operating system after the program ends.</returns>
        static int Main(string[] args)
        {
            Console.Clear();
            Console.Title = "GZipTest";

            GZipOptions options = new GZipOptions()
            {
                Mode = CompressionMode.Compress,
                InputFile = "/Users/ssm3ll/Distr/exelab2018.zip",
                OutputFile = "/Users/ssm3ll/Distr/exelab2018.zip.gz"
            };

            //try
            //{
            //    // try to read options from file
            //    options = GZipOptions.FromArgs(args);
            //}
            //catch(ArgumentException aex)
            //{
            //    Console.WriteLine($"Wrong argument: {aex.ParamName}");
            //    PrintHelp();
            //    return 1;
            //}
            //catch(Exception ex)
            //{
            //    Console.WriteLine($"{ex.Message}");
            //    PrintHelp();
            //    return 1;
            //}

            // create gZip instance using options
            var gZip = GZipArchive.Create(options);

            // event assignment
            gZip.Error += OnError;
            gZip.Completed += OnCompleted;
            gZip.Progress += OnProgress;

            // handle cancelation
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                gZip.Cancel();

                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Opeartion has been cancelled!");
            };

            // print Info
            PrintInfo(options);

            // start process
            return gZip.Process();
        }
        /// <summary>
        /// Ons the completed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        static void OnCompleted(object sender, EventArgs e)
        {
           Console.WriteLine(Environment.NewLine);
           Console.WriteLine("Done!");
        }


        /// <summary>
        /// Ons the progress.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        static void OnProgress(object sender, GZip.Events.ProgressEventArgs e)
        {
            Console.CursorVisible = false;
            Console.CursorLeft = 0;
            Console.Write($"Progress: {e.Progress}%");
        }

        /// <summary>
        /// Ons the error.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        static void OnError(object sender, GZip.Events.ErrorEventArgs e)
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine($"Error: {e.Message}");
        }

        /// <summary>
        /// Prints the info.
        /// </summary>
        /// <param name="options">Options.</param>
        static void PrintInfo(GZipOptions options)
        {
            Console.WriteLine($"GZipTest");
            Console.WriteLine($"=========================================");
            Console.WriteLine($"CPU Cores:   {Environment.ProcessorCount}");
            Console.WriteLine($"RAM Limit:   {options.MemoryLimit / 1024 / 1024} MB");
            Console.WriteLine($"Buffer Size: {options.BufferSize / 1024} Kb");
            Console.WriteLine($"Mode:        {options.Mode}");
            Console.WriteLine($"=========================================");
            Console.WriteLine(Environment.NewLine);
        }

        /// <summary>
        /// Prints the help.
        /// </summary>
        /// <returns>The help.</returns>
        static void PrintHelp()
        {
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("gziptest [mode] [inputfile] [outputfile]");
            Console.WriteLine("  mode       - compress/decompress");
            Console.WriteLine("  inputfile  - input file path");
            Console.WriteLine("  outputfile - output file path");
        }

    }
}
