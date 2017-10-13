namespace VcEngineAutomation.Panels
{
    public class Rotation
    {
        public Rotation() { }
        public Rotation(double rX, double rY, double rZ)
        {
            Rx = rX;
            Ry = rY;
            Rz = rZ;
        }

        public double Rz { get; set; }

        public double Ry { get; set; }

        public double Rx { get; set; }


        protected bool Equals(Rotation other)
        {
            return Rx.Equals(other.Rx) && Ry.Equals(other.Ry) && Rz.Equals(other.Rz);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Rotation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Rx.GetHashCode();
                hashCode = (hashCode * 397) ^ Ry.GetHashCode();
                hashCode = (hashCode * 397) ^ Rz.GetHashCode();
                return hashCode;
            }
        }
    }
}