using System;
using System.IO;
using VLDB;
using VLDB.IO;

namespace Test {
    class Program {

        static void Main(string[] args) {

            FileInfo testFile =
                new FileInfo(@"E:\Florian\Projekte\IdeaProjects\VarLight\test-servers\1.15\world\varlight\r.0.0.vldb2");

            FileInfo testFileDeflated =
                new FileInfo(
                    @"E:\Florian\Projekte\IdeaProjects\VarLight\test-servers\1.8.8\world\varlight\r.-2.-1.vldb2");
            
            Console.WriteLine(testFile.IsVldbFile());
            Console.WriteLine(testFileDeflated.IsVldbFile());

            VLDBReader reader;

            using (reader = VLDBReader.OpenVldb(testFile, false)) {
                int regionX = reader.BaseReader.ReadInt();
                int regionZ = reader.BaseReader.ReadInt();
                
                Console.WriteLine(reader.ReadHeader(regionX, regionZ).DictToString());
            }
            
            using (reader = VLDBReader.OpenVldb(testFileDeflated, false)) {
                int regionX = reader.BaseReader.ReadInt();
                int regionZ = reader.BaseReader.ReadInt();
                
                Console.WriteLine(reader.ReadHeader(regionX, regionZ).DictToString());
            }

            Console.WriteLine("==============================");

            using (reader = VLDBReader.OpenVldb(testFile, false)) {
                LightSource[] lightSources = reader.ReadAll();

                Console.WriteLine(testFile.Name);
                foreach (LightSource lightSource in lightSources) {
                    Console.WriteLine(lightSource);
                }

                Console.WriteLine("-----------------------");
            }
            
            using (reader = VLDBReader.OpenVldb(testFileDeflated, false)) {
                LightSource[] lightSources = reader.ReadAll();

                Console.WriteLine(testFileDeflated.Name);
                foreach (LightSource lightSource in lightSources) {
                    Console.WriteLine(lightSource);
                }

                Console.WriteLine("-----------------------");
            }
        }
    }
}