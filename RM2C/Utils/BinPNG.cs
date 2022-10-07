using ImageMagick;
using System;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class BinPNG
    {
        public static MagickImage RGBA16(int width, int height, BitStream bin) => RGBA(width, height, 16, bin);
        public static MagickImage IA4(int width, int height, BitStream bin) => IA(width, height, 4, bin);
        public static MagickImage IA8(int width, int height, BitStream bin) => IA(width, height, 8, bin);
        public static MagickImage IA16(int width, int height, BitStream bin) => IA(width, height, 16, bin);

        public static MagickImage RGBA(int width, int height, uint depth, BitStream bin, (byte[], string)? _ = null)
        {
            byte[] channelSizes = depth switch
            {
                16 => new byte[] { 5, 5, 5, 1 },
                32 => new byte[] { 8, 8, 8, 8 },
                _ => throw new NotSupportedException($"RGBA{depth} is unsupported")
            };

            return Generic(width, height, bin, channelSizes);
        }

        public static MagickImage IA(int width, int height, uint depth, BitStream bin, (byte[], string)? _ = null)
        {
            byte[] channelSizes = depth switch
            {
                4 => new byte[] { 3, 1 },
                8 => new byte[] { 4, 4 },
                16 => new byte[] { 8, 8 },
                _ => throw new NotSupportedException($"IA{depth} is unsupported")
            };

            return Generic(width, height, bin, channelSizes);
        }

        public static MagickImage CI(int width, int height, uint depth, BitStream bin, (byte[], string)? pal)
        {
            // TODO: Handled not supported depth (only 4 or 8 is accepted)

            MagickImage image = new()
            {
                Format = MagickFormat.Png32,
                Interlace = Interlace.NoInterlace,
            };

            byte[][] palette = new byte[pal.Value.Item1.Length / 2][];

            BitStream palBin = new(pal.Value.Item1);
            for (int i = 0; i < palette.Length; ++i)
            {
                palette[i] = new byte[4] {
                    palBin.ReadByte(5),
                    palBin.ReadByte(5),
                    palBin.ReadByte(5),
                    palBin.ReadByte(1)
                };
            }

            byte[] pixels = new byte[width * height * 4];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelId = y * (width * 4) + (x * 4);
                    byte id = bin.ReadByte((int)depth);

                    palette[id].CopyTo(pixels, pixelId);
                }
            }

            PixelReadSettings settings = new(width, height, StorageType.Char, PixelMapping.RGBA);
            image.ReadPixels(pixels, settings);

            return image;
        }

        static MagickImage Generic(int width, int height, BitStream bin, byte[] channelSizes)
        {
            MagickImage image = new()
            {
                Format = MagickFormat.Png32,
                Interlace = Interlace.NoInterlace,
            };

            PixelReadSettings settings = new(width, height, StorageType.Char, PixelMapping.RGBA);
            image.ReadPixels(BinToRGBA32(width, height, bin, channelSizes), settings);

            return image;
        }

        // BinPNG.py : EditFile
        static byte[] BinToRGBA32(int width, int height, BitStream bin, byte[] channelSizes)
        {
            byte[] pixels = new byte[width * height * 4];

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    int pixelId = y * (width * 4) + (x * 4);

                    byte[] sourceValues = channelSizes.Select(bitSize =>
                    {
                        // Convert the value to 0 <= x <= 255
                        return (byte)Math.Round(bin.ReadByte(bitSize) * 255 / (Math.Pow(2, bitSize) - 1));
                    }).ToArray();

                    if (sourceValues.Length == 2)
                    {
                        sourceValues = new byte[4]
                        {
                            sourceValues[0],
                            sourceValues[0],
                            sourceValues[0],
                            sourceValues[1]
                        };
                    }

                    sourceValues.CopyTo(pixels, pixelId);
                }
            }

            return pixels;
        }
    }
}
