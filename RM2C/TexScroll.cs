using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class TexScroll
    {
        public static int ScrollCount = 0;

        public Obj Obj;
        public uint AreaId;
        public uint Addr;
        public int VertCount;
        public short Speed;
        public string Bhv;
        public string Type;
        public int Cycle;

        public TexScroll(Obj obj, uint areaId, uint addr, int vertCount, short speed, string bhv, string type, int cycle)
        {
            Obj = obj;
            AreaId = areaId;
            Addr = addr;
            VertCount = vertCount;
            Speed = speed;
            Bhv = bhv;
            Type = type;
            Cycle = cycle;
        }

        public Obj? FormatScrollObject(Script script)
        {
            // Not efficient at all, but number of scrolls is low and I'm lazy
            // vert = [seg ptr, rom ptr, num verts], sorted by seg ptrs
            if (script.Verts.Count == 0)
                return null;

            uint closest = 0;
            long offset = 0;

            // If verts are not in order, I can falsely assume the vert does not exist
            // because I see a gap and mistake it for the end of an area or something.
            List<(uint, uint, uint)> verts = script.Verts.OrderBy(v => v.Item1).ToList();

            foreach ((uint, uint, uint) vert in verts)
            {
                if (Addr >= vert.Item1)
                {
                    closest = vert.Item1;
                    offset = Addr - vert.Item1;
                }
                else
                {
                    if (offset > 0xF0)
                    {
                        offset = 0xFF0;
                        Logger.Warn($"Texture Scroll Object in level {script.CurrLevel.Name} area {AreaId} at {Utils.Hex(Addr)} has unrecognized address.");
                    }
                    break;
                }
            }

            if (verts.Count == 0)
            {
                closest = (uint)Addr;
                offset = 0xFF0;
                Logger.Warn($"Texture Scroll Object in level {script.CurrLevel.Name} area {AreaId} at {Utils.Hex(Addr)} has unrecognized address.");
            }

            Dictionary<string, short> bhvs = new()
            {
                { "x", 4 },
                { "y", 5 },
                { "xPos", 0 },
                { "yPos", 1 },
                { "zPos", 2 }
            };

            Dictionary<string, short> types = new()
            {
                { "normal", 0 },
                { "sine", 1 },
                { "jumping", 2 }
            };

            // Format I will use is bparam=addr, z=vert count, x=spd, y=bhv, ry=type, rz=cycle
            Obj obj = new(Obj.ModelId,
                Speed, // x
                bhvs[Bhv], // y
                (short)VertCount, // z
                (short)(offset / 0x10), // rx
                types[Type], // ry
                (short)Cycle, // rz
                ScrollCount++.ToString(), // bparam
                Obj.BhvName,
                Obj.ActMask);

            script.ScrollArray.Add(($"VB_{script.CurrLevel.Name}_{AreaId}_0x{Utils.Hexx(closest)}", (int)(offset / (float)0x10)));

            return Obj;
        }

        public static Obj Convert(Script script, Obj obj, Rom rom)
        {
            return script.Editor ? ConvertEditor(script, obj, rom) : ConvertRM(script, obj);
        }

        public static Obj ConvertEditor(Script script, Obj obj, Rom rom)
        {
            // Editor rules
            // Verts scrolled = 0x0E000000+(Byte2(Zpos)-2)<<16+(bparam>>16)
            // Verts addr = Verts scrolled&0xFFFFFFF0
            // Verts axis = Verts scrolled&0xF (0x8 = x, 0xA = y)
            // Num verts scrolled = Byte2(Zpos)*3
            // Speed=Byte2(Zpos)
            // I have zero clue if this is true for all editor versions
            // it likely isn't
            if (obj.BhvName.Contains("editor_Scroll_Texture2"))
                obj.BhvName = "editor_Scroll_Texture";

            uint a = System.Convert.ToUInt32(obj.BParam, 16) >> 16;
            int b = (Utils.PosByte(obj.X) - 2) << 16;
            uint addr = (uint)(0x0E000000 + b + a);
            int num = 0;

            if (rom.DetScrollType)
            {
                // Different format used in later versions of editor
                // Num=Bparam34
                num = (int)(a & 0xFFFF);
            }
            else if (obj.Y != 0)
                num = Utils.PosByte(obj.Y) * 3;

            string dir = (addr & 0xF) == 0x8 ? "x" : "y";
            byte speed = Utils.PosByte(obj.Z);

            script.TexScrolls.Add(new TexScroll(obj, script.CurrArea.GetValueOrDefault(), addr & 0xFFFFFFF0, num, speed, dir, "normal", 0));

            return obj;
        }

        public static Obj ConvertRM(Script script, Obj obj)
        {
            // RM rules
            // Verts addr = bparam
            // Verts axis = Y&0xF000 (0x8000 - Y, 0xA000 - X, 0x4000 - Z, 0x2000 - Y, 0x0000 - X)
            // Scroll Type = Y&0F00 (0x000 - normal, 0x0100 - sine, 0x0200 - jumping)
            // Speed= Z pos
            // NumVerts = X
            uint addr = System.Convert.ToUInt32(obj.BParam, 16);
            int num = obj.X;
            short dir = obj.Y;
            short speed = obj.Z;

            Dictionary<int, string> bhvs = new()
            {
                { 0xA000, "x" },
                { 0x8000, "y" },
                { 0x4000, "xPos" },
                { 0x2000, "yPos" },
                { 0x0000, "zPos" }
            };

            Dictionary<int, string> types = new()
            {
                { 0x0, "normal" },
                { 0x100, "sine" },
                { 0x200, "jumping" }
            };

            int bhv = dir & 0xF000;
            int type = dir & 0xF00;

            if (bhvs.ContainsKey(bhv) && types.ContainsKey(type))
                script.TexScrolls.Add(new TexScroll(obj, script.CurrArea.GetValueOrDefault(), addr, num, speed, bhvs[bhv], types[type], dir & 0xFF));
            else
                Logger.Error("Unknown Scrolling Texture behavior or type");

            return obj;
        }
    }
}
