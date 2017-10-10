namespace VcEngineAutomation.Panels
{
    public class Position
    {

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Position Offset(Position other)
        {
            return new Position(X + other.X, Y + other.Y, Z + other.Z);
        }
        public Position Offset(double x, double y, double z)
        {
            return new Position(X + x, Y + y, Z + z);
        }

        public override string ToString()
        {
            return $"{X}; {Y}; {Z}";
        }

        private bool Equals(Position other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Position)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

        public Position() { }
        public Position(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}