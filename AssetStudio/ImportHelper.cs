using Org.Brotli.Dec;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static AssetStudio.BundleFile;
using static AssetStudio.Crypto;

namespace AssetStudio
{
    public static class ImportHelper
    {
        public static void MergeSplitAssets(string path, bool allDirectories = false)
        {
            var splitFiles = Directory.GetFiles(path, "*.split0", allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (var splitFile in splitFiles)
            {
                var destFile = Path.GetFileNameWithoutExtension(splitFile);
                var destPath = Path.GetDirectoryName(splitFile);
                var destFull = Path.Combine(destPath, destFile);
                if (!File.Exists(destFull))
                {
                    var splitParts = Directory.GetFiles(destPath, destFile + ".split*");
                    using (var destStream = File.Create(destFull))
                    {
                        for (int i = 0; i < splitParts.Length; i++)
                        {
                            var splitPart = destFull + ".split" + i;
                            using (var sourceStream = File.OpenRead(splitPart))
                            {
                                sourceStream.CopyTo(destStream);
                            }
                        }
                    }
                }
            }
        }

        public static string[] ProcessingSplitFiles(List<string> selectFile)
        {
            var splitFiles = selectFile.Where(x => x.Contains(".split"))
                .Select(x => Path.Combine(Path.GetDirectoryName(x), Path.GetFileNameWithoutExtension(x)))
                .Distinct()
                .ToList();
            selectFile.RemoveAll(x => x.Contains(".split"));
            foreach (var file in splitFiles)
            {
                if (File.Exists(file))
                {
                    selectFile.Add(file);
                }
            }
            return selectFile.Distinct().ToArray();
        }

        public static FileReader DecompressGZip(FileReader reader)
        {
            using (reader)
            {
                var stream = new MemoryStream();
                using (var gs = new GZipStream(reader.BaseStream, CompressionMode.Decompress))
                {
                    gs.CopyTo(stream);
                }
                stream.Position = 0;
                return new FileReader(reader.FullPath, stream);
            }
        }

        public static FileReader DecompressBrotli(FileReader reader)
        {
            using (reader)
            {
                var stream = new MemoryStream();
                using (var brotliStream = new BrotliInputStream(reader.BaseStream))
                {
                    brotliStream.CopyTo(stream);
                }
                stream.Position = 0;
                return new FileReader(reader.FullPath, stream);
            }
        }

        public static FileReader DecryptPack(FileReader reader, Game game)
        {
            const int PackSize = 0x880;
            const string PackSignature = "pack";
            const string UnityFSSignature = "UnityFS";
           
            var data = reader.ReadBytes((int)reader.Length);
            var idx = data.Search(PackSignature);
            if (idx == -1)
            {
                reader.Position = 0;
                return reader;
            }
            idx = data.Search("mr0k", idx);
            if (idx == -1)
            {
                reader.Position = 0;
                return reader;
            }

            var ms = new MemoryStream();
            try
            {
                var mr0k = (Mr0k)game;

                long readSize = 0;
                long bundleSize = 0;
                reader.Position = 0;
                while (reader.Remaining > 0)
                {
                    var pos = reader.Position;
                    var signature = reader.ReadStringToNull(4);
                    if (signature == PackSignature)
                    {
                        var isMr0k = reader.ReadBoolean();
                        var blockSize = BinaryPrimitives.ReadInt32LittleEndian(reader.ReadBytes(4));

                        Span<byte> buffer = new byte[blockSize];
                        reader.Read(buffer);
                        if (isMr0k)
                        {
                            buffer = Mr0kUtils.Decrypt(buffer, mr0k);
                        }
                        ms.Write(buffer);

                        if (bundleSize == 0)
                        {
                            using var blockReader = new EndianBinaryReader(new MemoryStream(buffer.ToArray()));
                            var header = new Header()
                            {
                                signature = blockReader.ReadStringToNull(),
                                version = blockReader.ReadUInt32(),
                                unityVersion = blockReader.ReadStringToNull(),
                                unityRevision = blockReader.ReadStringToNull(),
                                size = blockReader.ReadInt64()
                            };
                            bundleSize = header.size;
                        }

                        readSize += buffer.Length;

                        if (readSize % (PackSize - 0x80) == 0)
                        {
                            reader.Position += PackSize - 9 - blockSize;
                        }

                        if (readSize == bundleSize)
                        {
                            readSize = 0;
                            bundleSize = 0;
                        }

                        continue;
                    }

                    reader.Position = pos;
                    signature = reader.ReadStringToNull();
                    if (signature == UnityFSSignature)
                    {
                        var header = new Header()
                        {
                            signature = reader.ReadStringToNull(),
                            version = reader.ReadUInt32(),
                            unityVersion = reader.ReadStringToNull(),
                            unityRevision = reader.ReadStringToNull(),
                            size = reader.ReadInt64()
                        };

                        reader.Position = pos;
                        reader.BaseStream.CopyTo(ms, header.size);
                        continue;
                    }
                    
                    throw new InvalidOperationException($"Expected signature {PackSignature} or {UnityFSSignature}, got {signature} instead !!");
                }
            }
            catch (InvalidCastException)
            {
                Logger.Error($"Game type mismatch, Expected {nameof(GameType.GI_Pack)} ({nameof(Mr0k)}) but got {game.Name} ({game.GetType().Name}) !!");
            }
            catch (Exception e)
            {
                Logger.Error($"Error while reading pack file {reader.FullPath}", e);
            }
            finally
            {
                reader.Dispose();
            }

            ms.Position = 0;
            return new FileReader(reader.FullPath, ms);
        }

        public static FileReader DecryptMark(FileReader reader)
        {
            var signature = reader.ReadStringToNull(4);
            if (signature != "mark")
            {
                reader.Position = 0;
                return reader;
            }

            const int BlockSize = 0xA00;
            const int ChunkSize = 0x264;
            const int ChunkPadding = 4;

            var blockPadding = ((BlockSize / ChunkSize) + 1) * ChunkPadding;
            var chunkSizeWithPadding = ChunkSize + ChunkPadding;
            var blockSizeWithPadding = BlockSize + blockPadding;

            var index = 0;
            var block = new byte[blockSizeWithPadding];
            var chunk = new byte[chunkSizeWithPadding];
            var dataStream = new MemoryStream();
            while (reader.BaseStream.Length != reader.BaseStream.Position)
            {
                var readBlockBytes = reader.Read(block);
                using var blockStream = new MemoryStream(block, 0, readBlockBytes);
                while (blockStream.Length != blockStream.Position)
                {
                    var readChunkBytes = blockStream.Read(chunk);
                    if (readBlockBytes == blockSizeWithPadding || readChunkBytes == chunkSizeWithPadding)
                    {
                        readChunkBytes -= ChunkPadding;
                    }
                    for (int i = 0; i < readChunkBytes; i++)
                    {
                        chunk[i] ^= MarkKey[index++ % MarkKey.Length];
                    }
                    dataStream.Write(chunk, 0, readChunkBytes);
                }
            }

            reader.Dispose();
            dataStream.Position = 0;
            return new FileReader(reader.FullPath, dataStream);
        }

        public static FileReader DecryptEnsembleStar(FileReader reader)
        {
            if (Path.GetExtension(reader.FileName) != ".z")
            {
                return reader;
            }
            using (reader)
            {
                var data = reader.ReadBytes((int)reader.Length);
                var count = data.Length;

                var stride = count % 3 + 1;
                var remaining = count % 7;
                var size = remaining + ~(count % 3) + EnsembleStarKey2.Length;
                for (int i = 0; i < count; i += stride)
                {
                    var offset = i / stride;
                    var k1 = offset % EnsembleStarKey1.Length;
                    var k2 = offset % EnsembleStarKey2.Length;
                    var k3 = offset % EnsembleStarKey3.Length;

                    data[i] = (byte)(EnsembleStarKey1[k1] ^ ((size ^ EnsembleStarKey3[k3] ^ data[i] ^ EnsembleStarKey2[k2]) + remaining));
                }

                return new FileReader(reader.FullPath, new MemoryStream(data));
            }
        }

        public static FileReader ParseOPFP(FileReader reader)
        {
            var stream = reader.BaseStream;
            var data = reader.ReadBytes(0x1000);
            var idx = data.Search("UnityFS");
            if (idx != -1)
            {
                stream = new OffsetStream(stream, idx);
            }

            return new FileReader(reader.FullPath, stream);
        }

        public static FileReader ParseAlchemyStars(FileReader reader)
        {
            var stream = reader.BaseStream;
            var data = reader.ReadBytes(0x1000);
            var idx = data.Search("UnityFS");
            if (idx != -1)
            {
                var idx2 = data[(idx + 1)..].Search("UnityFS");
                if (idx2 != -1)
                {
                    stream = new OffsetStream(stream, idx + idx2 + 1);
                }
                else
                {
                    stream = new OffsetStream(stream, idx);
                }
            }

            return new FileReader(reader.FullPath, stream);
        }
        
        public static FileReader DecryptFantasyOfWind(FileReader reader)
        {
            byte[] encryptKeyName = Encoding.UTF8.GetBytes("28856");
            const int MinLength = 0xC8;
            const int KeyLength = 8;
            const int EnLength = 0x32;
            const int StartEnd = 0x14;
            const int HeadLength = 5;

            var signature = reader.ReadStringToNull(HeadLength);
            if (string.Compare(signature, "K9999") > 0 || reader.Length <= MinLength)
            {
                reader.Position = 0;
                return reader;
            }

            reader.Position = reader.Length + ~StartEnd;
            var keyLength = reader.ReadByte();
            reader.Position = reader.Length - StartEnd - 2;
            var enLength = reader.ReadByte();

            var enKeyPos = (byte)((keyLength % KeyLength) + KeyLength);
            var encryptedLength = (byte)((enLength % EnLength) + EnLength);

            reader.Position = reader.Length - StartEnd - enKeyPos;
            var encryptKey = reader.ReadBytes(KeyLength);

            var subByte = (byte)(reader.Length - StartEnd - KeyLength - (keyLength % KeyLength));
            for (var i = 0; i < KeyLength; i++)
            {
                if (encryptKey[i] == 0)
                {
                    encryptKey[i] = (byte)(subByte + i);
                }
            }

            var key = new byte[encryptKeyName.Length + KeyLength];
            encryptKeyName.CopyTo(key.AsMemory(0));
            encryptKey.CopyTo(key.AsMemory(encryptKeyName.Length));

            reader.Position = HeadLength;
            var data = reader.ReadBytes(encryptedLength);
            for (int i = 0; i < encryptedLength; i++)
            {
                data[i] ^= key[i % key.Length]; 
            }

            MemoryStream ms = new();
            ms.Write(Encoding.UTF8.GetBytes("Unity"));
            ms.Write(data);
            reader.BaseStream.CopyTo(ms);
            ms.Position = 0;

            return new FileReader(reader.FullPath, ms);
        }

        public static FileReader ParseShiningNikki(FileReader reader)
        {
            var data = reader.ReadBytes(0x1000);
            var idx = data.Search("UnityFS");
            if (idx == -1)
            {
                reader.Position = 0;
                return reader;
            }
            var stream = new OffsetStream(reader.BaseStream, idx);
            return new FileReader(reader.FullPath, stream);
        }
        public static FileReader ParseHelixWaltz2(FileReader reader)
        {
            var originalHeader = new byte[] { 0x55, 0x6E, 0x69, 0x74, 0x79, 0x46, 0x53, 0x00, 0x00, 0x00, 0x00, 0x07, 0x35, 0x2E, 0x78, 0x2E };

            var signature = reader.ReadStringToNull();
            reader.AlignStream();

            if (signature != "SzxFS")
            {
                reader.Position = 0;
                return reader;
            }

            var seed = reader.ReadInt32();
            reader.Position = 0x10;
            var data = reader.ReadBytes((int)reader.Remaining);

            var sbox = new byte[0x100];
            for (int i = 0; i < sbox.Length; i++)
            {
                sbox[i] = (byte)i;
            }

            var key = new byte[0x100];
            var random = new Random(seed);
            for (int i = 0; i < key.Length; i++)
            {
                var idx = random.Next(i, 0x100);
                var b = sbox[idx];
                sbox[idx] = sbox[i];
                sbox[i] = b;
                key[b] = (byte)i;
            }

            for (int i = 0; i < data.Length; i++)
            {
                var idx = data[i];
                data[i] = key[idx]; 
            }

            MemoryStream ms = new();
            ms.Write(originalHeader);
            ms.Write(data);
            ms.Position = 0;

            return new FileReader(reader.FullPath, ms);
        }
        public static FileReader DecryptAnchorPanic(FileReader reader)
        {
            const int BlockSize = 0x800;

            var data = reader.ReadBytes(0x1000);
            reader.Position = 0;

            var idx = data.Search("UnityFS");
            if (idx != -1)
            {
                return ParseAlchemyStars(reader);
            }

            var key = GetKey(Path.GetFileNameWithoutExtension(reader.FileName));

            var chunkIndex = 0;
            MemoryStream ms = new();
            while (reader.Remaining > 0)
            {
                var chunkSize = Math.Min((int)reader.Remaining, BlockSize);
                var chunk = reader.ReadBytes(chunkSize);
                if (IsEncrypt((int)reader.Length, chunkIndex++))
                    RC4(chunk, key);

                ms.Write(chunk);
            }

            ms.Position = 0;

            return new FileReader(reader.FullPath, ms);

            bool IsEncrypt(int fileSize, int chunkIndex)
            {
                const int MaxEncryptChunkIndex = 4;

                if (chunkIndex == 0)
                    return true;

                if (fileSize / BlockSize == chunkIndex)
                    return true;

                if (MaxEncryptChunkIndex < chunkIndex)
                    return false;

                return fileSize % 2 == chunkIndex % 2;
            }
            
            byte[] GetKey(string fileName)
            {
                const string Key = "KxZKZolAT3QXvsUU";

                string keyHash = CalculateMD5(Key);
                string nameHash = CalculateMD5(fileName);
                var key = $"{keyHash[..5]}leiyan{nameHash[Math.Max(0, nameHash.Length - 5)..]}";
                return Encoding.UTF8.GetBytes(key);

                string CalculateMD5(string str)
                {
                    var bytes = Encoding.UTF8.GetBytes(str);
                    bytes = MD5.HashData(bytes);
                    return Convert.ToHexString(bytes).ToLowerInvariant();
                }
            }

            void RC4(Span<byte> data, byte[] key)
            {
                int[] S = new int[0x100];
                for (int _ = 0; _ < 0x100; _++)
                {
                    S[_] = _;
                }

                int[] T = new int[0x100];

                if (key.Length == 0x100)
                {
                    Buffer.BlockCopy(key, 0, T, 0, key.Length);
                }
                else
                {
                    for (int _ = 0; _ < 0x100; _++)
                    {
                        T[_] = key[_ % key.Length];
                    }
                }

                int i = 0;
                int j = 0;
                for (i = 0; i < 0x100; i++)
                {
                    j = (j + S[i] + T[i]) % 0x100;

                    (S[j], S[i]) = (S[i], S[j]);
                }

                i = j = 0;
                for (int iteration = 0; iteration < data.Length; iteration++)
                {
                    i = (i + 1) % 0x100;
                    j = (j + S[i]) % 0x100;

                    (S[j], S[i]) = (S[i], S[j]);
                    var K = (uint)S[(S[j] + S[i]) % 0x100];

                    data[iteration] ^= Convert.ToByte(K);
                }
            }
        }

        public static FileReader DecryptDreamscapeAlbireo(FileReader reader)
        {
            var key = new byte[] { 0x1E, 0x1E, 0x01, 0x01, 0xFC };

            var signature = reader.ReadStringToNull(4);
            if (signature != "MJJ")
            {
                reader.Position = 0;
                return reader;
            }

            reader.Endian = EndianType.BigEndian;

            var u1 = reader.ReadUInt32();
            var u2 = reader.ReadUInt32();
            var u3 = reader.ReadUInt32();
            var u4 = reader.ReadUInt32();
            var u5 = reader.ReadUInt32();
            var u6 = reader.ReadUInt32();

            var flag = Scrample(u4) ^ 0x70020017;
            var compressedBlocksInfoSize = Scrample(u1) ^ u4;
            var uncompressedBlocksInfoSize = Scrample(u6) ^ u1;

            var sizeHigh = (u5 & 0xFFFFFF00 | u2 >> 24) ^ u4;
            var sizeLow = (u5 >> 24 | (u2 << 8)) ^ u1;
            var size = (long)(sizeHigh << 32 | sizeLow);

            var blocksInfo = reader.ReadBytes((int)compressedBlocksInfoSize);
            for(int i = 0; i < blocksInfo.Length; i++)
            {
                blocksInfo[i] ^= key[i % key.Length];
            }

            var data = reader.ReadBytes((int)reader.Remaining);

            var buffer = (stackalloc byte[8]);
            MemoryStream ms = new();
            ms.Write(Encoding.UTF8.GetBytes("UnityFS\x00"));
            BinaryPrimitives.WriteUInt32BigEndian(buffer, 6);
            ms.Write(buffer[..4]);
            ms.Write(Encoding.UTF8.GetBytes("5.x.x\x00"));
            ms.Write(Encoding.UTF8.GetBytes("2018.4.2f1\x00"));
            BinaryPrimitives.WriteInt64BigEndian(buffer, size);
            ms.Write(buffer);
            BinaryPrimitives.WriteUInt32BigEndian(buffer, compressedBlocksInfoSize);
            ms.Write(buffer[..4]);
            BinaryPrimitives.WriteUInt32BigEndian(buffer, uncompressedBlocksInfoSize);
            ms.Write(buffer[..4]);
            BinaryPrimitives.WriteUInt32BigEndian(buffer, flag);
            ms.Write(buffer[..4]);
            ms.Write(blocksInfo);
            ms.Write(data);
            reader.BaseStream.CopyTo(ms);
            ms.Position = 0;

            return new FileReader(reader.FullPath, ms);

            static uint Scrample(uint value) => (value >> 5) & 0xFFE000 | (value >> 29) | (value << 14) & 0xFF000000 | (8 * value) & 0x1FF8;
        }
    }
}
