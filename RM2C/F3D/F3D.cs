using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RM2ExCoop.RM2C.F3DCommands;

namespace RM2ExCoop.RM2C
{
    internal class F3D
    {
        public static bool IsCommand(byte code)
        {
            return code switch
            {
                0x03 => true,
                0xB6 => true,
                0xB7 => true,
                0xB9 => true,
                0xBA => true,
                0xBB => true,
                0xBC => true,
                0xEF => true,
                0xF7 => true,
                0xF8 => true,
                0xF9 => true,
                0xFA => true,
                0xFB => true,
                0xFC => true,
                0xFE => true,
                0xFF => true,
                // Non-Persist
                0x01 => true,
                0x04 => true,
                0x06 => true,
                0xB3 => true,
                0xB4 => true,
                0xB8 => true,
                0xBD => true,
                0xBF => true,
                0xE4 => true,
                0xE5 => true,
                0xE6 => true,
                0xE7 => true,
                0xE8 => true,
                0xE9 => true,
                0xEA => true,
                0xEB => true,
                0xEC => true,
                0xED => true,
                0xEE => true,
                0xF0 => true,
                0xF2 => true,
                0xF3 => true,
                0xF4 => true,
                0xF5 => true,
                0xF6 => true,
                0xFD => true,
                // Useless
                0x00 => true,
                0xC0 => true,
                _ => false
            };
        }

        public static F3DCommand GetCommand(byte code, byte[] bytes, string idPrefix)
        {
            F3DCommand command = code switch
            {
                // Persist
                0x03 => new G_MoveMem(code, "gsSPLight"),
                0xB6 => new G_ClearGeometryMode(code, "gsSPGeometryMode"),
                0xB7 => new G_SetGeometryMode(code, "gsSPGeometryMode"),
                0xB9 => new G_SetOtherModeL(code, "gsSPSetOtherMode"),
                0xBA => new G_SetOtherModeH(code, "gsSPSetOtherMode"),
                0xBB => new G_Texture(code, "gsSPTexture"),
                0xBC => new G_MoveWord(code, "gsMoveWd"),
                0xEF => new G_RDPSetOtherMode(code, "G_RDPSETOTHERMODE"),
                0xF7 => new G_Color(code, "gsDPSetFillColor"),
                0xF8 => new G_Color(code, "gsDPSetFogColor"),
                0xF9 => new G_Color(code, "gsDPSetBlendColor"),
                0xFA => new G_SetPrimColor(code, "gsDPSetPrimColor"),
                0xFB => new G_Color(code, "gsDPSetEnvColor"),
                0xFC => new G_SetCombine(code, "gsDPSetCombineLERP"),
                0xFE => new G_SetZImg(code, "gsDPSetDepthImage"),
                0xFF => new G_SetCImg(code, "gsDPSetColorImage"),
                // Non-Persist
                0x01 => new G_MTX(code, "gsSPMatrix"),
                0x04 => new G_VTX(code, "gsSPVertex"),
                0x06 => new G_DL(code, "gsSPBranchList"),
                0xB3 => new G_RDPHalf2(code, "G_RDPHALF_2"),
                0xB4 => new G_RDPHalf1(code, "G_RDPHALF_1"),
                0xB8 => new G_EndDL(code, "gsSPEndDisplayList"),
                0xBD => new G_PopMTX(code, "gsSPPopMatrix"),
                0xBF => new G_Tri1(code, "gsSP1Triangle"),
                0xE4 => new G_TexRect(code, "G_TEXRECT"),
                0xE5 => new G_TexRect(code, "G_TEXRECTFLIP"),
                0xE6 => new G_SNoOp(code, "gsDPLoadSync"),
                0xE7 => new G_SNoOp(code, "gsDPPipeSync"),
                0xE8 => new G_SNoOp(code, "gsDPTileSync"),
                0xE9 => new G_SNoOp(code, "gsDPFullSync"),
                0xEA => new G_SetKeyGB(code, "G_SETKEYGB"),
                0xEB => new G_SetKeyR(code, "G_SETKEYR"),
                0xEC => new G_SetConvert(code, "G_SETCONVERT"),
                0xED => new G_SetScissor(code, "G_SETSCISSOR"),
                0xEE => new G_SetPrimDepth(code, "gsDPSetPrimDepth"),
                0xF0 => new G_LoadTLUT(code, "gsDPLoadTLUTCmd"),
                0xF2 => new G_SetTileSize(code, "gsDPSetTileSize"),
                0xF3 => new G_LoadBlock(code, "gsDPLoadBlock"),
                0xF4 => new G_LoadTile(code, "gsDPSetTile"),
                0xF5 => new G_SetTile(code, "gsDPSetTile"),
                0xF6 => new G_FillRect(code, "G_FILLRECT"),
                0xFD => new G_SetTImg(code, "gsDPSetTextureImage"),
                // Useless
                0x00 => new G_SNoOp(code, "gsDPNoOp"),
                0xC0 => new G_SNoOp(code, "gsDPNoOp"),
                _ => throw new NotImplementedException("Unhandled F3D command: " + code)
            };

            BitStream bin = new(bytes);
            command.DecodeAndSetArgs(bin, idPrefix);

            return command;
        }

