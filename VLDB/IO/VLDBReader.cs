using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace VLDB.IO {
    public class VLDBReader : IDisposable {
        private readonly BigEndianBinaryReader _baseReader;

        public static VLDBReader OpenVldb(FileInfo fileInfo) {
            Stream fIn;

            if (fileInfo.IsDeflated()) {
                fIn = new GZipStream(fileInfo.OpenRead(), CompressionMode.Decompress);
            }
            else {
                fIn = fileInfo.OpenRead();
            }

            VLDBReader reader = new VLDBReader(fIn);
            reader.verifyVldb();

            return reader;
        }

        public VLDBReader(BigEndianBinaryReader baseReader) {
            _baseReader = baseReader;
        }

        public VLDBReader(Stream stream) {
            _baseReader = new BigEndianBinaryReader(stream);
        }

        public void verifyVldb() {
            if (_baseReader.ReadInt() != Util.VldbMagic) {
                throw new Exception("Couldn't identify VLDB magic!");
            }
        }

        public BigEndianBinaryReader BaseReader => _baseReader;

        public IDictionary<ChunkPos, int> ReadHeader(int regionX, int regionZ) {
            int amountChunks = _baseReader.ReadShort();

            IDictionary<ChunkPos, int> header = new Dictionary<ChunkPos, int>();

            for (int i = 0; i < amountChunks; i++) {
                int encodedCoords = _baseReader.ReadUShort();
                int offset = _baseReader.ReadInt();

                int chunkX = ((encodedCoords & 0xFF00) >> 8) + regionX * 32;
                int chunkZ = (encodedCoords & 0xFF) + regionZ * 32;

                header[new ChunkPos(chunkX, chunkZ)] = offset;
            }

            return new ReadOnlyDictionary<ChunkPos, int>(header);
        }

        public LightSource[] ReadAll() {
            int regionX = _baseReader.ReadInt();
            int regionZ = _baseReader.ReadInt();

            int amountChunks = _baseReader.ReadShort();

            IList<LightSource> list = new List<LightSource>();

            _baseReader.Skip(amountChunks * (sizeof(ushort) + sizeof(int)));

            for (int i = 0; i < amountChunks; i++) {
                foreach (LightSource lightSource in ReadChunk(regionX, regionZ).Item2) {
                    list.Add(lightSource);
                }
            }

            return list.ToArray();
        }

        public (ChunkPos, LightSource[]) ReadChunk(int regionX, int regionZ) {
            int encodedCoords = _baseReader.ReadUShort();

            int chunkX = ((encodedCoords & 0xFF00) >> 8) + regionX * 32;
            int chunkZ = (encodedCoords & 0xFF) + regionZ * 32;

            int amountLightSources = ReadUInt24();

            LightSource[] lightSources = new LightSource[amountLightSources];

            for (int i = 0; i < amountLightSources; i++) {
                int coords = _baseReader.ReadShort();
                byte data = _baseReader.ReadByte();
                string material = ReadASCII();

                Position position = new Position(
                    chunkX * 16 + ((coords & 0xF000) >> 12),
                    (coords & 0x0FF0) >> 4,
                    chunkZ * 16 + (coords & 0xF)
                );

                int luminance = (data & 0xF0) >> 4;
                bool migrated = (data & 0x0F) != 0;

                lightSources[i] = new LightSource(position, material, migrated, luminance);
            }

            return (new ChunkPos(chunkX, chunkZ), lightSources);
        }

        public int ReadUInt24() {
            return _baseReader.ReadByte() << 16 | _baseReader.ReadByte() << 8 | _baseReader.ReadByte();
        }

        // ReSharper disable once InconsistentNaming
        public string ReadASCII() {
            byte[] asciiBuffer = new byte[_baseReader.ReadShort()];
            _baseReader.BaseStream.Read(asciiBuffer, 0, asciiBuffer.Length);

            return Encoding.ASCII.GetString(asciiBuffer);
        }

        public void Dispose() {
            _baseReader?.Dispose();
        }
    }
}