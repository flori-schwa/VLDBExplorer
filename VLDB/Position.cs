namespace VLDB {
    public class Position {
        public int X { get; }
        
        public int Y { get; }

        public int Z { get; }

        public ChunkPos Chunk { get; }

        public int ChunkX => Chunk.X;
        
        public int ChunkZ => Chunk.Z;

        public int RegionX => Chunk.RegionX;

        public int RegionZ => Chunk.RegionZ;

        public Position(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
            
            Chunk = new ChunkPos(this);
        }

        public override string ToString() {
            return $"[{X}, {Y}, {Z}]";
        }
    }
}