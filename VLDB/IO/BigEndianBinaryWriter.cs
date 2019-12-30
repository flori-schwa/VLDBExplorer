using System;
using System.IO;

namespace VLDB.IO {
    public class BigEndianBinaryWriter : IDisposable {

        public void Dispose() {
            _outStream.Dispose();
        }

        private readonly Stream _outStream;
        private readonly byte[] _buffer = new byte[8];

        public BigEndianBinaryWriter(Stream outStream) {
            if (outStream == null) {
                throw new ArgumentException($"{nameof(outStream)} must not be null!");
            }

            if (!outStream.CanWrite) {
                throw new ArgumentException($"{nameof(outStream)} must be writeable!");
            }

            _outStream = outStream;
        }

        #region Unsigned Types

        public void Write(bool value) => _outStream.WriteByte(value ? (byte) 1 : (byte) 0);

        public void Write(byte value) => _outStream.WriteByte(value);

        public void Write(ushort value) {
            _buffer[0] = (byte) ((value >> 8) & 0xFF);
            _buffer[1] = (byte) (value & 0xFF);

            _outStream.Write(_buffer, 0, sizeof(ushort));
        }

        public void Write(uint value) {
            _buffer[0] = (byte) ((value >> (3 * 8)) & 0xFF);
            _buffer[1] = (byte) ((value >> (2 * 8)) & 0xFF);
            _buffer[2] = (byte) ((value >> 8) & 0xFF);
            _buffer[3] = (byte) (value & 0xFF);

            _outStream.Write(_buffer, 0, sizeof(uint));
        }

        public void Write(ulong value) {
            _buffer[0] = (byte) ((value >> (7 * 8)) & 0xFF);
            _buffer[1] = (byte) ((value >> (6 * 8)) & 0xFF);
            _buffer[2] = (byte) ((value >> (5 * 8)) & 0xFF);
            _buffer[3] = (byte) ((value >> (4 * 8)) & 0xFF);
            _buffer[4] = (byte) ((value >> (3 * 8)) & 0xFF);
            _buffer[5] = (byte) ((value >> (2 * 8)) & 0xFF);
            _buffer[6] = (byte) ((value >> 8) & 0xFF);
            _buffer[7] = (byte) (value & 0xFF);

            _outStream.Write(_buffer, 0, sizeof(ulong));
        }

        #endregion

        #region Signed Types

        public void Write(sbyte value) => _outStream.WriteByte((byte) value);

        public void Write(short value) => Write((ushort) value);

        public void Write(int value) => Write((uint) value);
        
        public void Write(float value) => Write(value.SingleToInt32Bits());
        
        public void Write(long value) => Write((ulong) value);
        
        public void Write(double value) => Write(value.DoubleToInt64Bits());

        #endregion

        public void Flush() => _outStream.Flush();
    }
}