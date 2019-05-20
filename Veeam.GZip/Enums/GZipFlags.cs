using System;

namespace Veeam.GZip
{
    [Flags]
    public enum GZipFlags : byte
    {
        None = 0,
        FTEXT = 1,
        FHCRC = 2,
        FEXTRA = 4,
        FNAME = 8,
        FCOMMENT = 16,
        Reserved1 = 32,
        Reserved2 = 64,
        Reserved3 = 128,
    }

    public enum GZipOS : byte
    {
        FAT = 0,
        Amiga = 1,
        VMS_OpenVMS = 2,
        Unix = 3,
        VM_CMS = 4,
        Atari_TOS = 5,
        HPFS = 6,
        Macintosh = 7,
        Z_System = 8,
        CP_M = 9,
        TOPS_20 = 10,
        NTFS = 11,
        QDOS = 12,
        Acorn_RISCOS = 13,
        Unknown = 255,
    }
}
