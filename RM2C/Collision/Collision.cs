using System.IO;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class Collision
    {
        public static void Write(string filePath, Script script, Rom rom, uint start, string idPrefix)
        {
            using StreamWriter colFile = new(File.Open(filePath, FileMode.Append));
            CollisionData colData = new();

            var (b, index) = WriteGeneric(colFile, colData, script, rom, start, idPrefix);

            WriteLevelSpecial(colFile, rom, b, index);
        }

        public static (long, long) WriteActor(string filePath, Script script, Rom rom, uint start, string idPrefix)
        {
            using StreamWriter colFile = new(File.Open(filePath, FileMode.Append));
            CollisionData colData = new();

            var (b, index) = WriteGeneric(colFile, colData, script, rom, start, idPrefix);

            colFile.WriteLine("COL_END(),");
            colFile.WriteLine("};");

            long fileOffset = colFile.BaseStream.Position;
            long l2 = colData.Verts.Count + colData.DPV.Count;

            return (fileOffset, l2);
        }

        static (uint, uint) WriteGeneric(StreamWriter colFile, CollisionData colData, Script script, Rom rom, uint start, string idPrefix)
        {
            colFile.WriteLine($"const Collision col_{idPrefix}{Utils.Hex(start)}[] = {{");
            colFile.WriteLine("COL_INIT(),");

            uint b = script.B2P(start);
            ushort vnum = rom.GetUInt16(b + 2);

            b += 4;

            for (int i = 0; i < vnum; ++i)
                colData.Verts.Add(Vector3iUtil.ReadRom(rom, b + i * 6));

            uint index = 0;
            b += (uint)(vnum * 6);

            while (true)
            {
                ushort type = rom.GetUInt16(b + index);
                ushort count = rom.GetUInt16(b + index + 2);

                if (type == 0x41 || index > 132000)
                    break;

                colData.InitType(type);

                // Special tri with param
                if (CollisionData.Specials.Contains(type))
                {
                    for (int j = 0; j < count; ++j)
                        colData.Tris[type].Add(ColTriangle.ReadRom(rom, b + index + j * 8 + 4, true));
                    index += (uint)(count * 8 + 4);
                }
                else
                {
                    for (int j = 0; j < count; ++j)
                    {
                        ColTriangle tri = ColTriangle.ReadRom(rom, b + index + j * 6 + 4, false);

                        // Normals for death place aren't proper thanks editor
                        if (type == 10)
                            tri.CheckNorm(rom, script.B2P(start) + 4);

                        colData.Tris[type].Add(tri);
                    }
                    index += (uint)(count * 6 + 4);
                }
            }

            colData.Write(colFile);

            // Figure out
            return (b, index);
        }

        public static void WriteLevelSpecial(StreamWriter colFile, Rom rom, uint b, uint index)
        {
            b += index + 2;

            while (true)
            {
                short type = rom.GetInt16(b);
                short count = rom.GetInt16(b + 2);

                if (type == 0x42) // End
                {
                    colFile.WriteLine("COL_END(),");
                    colFile.WriteLine("};");
                    break;
                }
                else if (type == 0x44) // Water
                {
                    b += 4;

                    colFile.WriteLine($"COL_WATER_BOX_INIT({count}),");
                    for (int i = 0; i < count; ++i)
                    {
                        var water1 = Vector3iUtil.ReadRom(rom, b);
                        b += 6;
                        var water2 = Vector3iUtil.ReadRom(rom, b);
                        b += 6;
                        colFile.WriteLine($"COL_WATER_BOX({water1.x}, {water1.y}, {water1.z}, {water2.x}, {water2.y}, {water2.z}),");
                    }
                }
                else // Neither
                {
                    // Something is wrong, just exit
                    // TODO: Log the error in the console, or smth?
                    colFile.WriteLine("COLD_END(),");
                    colFile.WriteLine("};");
                    break;
                }
            }
        }
    }
}