        public static ModelData DecodeVDL(Rom rom, (uint, uint) start0, Script script, string idPrefix, bool optimize)
        {
            ModelData dls = new(start0);
            uint x = 0;

            Globals.Cycle = 1;
            Globals.Fog = false;

            Mat lastMat = new(new Dictionary<byte, F3DCommand>()
            {
                { 0x03, new G_MoveMem(0x03, "gsSPLight") },
                { 0xB6, new G_ClearGeometryMode(0xB6, "gsSPGeometryMode") },
                { 0xB7, new G_SetGeometryMode(0xB7, "gsSPGeometryMode") },
                { 0xB9, new G_SetOtherModeL(0xB9, "gsSPSetOtherMode") },
                { 0xBA, new G_SetOtherModeH(0xBA, "gsSPSetOtherMode") },
                { 0xBB, new G_Texture(0xBB, "gsSPTexture") },
                { 0xBC, new G_MoveWord(0xBC, "gsMoveWd") },
                { 0xEF, new G_RDPSetOtherMode(0xEF, "G_RDPSETOTHERMODE") },
                { 0xF7, new G_Color(0xF7, "gsDPSetFillColor") },
                { 0xF8, new G_Color(0xF8, "gsDPSetFogColor") },
                { 0xF9, new G_Color(0xF9, "gsDPSetBlendColor") },
                { 0xFA, new G_SetPrimColor(0xFA, "gsDPSetPrimColor") },
                { 0xFB, new G_Color(0xFB, "gsDPSetEnvColor") },
                { 0xFC, new G_SetCombine(0xFC, "gsDPSetCombineLERP") },
                { 0xFE, new G_SetZImg(0xFE, "gsDPSetDepthImage") },
                { 0xFF, new G_SetCImg(0xFF, "gsDPSetColorImage") },
            });

            return dls.DecodeDL(rom, script, idPrefix, x, lastMat, optimize);
        }

        public static string CheckGeoMacro(uint set)
        {
            Dictionary<uint, string> geoMacros = new()
            {
                { 0x2000, "G_CULL_BACK" },
                { 0x3000, "G_CULL_BOTH" },
                { 0x1000, "G_CULL_FRONT" },
                { 0x10000, "G_FOG" },
                { 0x20000, "G_LIGHTING" },
                { 0x4, "G_SHADE" },
                { 0x200, "G_SHADING_SMOOTH" },
                { 0x40000, "G_TEXTURE_GEN" },
                { 0x80000, "G_TEXTURE_GEN_LINEAR" },
                { 0x1, "G_ZBUFFER" }
            };

            List<string> strs = new();

            foreach (var (key, value) in geoMacros)
            {
                if ((set & key) == key)
                {
                    set ^= key;
                    strs.Add(value);
                }
            }

            return string.Join('|', strs);
        }

