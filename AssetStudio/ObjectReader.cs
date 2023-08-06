using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public class ObjectReader : EndianBinaryReader
    {
        public SerializedFile assetsFile;
        public Game Game;
        public ObjectInfo objInfo;
        public BuildTarget platform;
        public SerializedFileFormatVersion m_Version;

        public int[] version => assetsFile.version;
        public BuildType buildType => assetsFile.buildType;
        public long m_PathID => objInfo.m_PathID;
        public long byteStart => objInfo.byteStart;
        public uint byteSize => objInfo.byteSize;
        public ClassIDType type => objInfo.type;
        public SerializedType serializedType => objInfo.serializedType;

        public ObjectReader(EndianBinaryReader reader, SerializedFile assetsFile, ObjectInfo objectInfo, Game game) : base(reader.BaseStream, reader.Endian)
        {
            this.assetsFile = assetsFile;
            Game = game;
            objInfo = objectInfo;
            platform = assetsFile.m_TargetPlatform;
            m_Version = assetsFile.header.m_Version;
        }

        public void Reset()
        {
            Position = byteStart;
        }
    }
}
