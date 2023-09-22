using System;
namespace AssetStudio
{
    public static class Progress
    {
        public static bool Silent = false;
        public static IProgress<int> Default = new Progress<int>();
        private static int preValue;

        public static void Reset()
        {
            if (!Silent)
            {
                preValue = 0;
                Default.Report(0);
            }
        }

        public static void Report(int current, int total)
        {
            if (!Silent)
            {
                var value = (int)(current * 100f / total);
                Report(value);
            }
        }

        private static void Report(int value)
        {
            if (value > preValue)
            {
                preValue = value;
                Default.Report(value);
            }
        }
    }
}
