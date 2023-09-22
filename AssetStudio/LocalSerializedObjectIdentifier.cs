using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class LocalSerializedObjectIdentifier
    {
        public int localSerializedFileIndex;
        public long localIdentifierInFile;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"localSerializedFileIndex: {localSerializedFileIndex} | ");
            sb.Append($"localIdentifierInFile: {localIdentifierInFile}");
            return sb.ToString();
        }
    }
}
