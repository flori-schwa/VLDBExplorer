using System;
using System.IO;

namespace VLDB.IO {
    public class BigEndianBinaryReader : IDisposable {
        private Stream _inStream;

        public BigEndianBinaryReader(Stream inStream) {
            if (inStream == null) {
                throw new ArgumentException($"{nameof(inStream)} must not be null!");
            }

            if (!inStream.CanRead) {
                throw new ArgumentException($"{nameof(inStream)} must be readable!");
            }

            _inStream = inStream;
        }

        public Stream BaseStream => _inStream;

        public void Skip(int n) {
            BaseStream.Read(new byte[n]);
        }

        #region Unsigned Types

        public byte ReadByte() {
            int read = _inStream.ReadByte();

            if (read == -1) {
                throw new EndOfStreamException();
            }

            return (byte) read;
        }

        public bool ReadBool() {
            return ReadByte() == 1;
        }

        public ushort ReadUShort() {
            ushort value = 0;

            value |= (ushort) (ReadByte() << 8);
            value |= ReadByte();

            return value;
        }

        public uint ReadUInt() {
            uint value = 0;

            value |= (uint) (ReadByte() << (3 * 8));
            value |= (uint) (ReadByte() << (2 * 8));
            value |= (uint) (ReadByte() << (1 * 8));
            value |= ReadByte();

            return value;
        }

        public ulong ReadULong() {
            ulong value = 0;

            value |= (ulong) ReadByte() << (7 * 8);
            value |= (ulong) ReadByte() << (6 * 8);
            value |= (ulong) ReadByte() << (5 * 8);
            value |= (ulong) ReadByte() << (4 * 8);
            value |= (ulong) ReadByte() << (3 * 8);
            value |= (ulong) ReadByte() << (2 * 8);
            value |= (ulong) ReadByte() << 8;
            value |= ReadByte();

            return value;
        }

        #endregion

        #region Signed Types

        public sbyte ReadSByte() => (sbyte) ReadByte();

        public short ReadShort() => (short) ReadUShort();

        public int ReadInt() => (int) ReadUInt();

        public float ReadFloat() {
            byte[] buffer = new byte[sizeof(float)];

            if (_inStream.Read(buffer) != sizeof(float)) {
                throw new EndOfStreamException();
            }

            Array.Reverse(buffer);

            return BitConverter.ToSingle(buffer);
        }

        public long ReadLong() => (long) ReadULong();

        public double ReadDouble() {
            byte[] buffer = new byte[sizeof(double)];

            if (_inStream.Read(buffer) != sizeof(double)) {
                throw new EndOfStreamException();
            }

            Array.Reverse(buffer);

            return BitConverter.ToDouble(buffer);
        }

        #endregion

        public void Dispose() {
            _inStream.Dispose();
        }
    }
}