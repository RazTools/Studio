using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AssetStudio
{
    public static class UnityCNManager
    {
        private static List<UnityCN.Entry> Entries = new List<UnityCN.Entry>();

        static UnityCNManager()
        {
            Entries = JsonConvert.DeserializeObject<List<UnityCN.Entry>>(Properties.Settings.Default.keys);
        }

        public static void SaveEntries(List<UnityCN.Entry> entries)
        {
            Entries.Clear();
            Entries.AddRange(entries);

            var str = JsonConvert.SerializeObject(Entries);
            Properties.Settings.Default.keys = str;
        }

        public static void SetKey(int index)
        {
            if (TryGetEntry(index, out var unityCN))
            {
                if (UnityCN.SetKey(unityCN))
                {
                    Logger.Info($"[UnityCN] Selected Key is {unityCN}");
                }
                else
                {
                    Logger.Info($"[UnityCN] No Key is selected !!");
                }
            }
            else
            {
                Logger.Error("Invalid Key !!");
                Logger.Warning(GetEntries().Select(x => x.ToString()).ToString());
            }
        }

        public static bool TryGetEntry(int index, out UnityCN.Entry key)
        {
            try
            {
                if (index < 0 || index > Entries.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                key = Entries[index];
            }
            catch(Exception e)
            {
                Logger.Error($"[UnityCN] Invalid Index, check if list is not empty !!\n{e.Message}");
                key = null;
                return false;
            }

            return true;
        }
        public static UnityCN.Entry[] GetEntries() => Entries.ToArray();

        public new static string ToString() => string.Join("\n", GetEntries().Select((x, i) => $"{i}: {x.Name}"));
    }
}
