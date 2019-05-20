using System;
using System.Runtime.InteropServices;

namespace Veeam.GZip.Helpers
{
    public static class Interop
    {
        /// <summary>
        /// Srtructs to array.
        /// </summary>
        /// <returns>The to array.</returns>
        /// <param name="struc">Struc.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static byte[] SrtructToArray<T>(T struc) where T: struct
        {
            int structSize = Marshal.SizeOf(struc);
            byte[] structArr = new byte[structSize];

            IntPtr structPtr = Marshal.AllocHGlobal(structSize);
            Marshal.StructureToPtr(struc, structPtr, true);
            Marshal.Copy(structPtr, structArr, 0, structSize);
            Marshal.FreeHGlobal(structPtr);

            return structArr;
        }
    }
}
