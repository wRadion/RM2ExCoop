namespace RM2ExCoop.RM2C
{
    internal class Obj
    {
        public byte ModelId;
        public short X;
        public short Y;
        public short Z;
        public short RX;
        public short RY;
        public short RZ;
        public string BParam;
        public string BhvName;
        public byte ActMask;

        public Obj(byte modelId, short x, short y, short z, short rX, short rY, short rZ, string bParam, string bhvName, byte actMask)
        {
            ModelId = modelId;
            X = x;
            Y = y;
            Z = z;
            RX = rX;
            RY = rY;
            RZ = rZ;
            BParam = bParam;
            BhvName = bhvName;
            ActMask = actMask;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;
            if (obj is not Obj other)
                return false;
            if (other == this)
                return true;
            return ModelId == other.ModelId && X == other.X && Y == other.Y && Z == other.Z &&
                RX == other.RX && RY == other.RY && RZ == other.RZ && BParam == other.BParam &&
                BhvName == other.BhvName && ActMask == other.ActMask;
        }

        public override string ToString() => $"{ModelId},{X},{Y},{Z},{RX},{RY},{RZ},{BParam},{BhvName},{ActMask}";

        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }
    }
}
