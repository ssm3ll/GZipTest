using System.Runtime.InteropServices;

namespace Veeam.GZip
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GZipHeader
    {
        public byte ID1 { get; set; }
        public byte ID2 { get; set; }
        public byte CM { get; set; }
        public GZipFlags FLG { get; set; }
        public uint MTIME { get; set; }
        public byte XFL { get; set; }
        public GZipOS OS { get; set; }
    }
}
