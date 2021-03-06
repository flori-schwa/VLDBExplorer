﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using VLDB.IO;

namespace VLDB {
    public static class Util {

        public const int VldbMagic = 0x56_4C_44_42;
        
        public static unsafe uint SingleToInt32Bits(this float f) {
            return *(uint*) &f;
        }

        public static unsafe ulong DoubleToInt64Bits(this double d) {
            return *(ulong*) &d;
        }

        public static bool IsDeflated(this FileInfo fileInfo) {
            bool deflated = false;
            
            using (FileStream fIn = fileInfo.OpenRead()) {
                byte[] shortBuffer = new byte[2];
                fIn.Read(shortBuffer, 0, 2);

                if (BitConverter.ToUInt16(shortBuffer, 0) == 0x8B_1F) {
                    if (fIn.ReadByte() == 0x08) {
                        deflated = true;
                    }
                }
            }

            return deflated;
        }

        public static byte[] ReadFileFullyInflate(this FileInfo fileInfo) {
            MemoryStream fileBuffer = new MemoryStream();
            byte[] readBuffer = new byte[1024];

            Stream inStream;

            if (fileInfo.IsDeflated()) {
                inStream = new GZipStream(fileInfo.OpenRead(), CompressionMode.Decompress);
            }
            else {
                inStream = fileInfo.OpenRead();
            }

            using (inStream) {
                int read;

                while ((read = inStream.Read(readBuffer, 0, readBuffer.Length)) > 0) {
                    fileBuffer.Write(readBuffer, 0, read);
                }
            }

            return fileBuffer.ToArray();
        }

        public static bool IsVldbFile(this FileInfo fileInfo) {
            BigEndianBinaryReader fIn;
            
            if (fileInfo.IsDeflated()) {
                fIn = new BigEndianBinaryReader(new GZipStream(fileInfo.OpenRead(), CompressionMode.Decompress));
            }
            else {
                fIn = new BigEndianBinaryReader(fileInfo.OpenRead());
            }

            using (fIn) {
                return fIn.ReadInt() == Util.VldbMagic;
            }
        }

        public static string DictToString<K, V>(this IDictionary<K, V> dict) {
            return $"{{{string.Join("; ", dict.Select(x => x.Key + "=" + x.Value).ToArray())}}}";
        }
    }
}