        public static List<ModelData> OptimizeModelData(List<ModelData> modelData)
        {
            static bool StartTri(F3DCommand cmd) => cmd is G_Tri1 || cmd is G_Tri2;

            for (int k = 0; k < modelData.Count; ++k)
            {
                ModelData md = modelData[k];
                var ranges = md.Ranges;
                var dl = md.DLs[0];

                int start = ranges[0][0];
                List<List<F3DCommand>> newDl = new() { dl.GetRange(start, (int)ranges[0][2] - start + 1) };

                // End should always start from mat start as its normally not drawing anything
                // But this could bite me, at worst this could end up missing a vert load or 5 tris
                start = ranges[^1][0];
                List<F3DCommand> end = dl.GetRange(start, (int)ranges[^1][2] - start + 1);

                List<List<dynamic>> newRanges = ranges.GetRange(1, ranges.Count - 2).OrderBy(x => (((TexturePtr)x[1]).BankPtr, x[4])).Reverse().ToList();

                for (int j = 0; j < newRanges.Count; ++j)
                {
                    var r = newRanges[j];
                    int x = 1;
                    bool orphan = false;
                    bool mat = false;

                    while (true)
                    {
                        F3DCommand cmd = dl[r[4] + x];
                        if (StartTri(cmd) && mat)
                            orphan = true;
                        else if ((cmd is G_VTX && mat) || (r[4] + x + 1) == dl.Count)
                            break;
                        // gsDP is an RDP cmd or a sync. Either way it signals end of tri draws
                        else if (cmd.ToString().StartsWith("gsDP"))
                            mat = true;
                        ++x;
                    }

                    // Check for duplicate textures
                    bool dupe = false;

                    if (j > 0 && newRanges[j - 1][1] == r[1])
                        dupe = true;

                    // If orphan is true it means there are tris orphaned from a vert load before
                    // the new mat. This means we have to include those orphaned tris.
                    // For dupes I check starting from the texture load (r[0]) and advance until a
                    // tri or vert load
                    if (dupe)
                    {
                        x = r[0];

                        while (true)
                        {
                            if (StartTri(dl[x]) || dl[x] is G_VTX)
                                break;
                            ++x;
                        }

                        if (orphan)
                            newDl[^1].Add(dl[(int)r[4]]);
                        newDl[^1].AddRange(dl.GetRange(x, (int)r[2] - x + 1));
                    }
                    else
                    {
                        if (orphan)
                        {
                            newDl.Add(new List<F3DCommand>() { dl[r[4]] });
                            newDl[^1].AddRange(dl.GetRange((int)r[0], (int)r[2] - (int)r[0] + 1));
                        }
                        else
                            newDl.Add(dl.GetRange((int)r[0], (int)r[2] - (int)r[0] + 1));
                    }
                }

                // Now NewDL is a list of each material. Inside each material the vertex loads
                // and draws are shitty due to moving around textures.
                // I will attempt to organize them to be optimal so no triangle is redrawn
                List<F3DCommand> optNewMats = new();

                foreach (var mat in newDl)
                {
                    Dictionary<F3DCommand, List<F3DCommand>> vertDict = new();
                    F3DCommand? lastLoad = null;

                    // Inside the mat, make a dictionary of every vertex load, and the subsequent triangles
                    foreach (var cmd in mat)
                    {
                        if (cmd is G_VTX)
                        {
                            lastLoad = cmd;
                            if (!vertDict.ContainsKey(cmd))
                                vertDict[cmd] = new List<F3DCommand>();
                        }

                        if (StartTri(cmd))
                        {
                            if (lastLoad is not null && !vertDict[lastLoad].Contains(cmd))
                                vertDict[lastLoad].Add(cmd);
                        }
                    }

                    // Now vertDict is optimized to only have the triangles that matter.
                    // Now remake material with first grabbing all material data, then when
                    // encountering first tri draw fill in via dictionary instead
                    int x = 0;
                    List<F3DCommand> newMat = new();

                    while (true)
                    {
                        if (x >= mat.Count)
                            break;
                        F3DCommand cmd = mat[x];

                        if (StartTri(cmd))
                            break;
                        else if (cmd is G_VTX)
                        {
                            ++x;
                            continue;
                        }
                        else
                            newMat.Add(cmd);
                        ++x;
                    }

                    foreach (var (vertLoad, tris) in vertDict)
                    {
                        if (tris.Count == 0) continue;
                        newMat.Add(vertLoad);
                        newMat.AddRange(tris);
                    }

                    optNewMats.AddRange(newMat);
                }

                optNewMats.AddRange(end);
                modelData[k].DLs = new() { optNewMats };
            }

            return modelData;
        }

