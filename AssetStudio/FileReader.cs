using System;
using System.IO;
using System.Linq;
using static AssetStudio.ImportHelper;

namespace AssetStudio
{
    public class FileReader : EndianBinaryReader
    {
        public string FullPath;
        public string FileName;
        public FileType FileType;

        private static readonly byte[] gzipMagic = { 0x1f, 0x8b };
        private static readonly byte[] brotliMagic = { 0x62, 0x72, 0x6F, 0x74, 0x6C, 0x69 };
        private static readonly byte[] zipMagic = { 0x50, 0x4B, 0x03, 0x04 };
        private static readonly byte[] zipSpannedMagic = { 0x50, 0x4B, 0x07, 0x08 };
        private static readonly byte[] mhy0Magic = { 0x6D, 0x68, 0x79, 0x30 };
        private static readonly byte[] blbMagic = { 0x42, 0x6C, 0x62, 0x02 };
        private static readonly byte[] narakaMagic = { 0x15, 0x1E, 0x1C, 0x0D, 0x0D, 0x23, 0x21 };
        private static readonly byte[] gunfireMagic = { 0x7C, 0x6D, 0x79, 0x72, 0x27, 0x7A, 0x73, 0x78, 0x3F };


        public FileReader(string path) : this(path, File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

        public FileReader(string path, Stream stream, bool leaveOpen = false) : base(stream, EndianType.BigEndian, leaveOpen)
        {
            FullPath = Path.GetFullPath(path);
            FileName = Path.GetFileName(path);
            FileType = CheckFileType();
        }

        private FileType CheckFileType()
        {
            var signature = this.ReadStringToNull(20);
            Position = 0;
            switch (signature)
            {
                case "ENCR":
                case "UnityWeb":
                case "UnityRaw":
                case "UnityArchive":
                case "UnityFS":
                    return FileType.BundleFile;
                case "UnityWebData1.0":
                    return FileType.WebFile;
                case "blk":
                    return FileType.BlkFile;
                default:
                    {
                        byte[] magic = ReadBytes(2);
                        Position = 0;
                        if (gzipMagic.SequenceEqual(magic))
                        {
                            return FileType.GZipFile;
                        }
                        Position = 0x20;
                        magic = ReadBytes(6);
                        Position = 0;
                        if (brotliMagic.SequenceEqual(magic))
                        {
                            return FileType.BrotliFile;
                        }
                        if (IsSerializedFile())
                        {
                            return FileType.AssetsFile;
                        }
                        magic = ReadBytes(4);
                        Position = 0;
                        if (zipMagic.SequenceEqual(magic) || zipSpannedMagic.SequenceEqual(magic))
                        {
                            return FileType.ZipFile;
                        }
                        if (mhy0Magic.SequenceEqual(magic))
                        {
                            return FileType.Mhy0File;
                        }
                        if (blbMagic.SequenceEqual(magic))
                        {
                            return FileType.BlbFile;
                        }
                        magic = ReadBytes(7);
                        Position = 0;
                        if (narakaMagic.SequenceEqual(magic))
                        {
                            return FileType.BundleFile;
                        }
                        magic = ReadBytes(9);
                        Position = 0;
                        if (gunfireMagic.SequenceEqual(magic))
                        {
                            Position = 0x32;
                            return FileType.BundleFile;
                        }
                        return FileType.ResourceFile;
                    }
            }
        }

        private bool IsSerializedFile()
        {
            var fileSize = BaseStream.Length;
            if (fileSize < 20)
            {
                return false;
            }
            var m_MetadataSize = ReadUInt32();
            long m_FileSize = ReadUInt32();
            var m_Version = ReadUInt32();
            long m_DataOffset = ReadUInt32();
            var m_Endianess = ReadByte();
            var m_Reserved = ReadBytes(3);
            if (m_Version >= 22)
            {
                if (fileSize < 48)
                {
                    Position = 0;
                    return false;
                }
                m_MetadataSize = ReadUInt32();
                m_FileSize = ReadInt64();
                m_DataOffset = ReadInt64();
            }
            Position = 0;
            if (m_FileSize != fileSize)
            {
                return false;
            }
            if (m_DataOffset > fileSize)
            {
                return false;
            }
            return true;
        }
    }

    public static class FileReaderExtensions
    {
        public static FileReader PreProcessing(this FileReader reader, Game game, bool blockFile = true)
        {
            if (reader.FileType == FileType.ResourceFile || !game.Type.IsNormal())
            {
                reader = game.Type switch
                {
                    GameType.GI_Pack => DecryptPack(reader, game),
                    GameType.GI_CB1 => DecryptMark(reader),
                    GameType.EnsembleStars => DecryptEnsembleStar(reader),
                    GameType.OPFP => ParseOPFP(reader),
                    GameType.AlchemyStars => ParseAlchemyStars(reader),
                    GameType.FantasyOfWind => DecryptFantasyOfWind(reader),
                    GameType.ShiningNikki => ParseShiningNikki(reader),
                    GameType.HelixWaltz2 => ParseHelixWaltz2(reader),
                    GameType.AnchorPanic => DecryptAnchorPanic(reader),
                    GameType.DreamscapeAlbireo => DecryptDreamscapeAlbireo(reader),
                    _ => reader
                };
            }
            if (reader.FileType == FileType.BundleFile && game.Type.IsBlockFile() && blockFile || reader.FileType == FileType.BlbFile)
            {
                try
                {
                    var signature = reader.ReadStringToNull();
                    reader.ReadInt32();
                    reader.ReadStringToNull();
                    reader.ReadStringToNull();
                    var size = reader.ReadInt64();
                    if (!(signature == "UnityFS" && size == reader.BaseStream.Length))
                    {
                        reader.FileType = FileType.BlockFile;
                    }
                }
                catch (Exception) { }
                reader.Position = 0;
            }
            switch (reader.FileType)
            {
                case FileType.GZipFile:
                    reader = PreProcessing(DecompressGZip(reader), game);
                    break;
                case FileType.BrotliFile:
                    reader = PreProcessing(DecompressBrotli(reader), game);
                    break;
            }
            return reader;
        }
    } 
}
