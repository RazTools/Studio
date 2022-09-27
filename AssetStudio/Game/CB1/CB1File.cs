using System.Collections.Generic;
using System.IO;

namespace AssetStudio
{
    public class CB1File
    {
        public Dictionary<long, StreamFile[]> Bundles = new Dictionary<long, StreamFile[]>();
        public CB1File(FileReader reader)
        {
            var data = Mark.Decrypt(reader);

            using (var ms = new MemoryStream(data))
            using (var subReader = new FileReader(reader.FullPath, ms, reader.Game))
            {
                var bundle = new BundleFile(subReader);
                Bundles.Add(0, bundle.FileList);
            }
        }
    }
}