        public static List<string> ModelWrite(Rom rom, List<ModelData> modelData, string dir, string idPrefix, string levelDir, bool optimize)
        {
            List<string> refs = new();
            List<(string, string)> excess = new();

            using StreamWriter modelFile = new(File.Open(Path.Join(dir, "custom.model.inc.c"), FileMode.Create));
            modelFile.WriteLine("#include \"custom.model.inc.h\"");

            using StreamWriter texturesFile = new(File.Open(Path.Join(levelDir, "textureNew.inc.c"), FileMode.Append));

            // For editor levels, do not use on actors or RM unless explicitly told flagged
            // depends on all data being in the same display list.
            if (optimize)
                modelData = OptimizeModelData(modelData);

            List<List<int>> trackers = new() { new(), new(), new(), new() };
            List<(uint, uint, uint)> verts = new();
            Func<int, Func<string, int, string>> GetNIDFunc = (_pos) => (id, _q) => id;

            for (int i = 0; i < modelData.Count; ++i)
            {
                ModelData md = modelData[i];
                if (md.Id != null)
                {
                    idPrefix = md.Id;
                    GetNIDFunc = (pos) => (_id, q) => modelData[trackers[pos][q]]?.Id ?? throw new IndexOutOfRangeException();
                }

                verts.AddRange(md.Verts);
            }

            // Vertices = 0
            // Write vertices first so that they're all in a row in ram so vert scrolls work better
            // Have to put all verts in same array, as individual display lists aren't in order
            ModelWrite_Vertices(rom, modelData, idPrefix, modelFile, verts, GetNIDFunc(0), trackers[0], excess, refs);

            List<TexturePtr> texturesSeen = new();
            List<(uint, uint)> diffsSeen = new();
            List<(uint, uint)> ambsSeen = new();
            List<(uint, uint)> startsSeen = new();
            List<Texture> pngs = new();

            for (int modelDataId = 0; modelDataId < modelData.Count; ++modelDataId)
            {
                ModelData md = modelData[modelDataId];

                if (md.Id != null)
                {
                    idPrefix = md.Id;
                    GetNIDFunc = (pos) => (_id, q) => modelData[trackers[pos][q]]?.Id ?? throw new IndexOutOfRangeException();
                }

                // Textures = 1
                foreach (TexturePtr t in md.TexturePtrs)
                {
                    int id = texturesSeen.IndexOf(t);
                    if (id >= 0)
                    {
                        excess.Add(($"{idPrefix}_texture_{t.BankPtr:X8}", $"{GetNIDFunc(1)(idPrefix, id)}_texture_{texturesSeen[id].BankPtr:X8}"));
                        continue;
                    }

                    Texture? tex = ExportTexture(rom, idPrefix, excess, trackers[1], texturesSeen, refs, levelDir, t, modelDataId, texturesFile);
                    if (tex != null)
                        pngs.Add(tex);
                }

                // Lights
                // Redundant symbols in lights in models sometimes
                // This happens with certain importers or if someone wanted to lazily remove shading
                // Diffuse = 2
                foreach ((uint, uint) diff in md.Diffuse)
                {
                    int id = diffsSeen.IndexOf(diff);
                    if (id >= 0)
                    {
                        excess.Add(($"Light_{idPrefix}{Utils.Hex(diff.Item2)}", $"Light_{GetNIDFunc(2)(idPrefix, id)}{Utils.Hex(diffsSeen[id].Item2)}"));
                        continue;
                    }

                    id = ambsSeen.IndexOf(diff);
                    if (id >= 0)
                    {
                        excess.Add(($"Light_{idPrefix}{Utils.Hex(diff.Item2)}", $"Light_{GetNIDFunc(2)(idPrefix, id)}{Utils.Hex(ambsSeen[id].Item2)}"));
                        continue;
                    }

                    diffsSeen.Add(diff);
                    trackers[2].Add(modelDataId);
                    string lig = $"Light_t Light_{idPrefix}{Utils.Hex(diff.Item2)}";
                    refs.Add(lig);
                    byte[] diffBin = rom.GetBytes(diff.Item1, 16);
                    string col1 = string.Join(", ", diffBin[0..3]);
                    string col2 = string.Join(", ", diffBin[4..7]);
                    string dir1 = string.Join(", ", diffBin[8..11]);

                    modelFile.WriteLine($"{lig} = {{");
                    modelFile.WriteLine($"\t{{ {col1}}}, 0, {{ {col2}}}, 0, {{ {dir1}}}, 0");
                    modelFile.WriteLine("};");
                    modelFile.WriteLine();
                }

                // Ambient = 2 (same as Diffuse because they both overlap excess detection)
                foreach ((uint, uint) amb in md.Ambient)
                {
                    int id = ambsSeen.IndexOf(amb);
                    if (id >= 0)
                    {
                        excess.Add(($"Light_{idPrefix}{Utils.Hex(amb.Item2)}", $"Light_{GetNIDFunc(2)(idPrefix, id)}{Utils.Hex(ambsSeen[id].Item2)}"));
                        continue;
                    }

                    id = diffsSeen.IndexOf(amb);
                    if (id >= 0)
                    {
                        excess.Add(($"Light_{idPrefix}{Utils.Hex(amb.Item2)}", $"Light_{GetNIDFunc(2)(idPrefix, id)}{Utils.Hex(diffsSeen[id].Item2)}"));
                        continue;
                    }

                    ambsSeen.Add(amb);
                    trackers[2].Add(modelDataId);
                    string lig = $"Ambient_t Light_{idPrefix}{Utils.Hex(amb.Item2)}";
                    refs.Add(lig);
                    byte[] ambBin = rom.GetBytes(amb.Item1, 8);
                    string col1 = string.Join(", ", ambBin[0..3]);
                    string col2 = string.Join(", ", ambBin[4..7]);

                    modelFile.WriteLine($"{lig} = {{");
                    modelFile.WriteLine($"\t{{{col1}}}, 0, {{{col2}}}, 0");
                    modelFile.WriteLine("};");
                    modelFile.WriteLine();
                }

                // Display Lists = 3
                // Because symbols can exist inside DLs and as DLs themselves
                // I have to create trackers and excess before writing any DL
                // because DLs can be completely non-linear and even recursive
                List<(uint, uint)> twiceStartsSeen = new();

                // This exists because I lose track of which DLs to skip
                foreach (var (s, dl) in md.Start.Zip(md.DLs))
                {
                    int id = startsSeen.IndexOf(s);
                    if (id >= 0)
                    {
                        excess.Add(($"DL_{idPrefix}{Utils.Hex(s.Item2)}", $"DL_{GetNIDFunc(3)(idPrefix, id)}{Utils.Hex(startsSeen[id].Item2)}"));
                        twiceStartsSeen.Add(s);
                        continue;
                    }

                    startsSeen.Add(s);
                    trackers[3].Add(modelDataId);
                }

                foreach (var (s, dl) in md.Start.Zip(md.DLs))
                {
                    if (twiceStartsSeen.Contains(s))
                        continue;

                    string dlName = $"Gfx DL_{idPrefix}{Utils.Hex(s.Item2)}[]";
                    refs.Add(dlName);
                    modelFile.WriteLine($"{dlName} = {{");

                    // Opaque and tex edge RM fog sets, to make sure they're unset which isn't always done
                    // Or it's set again to some dumb shit
                    bool setFogRMA = false;
                    bool setFogRMO = false;

                    for (int n = 0; n < dl.Count; ++n)
                    {
                        F3DCommand cmd = dl[n];

                        if (optimize)
                        {
                            // Remove assets loads that are not referenced (e.g. garbage texture loads)
                            // This may cause empty loads, but that's better than not compiling
                            if (cmd is G_SetTImg cmdSetTImg)
                            {
                                if (!refs.Any(r => r.Contains(cmdSetTImg.Texture)))
                                    continue;
                            }

                            // Just always have combiners repeat first cycle
                            if (cmd is G_SetCombine cmdSetCombine)
                            {
                                for (int j = 0; j < 8; ++j)
                                    cmdSetCombine.Args[8 + j] = cmdSetCombine.Args[j];
                            }

                            if (cmd.Name == "gsDPSetRenderMode" || n == dl.Count - 1)
                            {
                                string line = n == dl.Count - 1 ? ",\n" + cmd.ToString() : string.Empty;

                                if (setFogRMA)
                                {
                                    cmd.Name = "gsDPSetRenderMode";
                                    cmd.Args = new dynamic[] { "G_RM_AA_ZB_TEX_EDGE", "G_RM_NOOP2" };
                                    cmd.Suffix = line;
                                    setFogRMA = false;
                                }
                                else if (setFogRMO)
                                {
                                    cmd.Name = "gsDPSetRenderMode";
                                    cmd.Args = new dynamic[] { "G_RM_AA_ZB_OPA_SURF", "G_RM_NOOP2" };
                                    cmd.Suffix = line;
                                    setFogRMO = false;
                                }
                                else if (cmd.ToString().Contains("G_RM_FOG_SHADE_A, G_RM_AA_ZB_TEX_EDGE2"))
                                    setFogRMA = true;
                                else if (cmd.ToString().Contains("G_RM_FOG_SHADE_A, G_RM_AA_ZB_OPA_SURF2"))
                                    setFogRMO = true;
                            }
                        }

                        // Replace culled data refs with first instance of data
                        foreach (var (rej, orig)  in excess)
                        {
                            for (int a = 0; a < cmd.Args.Length; ++a)
                            {
                                if (cmd.Args[a] is string arg && arg.Contains(rej))
                                    cmd.Args[a] = arg.Replace(rej, orig);
                            }
                        }

                        modelFile.WriteLine($"\t{cmd.ToString()},");
                    }

                    modelFile.WriteLine("};");
                    modelFile.WriteLine();
                }
            }

            Parallel.ForEach(pngs, png => png.Write());

            return refs;
        }

