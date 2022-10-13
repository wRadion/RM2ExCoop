using g3;
using RM2ExCoop.RM2C.F3DCommands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class ModelData
    {
        public List<List<F3DCommand>> DLs;
        public readonly List<(uint, uint, uint)> Verts;
        public readonly List<TexturePtr> TexturePtrs;
        public readonly List<(uint, uint)> Ambient;
        public readonly List<(uint, uint)> Diffuse;
        public readonly List<List<dynamic>> Ranges;
        public readonly List<(uint, uint)> Start;
        public string? Id;

        public TexturePtr CurrTextPtr => TexturePtrs[^1];

        public ModelData((uint, uint) start0)
        {
            DLs = new() { new() };
            Verts = new();
            TexturePtrs = new() { new TexturePtr() };
            Ambient = new();
            Diffuse = new();
            Ranges = new() { new() { 0, 0, 0, 0, 0, 0 } };
            Start = new() { start0 };
            Id = null;
        }

        public ModelData DecodeDL(Rom rom, Script script, string idPrefix, uint x, Mat lastMat, bool optimize, int dlStack = 0)
        {
            while (true)
            {
                byte[] bytes = rom.GetBytes(Start[dlStack].Item1 + x, 8);
                F3DCommand? cmd = F3DCommand.FromBytes(bytes, idPrefix);

                if (cmd is null)
                    continue;

                // Separate case for set tile since it's special
                if (optimize)
                {
                    string attrKey = cmd.Code.ToString();

                    if (cmd is G_SetTile cmdSetTile && cmdSetTile.Tile == 7)
                        attrKey += '7';

                    if (lastMat.HasAttr(attrKey) && lastMat[attrKey] is F3DCommand attr)
                    {
                        if (attr.ArgsEquals(cmd))
                        {
                            x += 8;
                            continue;
                        }
                        else
                            lastMat.Set(attrKey, cmd);
                    }
                }

                if (cmd is G_DL cmdDL)
                {
                    uint ptr = cmdDL.Segment;
                    x += 8;
                    DLs[dlStack].Add(cmd);
                    DLs.Add(new());
                    Start.Add((script.B2P(ptr), ptr));
                    DecodeDL(rom, script, idPrefix, 0, lastMat, optimize, DLs.Count - 1);

                    if (cmdDL.Store == 1)
                        break;
                }
                else if (cmd is G_EndDL)
                {
                    DLs[dlStack].Add(cmd);
                    break;
                }
                else if (cmd is G_SetOtherModeL cmdSetOtherMode && cmdSetOtherMode.FogShadeA)
                {
                    if (!Globals.Fog)
                        Globals.Fog = true;
                    DLs[dlStack].Add(cmd);
                    x += 8;
                }
                else
                {
                    x += 8;

                    // Concat 2 tri ones to a tri2
                    bool found = false;
                    if (DLs[dlStack].Count > 0)
                    {
                        if (DLs[dlStack].Last() is G_Tri1 tri1 && cmd is G_Tri1 tri2)
                        {
                            DLs[dlStack][^1] = new G_Tri2(tri1, tri2);
                            found = true;
                        }
                    }
                    if (!found)
                        DLs[dlStack].Add(cmd);
                }

                EvalMaterial(cmd, script);
            }

            Ranges[^1][1] = CurrTextPtr.Clone();
            Ranges[^1][2] = (uint)(DLs[dlStack].Count - 1);
            Ranges[^1][3] = 1;
            Ranges[^1][4] = (uint)(DLs[dlStack].Count - 1);

            return this;
        }

        void EvalMaterial(F3DCommand cmd, Script script)
        {
            Dictionary<uint, string> types = new()
            {
                { 0, "RGBA" },
                { 2, "CI" },
                { 3, "IA" },
                { 4, "I" }
            };

            // Check for a RDP cmd or geo mode or texture enable/disable
            if ((cmd.Code & 0xF0) == 0xF0 || cmd is G_SetGeometryMode || cmd is G_Texture)
            {
                if (Ranges[^1][3] == 1)
                {
                    // Keep track of the ranges which mats are used for later optimization
                    Ranges[^1][1] = CurrTextPtr.Clone();

                    // I subtract 1 because len goes ones over the index,
                    // and then I subtract another one because I already appended the mat cmd to the dl
                    Ranges[^1][2] = DLs.Count - 2;

                    Ranges.Add(new List<dynamic>() { DLs.Count - 1, 0, 0, 0, Ranges[^1][5] == 0 ? Ranges[^1][4] : Ranges[^1][5], 0 });
                }
            }

            if (cmd is G_MoveMem cmdLight)
            {
                uint ptr = cmdLight.Segment;
                (uint, uint) tuple = (script.B2P(ptr), ptr);
                if (cmdLight.Index == 0x88)
                    Ambient.Add(tuple);
                else
                    Diffuse.Add(tuple);
            }
            else if (cmd is G_VTX cmdVTX) // gsSPVertex (G_VTX_Decode)
            {
                // Adding stuff to data arrays
                Ranges[^1][5] = DLs.Count - 1;
                uint ptr = cmdVTX.Segment;
                byte length = cmdVTX.Num;
                uint rPtr = script.B2P(ptr);
                Verts.Add(new(ptr, rPtr, (uint)(length + 1)));
            }
            else if (cmd is G_Tri1) // gsSP1Triangle (G_TRI1_Decode)
            {
                // If a triangle is drawn and there is a texture, assume a new one is loaded next
                Ranges[^1][3] = 1;

                if (CurrTextPtr.RawPtr != 0)
                {
                    // Editor or RM used to do solid colors using 1px texels with 0 dimensions UVs
                    // it was really dumb and now I have to deal with this case
                    if (CurrTextPtr.Width == 0)
                        CurrTextPtr.Width = 1;
                    if (CurrTextPtr.Height == 0)
                        CurrTextPtr.Height = 1;

                    // Clear palette for RGBA Textures
                    if (CurrTextPtr.ImgType != "CI")
                        CurrTextPtr.Palette.Clear();

                    TexturePtrs.Add(CurrTextPtr.Clone());
                    CurrTextPtr.RawPtr = 0;
                }
            }
            else if (cmd is G_LoadTLUT cmdLoadTLUT) // gsDPLoadTLUTCmd (G_LOADTLUT_Decode)
            {
                if (CurrTextPtr.Tile == cmdLoadTLUT.Tile)
                    CurrTextPtr.Palette = new List<uint>() { CurrTextPtr.RawPtr, CurrTextPtr.BankPtr };
            }
            else if (cmd is G_SetTileSize cmdSetTileSize) // gsDPSetTileSize (G_SETTILESIZE_Decode)
            {
                CurrTextPtr.Width = (uint)(cmdSetTileSize.Width >> 2) + 1;
                CurrTextPtr.Height = (uint)(cmdSetTileSize.Height >> 2) + 1;
            }
            else if (cmd is G_LoadBlock cmdLoadBlock) // gsDPLoadBlock (G_LOADBLOCK_Decode)
            {
                if (TexturePtrs.Count > 0)
                {
                    uint bpp = CurrTextPtr.Length;
                    CurrTextPtr.Length = (uint)Math.Floor((cmdLoadBlock.Texels + 1) * bpp / 16.0f);
                }
            }
            else if (cmd is G_SetTile cmdSetTile) // gsDPSetTile (G_SETTILE_Decode)
            {
                byte tile = cmdSetTile.Tile;

                if (tile != 7)
                {
                    byte type = cmdSetTile.Fmt;
                    CurrTextPtr.ImgType = types[type];
                    uint bpp = 4 * (uint)Math.Pow(2, cmdSetTile.BitSize);
                    CurrTextPtr.BitDepth = bpp;
                    CurrTextPtr.Tile = tile;
                }
            }
            else if (cmd is G_SetTImg cmdSetTImg) // gsDPSetTextureImage (G_SETTIMG_Decode)
            {
                uint ptr = cmdSetTImg.Segment;
                byte type = cmdSetTImg.Fmt;
                uint bpp = 4 * (uint)Math.Pow(2, cmdSetTImg.BitSize);

                if (types.TryGetValue(type, out string? typeStr))
                {
                    CurrTextPtr.RawPtr = script.B2P(ptr);
                    CurrTextPtr.BankPtr = ptr;
                    CurrTextPtr.Length = bpp;
                    CurrTextPtr.ImgType = typeStr;
                    CurrTextPtr.BitDepth = bpp;
                }
                else
                {
                    if (TexturePtrs.Count > 1)
                        TexturePtrs[^1] = TexturePtrs[^2].Clone();
                }
            }
        }
    }
}
