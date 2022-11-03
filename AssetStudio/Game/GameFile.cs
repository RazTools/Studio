using System;
using System.IO;
using System.Collections.Generic;

namespace AssetStudio
{
    public class GameFile
    {
        public Dictionary<long, StreamFile[]> Bundles = new Dictionary<long, StreamFile[]>();

        public GameFile(FileReader reader)
        {
            var bundles = new Dictionary<long, StreamFile[]>();
            var ext = Path.GetExtension(reader.FileName);
            switch (reader.Game.Name)
            {
                case "GI":
                    var gi = GameManager.GetGame("GI");
                    if (ext != gi.Extension)
                        goto default;

                    Blk.ExpansionKey = Crypto.GIExpansionKey;
                    Blk.SBox = Crypto.GISBox;
                    Blk.ConstKey = Crypto.GIConstKey;
                    Blk.ConstVal = Crypto.GIConstVal;

                    var giFile = new GIFile(reader);
                    bundles = giFile.Bundles;
                    break;
                case "GI_CB1":
                    var gi_cb1 = GameManager.GetGame("GI_CB1");
                    if (ext != gi_cb1.Extension)
                        goto default;

                    var gi_cb1File = new CB1File(reader);
                    bundles = gi_cb1File.Bundles;
                    break;
                case "GI_CB2":
                    var gi_cb2 = GameManager.GetGame("GI_CB2");
                    if (ext != gi_cb2.Extension)
                        goto default;

                    Blk.ExpansionKey = Crypto.CBXExpansionKey;
                    Blk.SBox = null;
                    Blk.ConstKey = Crypto.CBXConstKey;
                    Blk.ConstVal = Crypto.CBXConstVal;

                    var gi_cb2File = new CBXFile(reader);
                    bundles = gi_cb2File.Bundles;
                    break;
                case "GI_CB3":
                    var gi_cb3 = GameManager.GetGame("GI_CB3");
                    if (ext != gi_cb3.Extension)
                        goto default;

                    Blk.ExpansionKey = Crypto.CBXExpansionKey;
                    Blk.SBox = null;
                    Blk.ConstKey = Crypto.CBXConstKey;
                    Blk.ConstVal = Crypto.CBXConstVal;

                    var gi_cb3File = new CBXFile(reader);
                    bundles = gi_cb3File.Bundles;
                    break;
                case "BH3":
                    var bh3 = GameManager.GetGame("BH3");
                    if (ext != bh3.Extension)
                        goto default;

                    Mr0k.ExpansionKey = Crypto.BH3ExpansionKey;
                    Mr0k.Key = Crypto.BH3Key;
                    Mr0k.ConstKey = Crypto.BH3ConstKey;
                    Mr0k.SBox = Crypto.BH3SBox;
                    Mr0k.BlockKey = null;

                    var wmvFile = new WMVFile(reader);
                    bundles = wmvFile.Bundles;
                    break;
                case "ZZZ_CB1":
                    var zzz = GameManager.GetGame("ZZZ_CB1");
                    if (ext != zzz.Extension)
                        goto default;

                    Mr0k.ExpansionKey = Crypto.Mr0kExpansionKey;
                    Mr0k.Key = Crypto.Mr0kKey;
                    Mr0k.ConstKey = Crypto.Mr0kConstKey;
                    Mr0k.SBox = null;
                    Mr0k.BlockKey = null;

                    var zzzFile = new BundleFile(reader);
                    bundles.Add(0, zzzFile.FileList);
                    break;
                case "SR_CB2":
                    var sr_cb2 = GameManager.GetGame("SR_CB2");
                    if (ext != sr_cb2.Extension)
                        goto default;

                    Mr0k.ExpansionKey = Crypto.Mr0kExpansionKey;
                    Mr0k.Key = Crypto.Mr0kKey;
                    Mr0k.ConstKey = Crypto.Mr0kConstKey;
                    Mr0k.SBox = null;
                    Mr0k.BlockKey = null;

                    var sr_cb2File = new BundleFile(reader);
                    bundles.Add(0, sr_cb2File.FileList);
                    break;
                case "SR_CB3":
                    var sr_cb3 = GameManager.GetGame("SR_CB3");
                    if (ext != sr_cb3.Extension)
                        goto default;

                    Mr0k.ExpansionKey = Crypto.Mr0kExpansionKey;
                    Mr0k.Key = Crypto.Mr0kKey;
                    Mr0k.ConstKey = Crypto.Mr0kConstKey;
                    Mr0k.SBox = null;
                    Mr0k.BlockKey = null;

                    var sr_cb3File = new BlocksFile(reader);
                    bundles = sr_cb3File.Bundles;
                    break;
                case "TOT":
                    var tot = GameManager.GetGame("TOT");
                    if (ext != tot.Extension)
                        goto default;

                    Mr0k.ExpansionKey = Crypto.Mr0kExpansionKey;
                    Mr0k.Key = Crypto.Mr0kKey;
                    Mr0k.ConstKey = Crypto.Mr0kConstKey;
                    Mr0k.SBox = null;
                    Mr0k.BlockKey = Crypto.ToTBlockKey;

                    var totFile = new TOTFile(reader);
                    bundles = totFile.Bundles;
                    break;
                default:
                    throw new NotSupportedException("File not supported !!\nMake sure to select the right game before loading the file");
            }
            Bundles = bundles;
        }
    }
}