        static void ModelWrite_Vertices(Rom rom, List<ModelData> modelData, string idPrefix, StreamWriter modelFile, List<(uint, uint, uint)> verts, Func<string, int, string> getNID, List<int> tracker, List<(string, string)> excess, List<string> refs)
        {
            List<(uint, uint, uint)> vertsSeen = new();

            verts = verts.OrderBy(v => v.Item1).ToList();

            foreach ((uint, uint, uint) vert in verts)
            {
                int id = vertsSeen.IndexOf(vert);
                if (id >= 0)
                {
                    excess.Add(($"VB_{idPrefix}{Utils.Hex(vert.Item1)}", $"VB_{getNID(idPrefix, id)}{Utils.Hex(vertsSeen[id].Item1)}"));
                    continue;
                }

                vertsSeen.Add(vert);
                tracker.Add(modelData.Count - 1);
                string vbName = $"Vtx VB_{idPrefix}{Utils.Hex(vert.Item1)}[]";
                refs.Add(vbName);
                modelFile.WriteLine($"{vbName} = {{");

                for (int i = 0; i < vert.Item3; ++i)
                {
                    BitStream bin = new(rom.GetBytes(vert.Item2 + i * 16, 16));

                    short[] vpos = new short[3];
                    for (int j = 0; j < vpos.Length; ++j)
                        vpos[j] = bin.ReadInt16();

                    bin.Pad(16);

                    short[] uv = new short[2];
                    for (int j = 0; j < uv.Length; ++j)
                        uv[j] = bin.ReadInt16();

                    byte[] rgba = new byte[4];
                    for (int j = 0; j < rgba.Length; ++j)
                        rgba[j] = bin.ReadByte();

                    string vposStr = string.Join(", ", vpos.Select(s => s.ToString()));
                    string uvStr = string.Join(", ", uv.Select(s => s.ToString()));
                    string rgbaStr = string.Join(", ", rgba.Select(b => b.ToString()));

                    modelFile.WriteLine("\t{{" + $"{{ {vposStr} }}, 0, {{ {uvStr} }}, {{ {rgbaStr}}}" + "}},");
                }

                modelFile.WriteLine("};");
                modelFile.WriteLine();
            }
        }

