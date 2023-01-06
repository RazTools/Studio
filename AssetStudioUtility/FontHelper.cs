using System.Runtime.InteropServices;

namespace AssetStudio
{
    public static partial class FontHelper
    {
        [LibraryImport("gdi32.dll")]
        public static partial nint AddFontMemResourceEx(nint pbFont, uint cbFont, nint pdv, ref uint pcFonts);
    }
}
