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

        protected bool Equals(ChunkPos other) {
            return X == other.X && Z == other.Z;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ChunkPos) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (X * 397) ^ Z;
            }
        }
    }
}