        static Texture? ExportTexture(Rom rom, string idPrefix, List<(string, string)> excess, List<int> tracker, List<TexturePtr> texturesSeen, List<string> refs, string outputDir, TexturePtr t, int modelDataIndex, StreamWriter texturesFile)
        {
            if (t.RawPtr == 0)
                return null;

            byte[] bin = Array.Empty<byte>();
            try
            {
                bin = rom.GetBytes(t.RawPtr, t.Length * 2 + 2);
            }
            catch
            {
                throw new IndexOutOfRangeException("Texture pointer out of ROM range.");
            }

            tracker.Add(modelDataIndex);
            texturesSeen.Add(t);
            string texName = $"u8 {idPrefix}_texture_{t.BankPtr:X8}[]";
            refs.Add(texName);

            List<string> splitPath = outputDir.Split('\\').ToList();
            int dIndex = splitPath.Contains("actors") ? splitPath.IndexOf("actors") : splitPath.IndexOf("levels");
            string includeDir = string.Join('/', splitPath.GetRange(dIndex, splitPath.Count - dIndex)) + '/';

            // Export a include of a png file
            texturesFile.WriteLine($"ALIGNED8 {texName} = {{");
            texturesFile.WriteLine($"#include \"{includeDir}{idPrefix}{Utils.Hex(t.BankPtr)}_custom.{t.ImgType.ToLower()}{t.BitDepth}.inc.c\"");
            texturesFile.WriteLine("};");

            string png = Path.Join(outputDir, idPrefix + Utils.Hex(t.BankPtr)) + $"_custom.{t.ImgType.ToLower()}{t.BitDepth}";
            (byte[], string)? palette = null;

            if (t.ImgType == "CI")
            {
                string texNamePalette = $"u8 {idPrefix}_texture_{t.Palette[1]:X8}";

                // Palette
                texturesFile.WriteLine($"ALIGNED8 {texNamePalette} = {{");
                texturesFile.WriteLine($"#include \"{includeDir}{idPrefix}{Utils.Hex(t.BankPtr)}_custom.{t.ImgType.ToLower()}{t.BitDepth}.pal.inc.c\"");
                texturesFile.WriteLine("};");

                // Export a PNG
                palette = (rom.GetBytes(t.Palette[0], (uint)Math.Pow(2, t.BitDepth) * 2), "rgba16");
            }
            else
            {
                // I lost the dimensions somehow, so its probably jumping around alot. Assume 32x32
                if (t.Width == 0 || t.Height == 0)
                {
                    t.Width = 32;
                    t.Height = 32;
                }
            }

            return new Texture(t, bin, png, palette);
        }
    }
}
