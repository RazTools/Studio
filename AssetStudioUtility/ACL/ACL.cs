using System;
using System.Runtime.InteropServices;
using AssetStudio.PInvoke;

namespace ACL
{
    public static class ACL
    {
        private const string DLL_NAME = "acl";
        static ACL()
        {
            DllLoader.PreloadDll(DLL_NAME);
        }
        public static void DecompressAll(byte[] data, out float[] values, out float[] times)
        {
            var pinned = GCHandle.Alloc(data, GCHandleType.Pinned);
            var pData = pinned.AddrOfPinnedObject();
            DecompressAll(pData, out var pValues, out var numValues, out var pTimes, out var numTimes);
            pinned.Free();

            values = new float[numValues];
            Marshal.Copy(pValues, values, 0, numValues);

            times = new float[numTimes];
            Marshal.Copy(pTimes, times, 0, numTimes);
        }

        #region importfunctions

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DecompressAll(IntPtr data, out IntPtr pValues, out int numValues, out IntPtr pTimes, out int numTimes);

        #endregion
    }

    public static class SRACL
    {
        private const string DLL_NAME = "sracl";
        static SRACL()
        {
            DllLoader.PreloadDll(DLL_NAME);
        }
        public static void DecompressAll(uint[] data, out float[] values, out float[] times)
        {
            var pinned = GCHandle.Alloc(data, GCHandleType.Pinned);
            var pData = pinned.AddrOfPinnedObject();
            DecompressAll(pData, out var pValues, out var numValues, out var pTimes, out var numTimes);
            pinned.Free();

            values = new float[numValues];
            Marshal.Copy(pValues, values, 0, numValues);

            times = new float[numTimes];
            Marshal.Copy(pTimes, times, 0, numTimes);
        }

        #region importfunctions

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DecompressAll(IntPtr data, out IntPtr pValues, out int numValues, out IntPtr pTimes, out int numTimes);

        #endregion
    }
}
