namespace VLDB {
    public class LightSource {
        
        public Position Position { get; }
        
        public string Type { get; }
        
        public bool Migrated { get; }
        
        public int Luminance { get; }

        public LightSource(Position position, string type, bool migrated, int luminance) {
            Position = position;
            Type = type;
            Migrated = migrated;
            Luminance = luminance;
        }

        public override string ToString() {
            return $"{(Migrated ? "MIGRATED" : "NOT MIGRATED")} {Type} {Position} = {Luminance}";
        }
    }
}