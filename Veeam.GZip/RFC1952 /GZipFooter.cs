using System;
using System.Runtime.InteropServices;

namespace Veeam.GZip
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GZipFooter
    {
        public uint Crc32 { get; set; }
        public uint ISize { get; set; }
    }
}
