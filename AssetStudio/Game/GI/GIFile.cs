using System;
using System.Collections.Generic;
using System.IO;

namespace AssetStudio
{
    public class GIFile
    {
        public Dictionary<long, StreamFile[]> Bundles = new Dictionary<long, StreamFile[]>();
        public GIFile(FileReader reader)
        {
            var data = Blk.Decrypt(reader);

            using (var ms = new MemoryStream(data))
            using (var subReader = new EndianBinaryReader(ms, reader.Endian))
            {
                long pos = -1;
                try
                {
                    if (reader.BundlePos.Length != 0)
                    {
                        for (int i = 0; i < reader.BundlePos.Length; i++)
                        {
                            pos = reader.BundlePos[i];
                            subReader.Position = pos;
                            var mhy0 = new Mhy0File(subReader, reader.FullPath);
                            Bundles.Add(pos, mhy0.FileList);
                        }
                    }
                    else
                    {
                        while (subReader.Position != subReader.BaseStream.Length)
                        {
                            pos = subReader.Position;
                            var mhy0 = new Mhy0File(subReader, reader.FullPath);
                            Bundles.Add(pos, mhy0.FileList);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load a mhy0 at {string.Format("0x{0:x8}", pos)} in {Path.GetFileName(reader.FullPath)}");
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
