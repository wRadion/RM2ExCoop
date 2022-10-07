using System.Collections.Generic;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class TexturePtr
    {
        public uint RawPtr;
        public uint BankPtr;
        public uint Length;
        public uint Width;
        public uint Height;
        public string ImgType;
        public uint BitDepth;
        public List<uint> Palette;
        public byte Tile;


        public TexturePtr(uint rawPtr = 0, uint bankPtr = 0, uint length = 0, uint width = 0, uint height = 0, string imgType = "", uint bitDepth = 0, byte tile = 0)
            : this(new List<uint>())
        { }

        public TexturePtr(List<uint> palette, uint rawPtr = 0, uint bankPtr = 0, uint length = 0, uint width = 0, uint height = 0, string imgType = "", uint bitDepth = 0, byte tile = 0)
        {
            RawPtr = rawPtr;
            BankPtr = bankPtr;
            Length = length;
            Width = width;
            Height = height;
            ImgType = imgType;
            BitDepth = bitDepth;
            Palette = palette.ToList();
            Tile = tile;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not TexturePtr other)
                return false;

            return RawPtr == other.RawPtr && BankPtr == other.BankPtr;
        }

        public override int GetHashCode() => RawPtr.GetHashCode() ^ BankPtr.GetHashCode();

        public TexturePtr Clone() => new(Palette, RawPtr, BankPtr, Length, Width, Height, ImgType, BitDepth, Tile);
    }
}
