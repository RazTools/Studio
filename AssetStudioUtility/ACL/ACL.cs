using System;
using System.Runtime.InteropServices;
using AssetStudio.PInvoke;

namespace ACL
{
    public struct DecompressedClip
    {
        public IntPtr Values;
        public int ValuesCount;
        public IntPtr Times;
        public int TimesCount;
    }
    public static class ACL
    {
        private const string DLL_NAME = "acl";
        static ACL()
        {
            DllLoader.PreloadDll(DLL_NAME);
        }
        public static void DecompressAll(byte[] data, out float[] values, out float[] times)
        {
            var decompressedClip = new DecompressedClip();
            DecompressAll(data, ref decompressedClip);

            values = new float[decompressedClip.ValuesCount];
            Marshal.Copy(decompressedClip.Values, values, 0, decompressedClip.ValuesCount);

            times = new float[decompressedClip.TimesCount];
            Marshal.Copy(decompressedClip.Times, times, 0, decompressedClip.TimesCount);

            Dispose(ref decompressedClip);
        }

        #region importfunctions

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DecompressAll(byte[] data, ref DecompressedClip decompressedClip);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Dispose(ref DecompressedClip decompressedClip);

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
            var decompressedClip = new DecompressedClip();
            DecompressAll(data, ref decompressedClip);

            values = new float[decompressedClip.ValuesCount];
            Marshal.Copy(decompressedClip.Values, values, 0, decompressedClip.ValuesCount);

            times = new float[decompressedClip.TimesCount];
            Marshal.Copy(decompressedClip.Times, times, 0, decompressedClip.TimesCount);

            Dispose(ref decompressedClip);
        }

        #region importfunctions

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void DecompressAll(uint[] data, ref DecompressedClip decompressedClip);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Dispose(ref DecompressedClip decompressedClip);

        #endregion
    }
}
