namespace VLDB {
    public class ChunkPos {
        
        public int X { get; }
        
        public int Z { get; }
        
        public int RegionX { get; }
        
        public int RegionZ { get; }

        public ChunkPos(Position pos) : this(pos.X >> 4, pos.Z >> 4) {
        }
        
        public ChunkPos(int x, int z) {
            X = x;
            Z = z;

            RegionX = X >> 5;
            RegionZ = Z >> 5;
        }

        public override string ToString() {
            return $"({X}, {Z})";
        }
    }
}