using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NDesk.Options;
using VLDB;
using VLDB.IO;

namespace CLIManager {
    class Program {
        
        /*
         * CLIManager.exe <file> 
         *
         * 
         */

        private class ChunkPosComparer : IComparer<ChunkPos> {
            private IDictionary<ChunkPos, int> _header;
            
            public ChunkPosComparer(IDictionary<ChunkPos, int> header) {
                _header = header;
            }
            
            public int Compare(ChunkPos x, ChunkPos y) {
                return _header[x].CompareTo(_header[y]);
            }
        }

        private class Options {
            public FileInfo File;
            public string GetChunk;
            public bool DisplayHelp;
            public Mode? Mode;
            public bool OldVldb;
        }

        private enum Mode {
            ListAll,
            ListChunks,
            SingleChunk
        }
        
        static void Main(string[] args) {
            Options options = new Options();

            OptionSet optionSet = new OptionSet() {
                {"f|file=", "The target VLDB File", x => options.File = new FileInfo(x)},
                {"h|help", "Show Command help", v => options.DisplayHelp = v != null},
                {"a|all", "List all light sources", v => {
                    if (v != null) {
                        options.Mode = Mode.ListAll;
                    }
                }},
                {"c|list-chunks", "List all affected chunks", v => {
                    if (v != null) {
                        options.Mode = Mode.ListChunks;
                    }
                }},
                {"g|get-chunk=", "List all light sources in specified chunk", v => {
                    if (v != null) {
                        options.Mode = Mode.SingleChunk;
                        options.GetChunk = v;
                    }
                }},
                {"o|old", "Enables support for old VLDB version", v => options.OldVldb = v != null}
            };

            try {
                optionSet.Parse(args);
            }
            catch (OptionException e) {
                Console.Error.WriteLine($"CLIManager: {e.Message}");
                Console.Error.WriteLine("Try running CLIManager --help");
            }

            if (options.DisplayHelp) {
                optionSet.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (options.File == null) {
                Console.Error.WriteLine("No file specified!");
                return;
            }

            if (!options.File.Exists) {
                Console.Error.WriteLine($"File \"{options.File.FullName}\" does not exist!");
                return;
            }

            if (!options.OldVldb && !options.File.IsVldbFile()) {
                Console.Error.WriteLine($"File \"{options.File.FullName}\" is not a VLDB file!");
                return;
            }

            if (options.Mode == null) {
                Console.Error.WriteLine("No Mode specified!");
                return;
            }

            switch (options.Mode) {
                case Mode.ListAll: {
                    ListAll(options.File, options.OldVldb);
                    return;
                }

                case Mode.ListChunks: {
                    ListChunks(options.File, options.OldVldb);
                    return;
                }

                case Mode.SingleChunk: {
                    ListSingleChunk(options.File, options.GetChunk, options.OldVldb);
                    return;
                }
            }
        }

        static void ListAll(FileInfo vldbFile, bool old) {
            using (VLDBReader reader = VLDBReader.OpenVldb(vldbFile, old)) {
                LightSource[] lightSources = reader.ReadAll();

                Console.WriteLine($"============== {vldbFile.Name} ==============");
                Console.WriteLine($"TOTAL LIGHT SOURCES: {lightSources.Length}");
                Console.WriteLine();

                foreach (LightSource lightSource in lightSources) {
                    Console.WriteLine(lightSource);
                }
            }
        }

        static void ListChunks(FileInfo vldbFile, bool old) {
            byte[] fileContents = vldbFile.ReadFileFullyInflate();
            
            using (VLDBReader reader = new VLDBReader(new MemoryStream(fileContents))) {
                if (!old) {
                    reader.verifyVldb();
                }

                int regionX = reader.BaseReader.ReadInt();
                int regionZ = reader.BaseReader.ReadInt();
                
                IDictionary<ChunkPos, int> header = reader.ReadHeader(regionX, regionZ);
                SortedDictionary<ChunkPos, int> sortedHeader = new SortedDictionary<ChunkPos, int>(header, new ChunkPosComparer(header));
                
                Console.WriteLine($"============== {vldbFile.Name} ==============");
                Console.WriteLine($"TOTAL AFFECTED CHUNKS: {header.Keys.Count}");
                Console.WriteLine();
                
                foreach (ChunkPos chunkPos in sortedHeader.Keys) {
                    int offset = header[chunkPos];
                    reader.BaseReader.BaseStream.Position = offset;
                    
                    reader.BaseReader.ReadShort(); // Skip position
                    int amountLightSources = reader.ReadUInt24();

                    Console.WriteLine($"Chunk {chunkPos,10}: {amountLightSources,5} Light sources - OFFSET 0x{$"{header[chunkPos]:X}",-10}");
                }
            }
        }

        static void ListSingleChunk(FileInfo vldbFile, string input, bool old) {
            Regex regex = new Regex(@"-?[0-9]+,-?[0-9]+");

            Match match = regex.Match(input);

            if (!match.Success) {
                throw new Exception("Could not parse chunk coordinates");
            }

            string coords = match.Value;
            string[] parts = coords.Split(',');

            int chunkX = int.Parse(parts[0]);
            int chunkZ = int.Parse(parts[1]);
            
            byte[] fileContents = vldbFile.ReadFileFullyInflate();
            
            ChunkPos targetChunkPos = new ChunkPos(chunkX, chunkZ);

            using (VLDBReader reader = new VLDBReader(new MemoryStream(fileContents))) {
                if (!old) {
                    reader.verifyVldb();
                }

                int regionX = reader.BaseReader.ReadInt();
                int regionZ = reader.BaseReader.ReadInt();

                IDictionary<ChunkPos, int> header = reader.ReadHeader(regionX, regionZ);

                if (!header.ContainsKey(targetChunkPos)) {
                    Console.WriteLine($"File does not contain chunk {targetChunkPos}");
                    return;
                }

                reader.BaseReader.BaseStream.Position = header[targetChunkPos];
                LightSource[] lightSources = reader.ReadChunk(regionX, regionZ).Item2;
                
                Console.WriteLine($"============== {vldbFile.Name} ==============");
                Console.WriteLine($"CHUNK {targetChunkPos}");
                Console.WriteLine($"AMOUNT LIGHT SOURCES: {lightSources.Length}");
                Console.WriteLine();

                foreach (LightSource lightSource in lightSources) {
                    Console.WriteLine(lightSource);
                }
            }
        }
    }
}