using System.Runtime.InteropServices;

namespace AssetStudio
{
    public static partial class ConsoleHelper
    {
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool AllocConsole();

        [LibraryImport("kernel32.dll", EntryPoint = "SetConsoleTitleA", SetLastError = true, StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool SetConsoleTitle(string lpConsoleTitle);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        public static partial nint GetConsoleWindow();

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ShowWindow(nint hWnd, int nCmdShow);

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 5;
    }
}
