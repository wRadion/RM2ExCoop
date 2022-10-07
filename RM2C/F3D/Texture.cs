using ImageMagick;
using System;
using System.Collections.Generic;

namespace RM2ExCoop.RM2C
{
    internal class Texture
    {
        readonly TexturePtr _ptr;
        readonly byte[] _texels;
        readonly string _path;
        readonly (byte[], string)? _palette;

        public Texture(TexturePtr ptr, byte[] texels, string path, (byte[], string)? palette = null)
        {
            _ptr = ptr;
            _texels = texels;
            _path = path + ".png";
            _palette = palette;
        }

        // F3D.py : WriteTex
        public void Write()
        {
            Func<int, int, uint, BitStream, (byte[], string)?, MagickImage> func = _ptr.ImgType switch
            {
                "RGBA" => BinPNG.RGBA, // 16 or 32
                "CI" => BinPNG.CI,   // 4 or 8
                "IA" => BinPNG.IA,   // 4 or 8 or 16
                //"I" => BinPNG.I,     // 4 or 8
                _ => throw new KeyNotFoundException("Texture is not of supported image type")
            };

            MagickImage image = func((int)_ptr.Width, (int)_ptr.Height, _ptr.BitDepth, new BitStream(_texels), _palette);
            image.Write(_path);
        }
    }
}
