using System;

namespace AssetStudio
{
    [Flags]
    public enum ExportListType
    {
        None,
        MessagePack,
        XML,
        JSON = 4,
    }
}
