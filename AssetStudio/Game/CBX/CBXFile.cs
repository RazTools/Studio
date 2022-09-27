using System.Collections.Generic;
using System.IO;

namespace AssetStudio
{
    public class CBXFile
    {
        public Dictionary<long, StreamFile[]> Bundles = new Dictionary<long, StreamFile[]>();
        public CBXFile(FileReader reader)
        {
            var data = Blk.Decrypt(reader);

            using (var ms = new MemoryStream(data))
            using (var subReader = new FileReader(reader.FullPath, ms, reader.Game))
            {
                if (subReader.BundlePos.Length != 0)
                {
                    foreach (var pos in subReader.BundlePos)
                    {
                        subReader.Position = pos;
                        var bundle = new BundleFile(subReader);
                        Bundles.Add(pos, bundle.FileList);
                    }
                }
                else
                {
                    while (subReader.Position != subReader.BaseStream.Length)
                    {
                        var pos = subReader.Position;
                        var bundle = new BundleFile(subReader);
                        if (bundle.FileList == null)
                            continue;
                        Bundles.Add(pos, bundle.FileList);
                    }
                }
            }
        }
    }
}
