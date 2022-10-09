using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RM2ExCoop.RM2C
{
    internal class Rom
    {
        const int _txtAmount = 170;
        readonly string _romPath;
        readonly string _romName;
        readonly byte[] _romBytes;

        public bool DetScrollType => GetUInt32(0x1202400) != 0x27BDFFE8;

        public Rom(string path)
        {
            _romPath = path;
            _romName = path.Split('\\')[^1].Split(".")[0];
            _romBytes = File.ReadAllBytes(path);
        }

        public void ExportText()
        {
            Script script = new(9);
            script.Seg2(this);
            uint diaTbl = script.B2P(0x0200FFC8);

            string GetText(uint start)
            {
                uint index = start;
                StringBuilder str = new();

                while (true)
                {
                    byte num = GetByte(index++);
                    if (num == 0xFF) break;
                    str.Append(Utils.AsciiConvert(num));
                }

                return str.ToString();
            }

            // Dialogs
            using (StreamWriter dialogFile = new(File.Open(Path.Join(Globals.TextDir, "dialogs.h"), FileMode.Create)))
            {
                for (uint dialog = 0; dialog < _txtAmount * 16; dialog += 16)
                {
                    uint start = diaTbl + dialog;

                    int unused = GetInt32(start);
                    byte lines_box = GetByte(start + 4);
                    //byte pad = GetByte(start + 5);
                    ushort x = GetUInt16(start + 6);
                    ushort width = GetUInt16(start + 8);
                    //UInt16 pad2 = GetUInt16(start + 10);
                    uint offset = GetUInt32(start + 12);

                    uint textStartIndex = script.B2P(offset);
                    string str = GetText(textStartIndex);

                    dialogFile.WriteLine($"DEFINE_DIALOG(DIALOG_{dialog / 16:000},{unused},{lines_box},{x},{width}, _(\"{str}\"))\n");
                }
            }

            // Courses + Extra
            using (StreamWriter courseFile = new(File.Open(Path.Join(Globals.TextDir, "courses.h"), FileMode.Create)))
            {
                // Courses
                int levelNames = 0x8140BE;

                for (int course = 0; course < 26; ++course)
                {
                    // Course name
                    uint courseNameStartIndex = script.B2P(GetUInt32(levelNames + course * 4));
                    string courseName = GetText(courseNameStartIndex);

                    // Acts
                    List<string> acts = new();
                    int actTbl = 0x814A82;

                    if (course < 15)
                    {
                        for (int act = 0; act < 6; ++act)
                        {
                            // Act name
                            uint actNameStartIndex = script.B2P(GetUInt32(course * 24 + actTbl + act * 4));
                            acts.Add(GetText(actNameStartIndex));
                        }

                        StringBuilder courseLine = new("COURSE_ACTS(");
                        courseLine.Append(Data.CourseNames[course]);
                        courseLine.Append(", _(\"");
                        courseLine.Append(courseName);
                        courseLine.Append("\")");
                        foreach (string act in acts)
                        {
                            courseLine.Append(",\t_(\"");
                            courseLine.Append(act);
                            courseLine.Append("\")");
                        }
                        courseLine.Append(")\n");

                        courseFile.WriteLine(courseLine.ToString());
                    }
                    else if (course < 25)
                        courseFile.WriteLine($"SECRET_STAR({course}, _(\"{courseName}\"))");
                    else
                        courseFile.WriteLine($"CASTLE_SECRET_STARS(_(\"{courseName}\"))");
                }

                // Extra
                int extra = 0x814A82 + 15 * 6 * 4;

                for (int i = 0; i < 7; ++i)
                {
                    uint extraStartIndex = script.B2P(GetUInt32(extra + i * 4));
                    string extraText = GetText(extraStartIndex);
                    courseFile.WriteLine($"EXTRA_TEXT({i},_(\"{extraText}\"))");
                }
            }
        }

        public void ExportMisc(bool editor)
        {
            ExportInternalName();

            // Trajectories
            using (StreamWriter trajFile = new(File.Open(Path.Join(Globals.GameDir, "Trajectories.inc.c"), FileMode.Create)))
            {
                trajFile.WriteLine("#include <PR/ultratypes.h>");
                trajFile.WriteLine("#include \"level_misc_macros.h\"");
                trajFile.WriteLine("#include \"macros.h\"");
                trajFile.WriteLine("#include \"types.h\"");
                //trajFile.WriteLine();

                string GetTrajectoryString(string trajName, int start, out int length)
                {
                    int index = start;
                    StringBuilder str = new($"const Trajectory {trajName}_path[] = {{\n");

                    while (true)
                    {
                        short[] point = new short[4];
                        for (int i = 0; i < 4; ++i)
                            point[i] = GetInt16(index + i * 2);
                        if (point[0] == -1) break;
                        str.AppendLine($"\tTRAJECTORY_POS({point[0]}, /*pos*/ {point[1]}, {point[2]}, {point[3]}),");
                        index += 8;
                    }
                    str.Append("\tTRAJECTORY_END(),\n};");
                    length = index - start;

                    return str.ToString();
                }

                foreach (var pair in Data.Trajectories)
                {
                    uint dat = GetUInt32(pair.Value);

                    if (pair.Key == "ccm_seg7_trajectory_penguin_race_RM2C" && dat != 0x3C040702) // Loaded via asm, default value
                        dat = 0x80405A00;

                    // Check if dat is in segment or not
                    if (dat >> 24 != 0x80)
                    {
                        trajFile.WriteLine($"//{pair.Key} Has the default vanilla value or an unrecognizable pointer");
                        trajFile.WriteLine();
                        trajFile.WriteLine(Data.DefaultTrajectories[pair.Key]);
                    }
                    else
                    {
                        dat -= 0x7F200000;
                        trajFile.WriteLine(GetTrajectoryString(pair.Key, (int)dat, out _));
                    }
                    //trajFile.WriteLine();
                }

                // Custom Trajectories
                if (!editor)
                {
                    int i = 0;
                    int offset = 0x01205000; // Rom Manager post-editor fixed offset(?)
                    while (GetUInt16(offset) != 0x0101)
                    {
                        trajFile.WriteLine("// Additional custom trajectory found in the rom");
                        trajFile.WriteLine(GetTrajectoryString($"custom{i++}", offset, out int length));
                        trajFile.WriteLine();
                        offset += length;

                        while (GetByte(offset++) == 0xFF) ;
                        --offset;
                    }
                }
            }

            // Star Positions
            using (StreamWriter starPosFile = new(File.Open(Path.Join(Globals.GameDir, "Star_Pos.inc.c"), FileMode.Create)))
            {
                // Special Koopas Star positions
                foreach (var pair in Data.KoopaStarPositions)
                {
                    short[] pos = new short[3];
                    for (int i = 0; i < 3; ++i)
                        pos[i] = GetInt16(pair.Value[i]);
                    starPosFile.WriteLine($"#define {pair.Key}StarPos {{ {pos[0]}, {pos[1]}, {pos[2]} }}");
                }

                foreach (var pair in Data.StarPositions)
                {
                    int start = editor ? pair.Value[1] : pair.Value[0];
                    float[] pos = new float[3];
                    for (int i = 0; i < 3; ++i)
                        pos[i] = GetFloat32(start + i * 4);
                    if (GetUInt32(pair.Value[0]) == 0x01010101)
                        starPosFile.WriteLine(Data.DefaultStarPositions[pair.Key]);
                    else
                        starPosFile.WriteLine($"#define {pair.Key}StarPos {pos[0]:0}.0f, {pos[1]:0}.0f, {pos[2]:0}.0f");
                }
            }

            // Item Box items
            using (StreamWriter itemBoxFile = new(File.Open(Path.Join(Globals.GameDir, "Item_Box.inc.c"), FileMode.Create)))
            {
                int itemBoxOffset = 0x1204000;
                int end = itemBoxOffset + 0x800;

                itemBoxFile.WriteLine("#include <PR/ultratypes.h>");
                itemBoxFile.WriteLine("#include \"behavior_actions.h\"");
                itemBoxFile.WriteLine("#include \"macros.h\"");
                itemBoxFile.WriteLine("#include \"types.h\"");
                itemBoxFile.WriteLine("#include \"behavior_data.h\"");
                //itemBoxFile.WriteLine();
                itemBoxFile.Write("struct Struct802C0DF0 sExclamationBoxContents[] = { ");

                bool f = false;
                while (true)
                {
                    byte[] b = new byte[4];
                    for (int i = 0; i < 4; ++i)
                        b[i] = GetByte(itemBoxOffset + i);

                    if (!f && b[0] == 1)
                    {
                        itemBoxOffset = 0xEBBA0;
                        f = true;
                        continue;
                    }

                    f = true;
                    if (b[0] == 99) break;

                    string bhv = RomMap.GetLabel(GetUInt32(itemBoxOffset + 4).ToString("X8"));
                    itemBoxOffset += 8;
                    itemBoxFile.WriteLine($"{{ {b[0]}, {b[1]}, {b[2]}, {b[3]}, {bhv} }},");

                    if (itemBoxOffset > end) break;
                }

                itemBoxFile.Write("{ 99, 0, 0, 0, NULL } ");
                itemBoxFile.WriteLine("};");
            }

            ExportTweaks();
        }

        public void ExportTweaks()
        {
            using StreamWriter tweaksFile = new(File.Open(Path.Join(Globals.GameDir, "tweaks.inc.c"), FileMode.Create));
            tweaksFile.WriteLine("//This is a series of defines to edit commonly changed parameters in romhacks");
            tweaksFile.WriteLine("//These are commonly referred to as tweaks");
            //tweaksFile.WriteLine();

            foreach (var tweak in Data.Tweaks)
                tweaksFile.WriteLine($"#define {tweak.Name} {tweak.GetStrValue(this)}");

            tweaksFile.WriteLine("//The following are not exported from the rom, but are placed here for user convenience");
            tweaksFile.WriteLine("#define SHOW_STAR_MILESTONES 0");
            tweaksFile.WriteLine("#define TOAD_STAR_1_DIALOG DIALOG_082");
            tweaksFile.WriteLine("#define TOAD_STAR_2_DIALOG DIALOG_076");
            tweaksFile.WriteLine("#define TOAD_STAR_3_DIALOG DIALOG_083");
            tweaksFile.WriteLine("#define TOAD_STAR_1_DIALOG_AFTER DIALOG_154");
            tweaksFile.WriteLine("#define TOAD_STAR_2_DIALOG_AFTER DIALOG_155");
            tweaksFile.WriteLine("#define TOAD_STAR_3_DIALOG_AFTER DIALOG_156");
            tweaksFile.WriteLine("//whether coins are 3d or not. Changes bhv scripts and pause menu reds rendering");
            tweaksFile.WriteLine("#define USE3DCOINS 0");
            tweaksFile.WriteLine("//must be manually set");
            tweaksFile.WriteLine("#define INCLUDE_MOP 0");
            //tweaksFile.WriteLine();
        }

        public Script ExportLevel(int levelId, StreamWriter lvldefs, Options options, List<WaterBox> allWaterBoxes, List<string> m64Files, List<uint> seqNums)
        {
            Script script = new(levelId);
            script.Seg2(this);

            uint? entry = 0x108A10;
            script.Editor = options.Editor;

            int x = 0;

            while (true)
            {
                Command q = Utils.PLC(this, entry.GetValueOrDefault());
                entry = Data.Jumps[q.Cmd](this, q, script);
                ++x;

                if (!entry.HasValue)
                    break;

                if (x > 10000)
                    return script;
            }

            Level level = script.CurrLevel;
            string levelDir = Path.Join(Globals.OutputDir, "levels", level.Name);
            string originalLevelDir = Path.Join(Directory.GetCurrentDirectory(), "originals", level.Name);

            Utils.CopyDirectory(originalLevelDir, levelDir);
            string scriptPath = Path.Join(levelDir, "script.c");

            if (!script.Banks[0x19].IsSet)
                WriteVanillaLevel(scriptPath, script);
            else
            {
                // LEVELName uses a different castle inside name which is dumb
                lvldefs.WriteLine($"DEFINE_LEVEL({level.Name},{level.LevelName})");
                WriteLevel(levelDir, script, options, allWaterBoxes, m64Files, seqNums);
            }

            return script;
        }

        void ExportInternalName()
        {
            using StreamWriter intNameFile = new(File.Open(Path.Join(Globals.SrcDir, "internal_name.s"), FileMode.Create));

            intNameFile.Write(".byte ");
            string[] bytes = new string[20];
            for (int i = 0; i < 20; ++i)
                bytes[i] = "0x" + GetByte(0x20 + i).ToString("X2").ToLowerInvariant();
            intNameFile.Write(string.Join(',', bytes));
        }

        static void WriteVanillaLevel(string scriptPath, Script script)
        {
            Level level = script.CurrLevel;

            List<string> sLines = File.ReadAllLines(scriptPath).ToList();
            int linePos = 0;
            bool macro = false;

            foreach (Area area in level.GetAreas())
            {
                if (area.Id < 0) continue;

                // Advance past includes for first area
                if (area.Id == 1)
                {
                    for (; linePos < sLines.Count; ++linePos)
                    {
                        string line = sLines[linePos];
                        if (line.Contains($"levels/{level.Name}/header.h"))
                        {
                            ++linePos;
                            break;
                        }
                        // Scripts always start with some static data
                        if (line.Contains("static"))
                            break;
                    }
                }

                // Write Macro objects if they exist
                if (area.Macros.Count > 0)
                {
                    if (!macro)
                    {
                        sLines.Insert(linePos++, "#include \"level_misc_macros.h\"\n#include \"macro_preset_names.h\"");
                        macro = true;
                    }
                    sLines.Insert(linePos++, $"static const MacroObject local_macro_objects_{level.Name}_{area.Id}[] = {{");
                    foreach (Macro m in area.Macros)
                        sLines.Insert(linePos++, $"MACRO_OBJECT_WITH_BEH_PARAM({m}),");
                    sLines.Insert(linePos++, "MACRO_OBJECT_END(),\n};");
                }
            }

            foreach (Area area in level.GetAreas())
            {
                for (; linePos < sLines.Count; ++linePos)
                {
                    if (sLines[linePos].Contains(" AREA("))
                    {
                        ++linePos;
                        break;
                    }
                }

                // Remove other objects/warps
                int j = 0;
                for (; j + linePos < sLines.Count; ++j)
                {
                    string line = sLines[linePos + j];

                    if (line.Contains("OBJECT") || line.Contains("WARP_NODE") || line.Contains("JUMP_LINK") || (macro && line.Contains("MACRO_OBJECTS(")))
                    {
                        sLines.RemoveAt(linePos + j--);
                        continue;
                    }
                    else if (line.Contains("END_AREA()"))
                    {
                        ++j;
                        break;
                    }
                }

                // Write Objects and warps for each area
                linePos = Math.Min(sLines.Count - 1, linePos);
                foreach (Obj obj in area.Objects)
                    sLines.Insert(linePos, $"OBJECT_WITH_ACTS({obj}),");
                foreach (Warp warp in area.Warps)
                {
                    if (warp is WarpPainting paint)
                        sLines.Insert(linePos, $"PAINTING_WARP_NODE({paint}),");
                    else if (warp is WarpConnected con)
                        sLines.Insert(linePos, $"WARP_NODE({con}),");
                    else if (warp is WarpInstant ins)
                        sLines.Insert(linePos, $"INSTANT_WARP({ins}),");
                }
                if (area.Macros.Count > 0)
                    sLines.Insert(linePos, $"MACRO_OBJECTS(local_macro_objects_{level.Name}_{area.Id}),");

                linePos += j;
            }

            File.WriteAllLines(scriptPath, sLines);
        }

        void WriteLevel(string levelDir, Script script, Options options, List<WaterBox> allWaterBoxes, List<string> m64Files, List<uint> seqNums)
        {
            Level level = script.CurrLevel;

            string areasDir = Path.Join(levelDir, "areas");
            Directory.CreateDirectory(areasDir);

            bool envfx = false;

            // Create area directory for each area
            foreach (Area area in level.GetAreas())
            {
                string areaDir = Path.Join(areasDir, $"{area.Id}");
                Directory.CreateDirectory(areaDir);

                if (area.Music.HasValue && !(options.ObjectOnly || options.WaterOnly))
                {
                    var (m64File, seqNum) = RipSequence(area.Music.Value, options.MusicExtend);
                    if (!m64Files.Contains(m64File))
                    {
                        m64Files.Add(m64File);
                        seqNums.Add(seqNum);
                    }
                }

                // Get real bank 0x0E location
                script.RME(area);
                string idPrefix = $"{level.Name}_{area.Id}_";

                bool cbg = false;
                string cskybox = "";

                if (script.Banks[10].IsSet && script.Banks[10].IsValid && script.Banks[10].Start > 0x1220000)
                {
                    cbg = true;
                    cskybox = $"SkyboxCustom{script.Banks[10].Start}_skybox_Index";
                }

                if (GeoArea.Parse(area, script, idPrefix, cskybox, cbg) is GeoArea geoObj)
                {
                    // Deal with some areas having it vs others not
                    if (geoObj.Envfx)
                        envfx = true;

                    if (!options.OnlySkip)
                    {
                        // Write Geo
                        geoObj.Write(Path.Join(areaDir, "custom.geo.inc.c"));
                        foreach (var g in geoObj.G)
                            script.MakeDec($"const GeoLayout Geo_{idPrefix}{Utils.Hex(g.Item2)}[]");

                        // Write Model
                        area.WriteModel(geoObj, script, areaDir, $"{level.Name.ToUpper()}_{area.Id}", levelDir);

                        if (geoObj.DLs.Count == 0)
                            Logger.Warn($"{level.Name} has no Display Lists, that is very bad");
                        else
                        {
                            foreach (var dl in geoObj.DLs)
                                script.MakeDec($"Gfx DL_{idPrefix}{Utils.Hex(dl.Item2)}[]");
                        }

                        // Write Collision
                        Collision.Write(Path.Join(areaDir, "custom.collision.inc.c"), script, this, area.Col, idPrefix);
                    }

                    script.MakeDec($"const Collision col_{idPrefix}{Utils.Hex(area.Col)}[]");

                    // Write MoveTex
                    if (!options.ObjectOnly && !options.MusicOnly)
                    {
                        using StreamWriter movTexFile = new(File.Open(Path.Join(areaDir, "movtextNew.inc.c"), FileMode.Create));
                        List<List<string>> wRefs = new();

                        for (int i = 0; i < geoObj.WaterBoxes.Count; ++i)
                        {
                            List<string> wRef = new();

                            for (int j = 0; j < geoObj.WaterBoxes[i].Count; ++j)
                            {
                                // Now a box is an array of all the data
                                List<int> box = geoObj.WaterBoxes[i][j];
                                string boxStr = string.Join(", ", box.Select(d => d.ToString()));
                                string movTexRef = $"{idPrefix}Movtex_{j}_{i}";

                                // Movtex is just a s16 array, it uses macros but they don't matter
                                movTexFile.WriteLine($"static Movtex {movTexRef}[] = {{{boxStr}}};");
                                movTexFile.WriteLine();

                                wRef.Add(movTexRef);
                            }

                            wRefs.Add(wRef);
                        }

                        for (int i = 0; i < wRefs.Count; ++i)
                        {
                            string movTexId = $"{idPrefix}Movtex_{i}";
                            movTexFile.WriteLine($"const struct MovtexQuadCollection {movTexId}[] = {{");

                            for (int j = 0; j < wRefs[i].Count; ++j)
                                movTexFile.WriteLine($"{{{j},{wRefs[i][j]}}},");
                            movTexFile.WriteLine("{-1, NULL},");
                            movTexFile.WriteLine("};");

                            script.MakeDec($"struct MovtexQuadCollection {movTexId}[]");
                            allWaterBoxes.Add(new WaterBox(movTexId, level.Id, area.Id, i));
                        }
                    }
                }

                Logger.Info($"finished area {area.Id} in level {level.Name}");
            }

            // Write Script
            if (!options.WaterOnly && !options.MusicOnly)
                WriteLevelScript(Path.Join(levelDir, "custom.script.c"), level, script, envfx);
            script.MakeDec($"const LevelScript level_{level.Name}_entry[]");

            // Write Header
            if (!options.OnlySkip)
            {
                using (StreamWriter headerFile = new(File.Open(Path.Join(levelDir, "header.h"), FileMode.Create)))
                {
                    string headerGuard = $"{level.Name.ToUpper()}_HEADER_H";

                    headerFile.WriteLine($"#ifndef {headerGuard}");
                    headerFile.WriteLine($"#define {headerGuard}");
                    headerFile.WriteLine($"#include \"types.h\"");
                    headerFile.WriteLine($"#include \"game/moving_texture.h\"");

                    foreach (string header in script.Header)
                        headerFile.WriteLine($"extern {header};");

                    // Now includes externs from stuff in original level
                    string[] lines = File.ReadAllLines(Path.Join(Globals.OriginalsDir, level.Name, "header.h"));
                    foreach (string line in lines.Where(l => l.StartsWith("extern")))
                        headerFile.WriteLine(line);

                    headerFile.Write("#endif");
                }

                // Append to geo.c, maybe the original works good always??
                using (StreamWriter geoFile = new(File.Open(Path.Join(levelDir, "custom.geo.c"), FileMode.Create)))
                {
                    geoFile.WriteLine("#include <ultra64.h>");
                    geoFile.WriteLine("#include \"sm64.h\"");
                    geoFile.WriteLine("#include \"geo_commands.h\"");
                    geoFile.WriteLine("#include \"game/level_geo.h\"");
                    geoFile.WriteLine("#include \"game/geo_misc.h\"");
                    geoFile.WriteLine("#include \"game/camera.h\"");
                    geoFile.WriteLine("#include \"game/moving_texture.h\"");
                    geoFile.WriteLine("#include \"game/screen_transition.h\"");
                    geoFile.WriteLine("#include \"game/paintings.h\"");
                    geoFile.WriteLine("#include \"make_const_nonconst.h\"");
                    geoFile.WriteLine();
                    geoFile.WriteLine($"#include \"levels/{level.Name}/header.h\"");

                    foreach (Area area in level.GetAreas())
                        // Add in some support for level specific objects somehow
                        geoFile.WriteLine($"#include \"levels/{level.Name}/areas/{area.Id}/custom.geo.inc.c\"");
                }

                // Write leveldata.c
                using (StreamWriter dataFile = new(File.Open(Path.Join(levelDir, "custom.leveldata.c"), FileMode.Create)))
                {
                    dataFile.WriteLine("#include <ultra64.h>");
                    dataFile.WriteLine("#include \"sm64.h\"");
                    dataFile.WriteLine("#include \"surface_terrains.h\"");
                    dataFile.WriteLine("#include \"moving_texture_macros.h\"");
                    dataFile.WriteLine("#include \"level_misc_macros.h\"");
                    dataFile.WriteLine("#include \"macro_preset_names.h\"");
                    dataFile.WriteLine("#include \"special_preset_names.h\"");
                    dataFile.WriteLine("#include \"textures.h\"");
                    dataFile.WriteLine("#include \"dialog_ids.h\"");
                    dataFile.WriteLine();
                    dataFile.WriteLine("#include \"make_const_nonconst.h\"");
                    dataFile.WriteLine();
                    dataFile.WriteLine($"#include \"levels/{level.Name}/textureNew.inc.c\"");

                    foreach (Area area in level.GetAreas())
                    {
                        string start = $"#include \"levels/{level.Name}/areas/{area.Id}";
                        dataFile.WriteLine($"{start}/movtextNew.inc.c\"");
                        dataFile.WriteLine($"{start}/custom.model.inc.c\"");
                        dataFile.WriteLine($"{start}/custom.collision.inc.c\"");
                    }
                }
            }
        }

        public List<ModelData> WriteModel(string idPrefix, List<(uint, uint)> dls, Script script, bool optimize, bool checkFog = false)
        {
            List<ModelData> modelData = new();

            for (int x = 0; x < dls.Count; ++x)
            {
                // Check for bad ptr
                uint st = dls[x].Item1;
                uint first = GetUInt32(st);
                byte c = GetByte(st);

                if (first == 0x01010101 || !F3D.IsCommand(c))
                    return new();

                ModelData? dl = null;
                try
                {
                    dl = F3D.DecodeVDL(this, dls[x], script, idPrefix, optimize);
                    dl.Id = idPrefix;
                    if (checkFog && Globals.Fog)
                    {
                        Logger.Warn($"Model {idPrefix} has fog, for editor, fog DLs are heavily edited, potential for gfx errors.");
                        Globals.AreasWithFog.Add(idPrefix);
                    }
                    modelData.Add(dl);
                }
                catch (F3DDecodeException) { }
                if (dl == null) continue;

                script.Verts.AddRange(dl.Verts);
            }

            return modelData;
        }

        public static void WriteModelHeader(List<string> refs, string dir, string hName)
        {
            using StreamWriter modelHFile = new(File.Open(Path.Join(dir, "custom.model.inc.h"), FileMode.Create));
            string headerGuard = hName + "_HEADER_H";
            modelHFile.WriteLine("#ifndef " + headerGuard);
            modelHFile.WriteLine("#define " + headerGuard);
            modelHFile.WriteLine("#include \"types.h\"");

            foreach (string r in refs)
                modelHFile.WriteLine($"extern {r};");

            modelHFile.Write("#endif");
        }

        static void WriteLevelScript(string filePath, Level level, Script script, bool envfx)
        {
            using StreamWriter scriptFile = new(File.Open(filePath, FileMode.Create));

            scriptFile.WriteLine("#include <ultra64.h>");
            scriptFile.WriteLine("#include \"sm64.h\"");
            scriptFile.WriteLine("#include \"behavior_data.h\"");
            scriptFile.WriteLine("#include \"model_ids.h\"");
            scriptFile.WriteLine("#include \"seq_ids.h\"");
            scriptFile.WriteLine("#include \"dialog_ids.h\"");
            scriptFile.WriteLine("#include \"segment_symbols.h\"");
            scriptFile.WriteLine("#include \"level_commands.h\"");
            scriptFile.WriteLine("#include \"game/level_update.h\"");
            scriptFile.WriteLine("#include \"levels/scripts.h\"");
            scriptFile.WriteLine("#include \"actors/common1.h\"");
            scriptFile.WriteLine("#include \"make_const_nonconst.h\"");
            scriptFile.WriteLine();

            foreach (Area area in level.GetAreas())
                scriptFile.WriteLine($"#include \"areas/{area.Id}/custom.model.inc.h\"");
            scriptFile.WriteLine($"#include \"levels/{level.Name}/header.h\"");
            scriptFile.WriteLine($"extern u8 _{level.Name}_segment_ESegmentRomStart[]; ");
            scriptFile.WriteLine($"extern u8 _{level.Name}_segment_ESegmentRomEnd[];");

            // This is the ideal to match hacks, but currently the way the linker is
            // setup, level object data is in the same bank as level mesh so this cannot be done.
            string? loadLevelStr = script.DetLevelSpecificBank();
            bool loadLevel = loadLevelStr is not null && loadLevelStr.Length > 0;

            if (loadLevel && loadLevelStr != level.Name)
                scriptFile.WriteLine($"#include \"levels/{loadLevelStr}/header.h\"");
            scriptFile.WriteLine($"const LevelScript level_{level.Name}_custom_entry[] = {{");
            script.MakeDec($"const LevelScript level_{level.Name}_custom_entry[]");

            // Entry stuff
            scriptFile.WriteLine("INIT_LEVEL(),");

            if (loadLevel)
            {
                scriptFile.WriteLine($"LOAD_MIO0(0x07, _{loadLevelStr}_segment_7SegmentRomStart, _{loadLevelStr}_segment_7SegmentRomEnd),");
                scriptFile.WriteLine($"LOAD_RAW(0x1A, _{loadLevelStr}SegmentRomStart, _{loadLevelStr}SegmentRomEnd),");
            }
            scriptFile.WriteLine($"LOAD_RAW(0x0E, _{level.Name}_segment_ESegmentRomStart, _{level.Name}_segment_ESegmentRomEnd),");

            if (envfx)
                scriptFile.WriteLine("LOAD_MIO0(        /*seg*/ 0x0B, _effect_mio0SegmentRomStart, _effect_mio0SegmentRomEnd),");

            // Add in loaded banks
            dynamic[] banks = script.InsertBankLoads(scriptFile);

            scriptFile.WriteLine("ALLOC_LEVEL_POOL(),");
            scriptFile.WriteLine("MARIO(/*model*/ MODEL_MARIO, /*behParam*/ 0x00000001, /*beh*/ bhvMario),");

            if (loadLevelStr is not null)
            {
                scriptFile.WriteLine(Data.LevelSpecificModels[loadLevelStr]);

                // Load models that the level uses that are outside groups/level
                script.LoadUnspecifiedModels(scriptFile, loadLevelStr);
            }

            // Add in jumps based on banks returned
            foreach (dynamic bank in banks)
            {
                if (bank is Array arr && arr.Length >= 2)
                    scriptFile.WriteLine($"JUMP_LINK({arr.GetValue(3)}),");
            }

            // A bearable amount of cringe
            foreach (Area area in level.GetAreas())
                scriptFile.WriteLine($"JUMP_LINK(local_area_{level.Name}_{area.Id}_),");

            // End script
            scriptFile.WriteLine("FREE_LEVEL_POOL(),");
            scriptFile.WriteLine($"MARIO_POS({script.MStart.Item1},{script.MStart.Item2},{script.MStart.Item3},{script.MStart.Item4},{script.MStart.Item5}),");
            scriptFile.WriteLine("CALL(/*arg*/ 0, /*func*/ lvl_init_or_update),");
            scriptFile.WriteLine("CALL_LOOP(/*arg*/ 1, /*func*/ lvl_init_or_update),");
            scriptFile.WriteLine("CLEAR_LEVEL(),");
            scriptFile.WriteLine("SLEEP_BEFORE_EXIT(/*frames*/ 1),");
            scriptFile.WriteLine("EXIT(),");
            scriptFile.WriteLine("};");

            foreach (Area area in level.GetAreas())
            {
                string idPrefix = $"{level.Name}_{area.Id}_";
                area.Write(scriptFile, script, idPrefix);
            }
        }

        public static void ExportTextureScrolls(List<Script> scripts)
        {
            using StreamWriter scrollTargetsFile = new(File.Open(Path.Join(Globals.GameDir, "ScrollTargets.inc.c"), FileMode.Create));
            List<string> scrollTargets = new();

            scrollTargetsFile.WriteLine("#include <PR/ultratypes.h>");
            scrollTargetsFile.WriteLine("#include \"sm64.h\"");
            scrollTargetsFile.WriteLine("#include \"types.h\"");
            scrollTargetsFile.WriteLine();
            scrollTargetsFile.WriteLine("//Q. Why does this exist instead of just directly referencing VBs?");
            scrollTargetsFile.WriteLine("//A. Because gcc is dumb and will seg fault if you reference a VB by abstracting it through a bparam");
            scrollTargetsFile.WriteLine("//instead of directly refencing it, causing this horrible shit.");

            foreach (Script script in scripts)
            {
                foreach ((string, int) scroll in script.ScrollArray)
                {
                    scrollTargetsFile.WriteLine($"extern Vtx {scroll.Item1}[];");
                    scrollTargets.Add($" &{scroll.Item1}[{scroll.Item2}],");
                }
            }

            scrollTargetsFile.WriteLine("Vtx *ScrollTargets[]={");
            foreach (string scrollTarget in scrollTargets)
                scrollTargetsFile.WriteLine(scrollTarget);
            scrollTargetsFile.Write("};");
        }

        public static (Dictionary<uint, List<AddressRange>>, Dictionary<string, List<ScriptModel>>, Dictionary<string, ScriptObject>) ProcessScripts(List<Script> scripts)
        {
            // Key = bank num, Values = list of start/end locations
            Dictionary<uint, List<AddressRange>> banks = new();

            // Key = group name, Values = [seg num, label, type, rom addr, seg addr, ID, folder, script]
            Dictionary<string, List<ScriptModel>> models = new();

            // Key = bhv, Values = [ram addr, rom addr, models used with, script]
            Dictionary<string, ScriptObject> objects = new();

            foreach (Script script in scripts)
            {
                // Banks
                for (uint i = 0; i < script.Banks.Length; ++i)
                {
                    AddressRange bank = script.Banks[i];
                    if (!bank.IsSet)
                        continue;

                    // Throw out garbage editor fake loads
                    if (!bank.IsValid)
                        continue;

                    if (!banks.ContainsKey(i))
                        banks[i] = new List<AddressRange>();

                    // Check for duplicate which should be the cast often
                    if (!banks[i].Contains(bank))
                        banks[i].Add(bank);
                }

                // Models
                // Refs of vals to Ids for this script alone so I can view with Objects dict
                Dictionary<int, ScriptModel> ids = new();

                for (int i = 0; i < script.Models.Length; ++i)
                {
                    if (script.Models[i] is not Model m)
                        continue;

                    if (ProcessModel(script, i, m) is not var (group, model))
                        continue;

                    if (!models.ContainsKey(group))
                        models[group] = new List<ScriptModel>();

                    if (!models[group].Contains(model))
                        models[group].Add(model);

                    ids[i] = model;
                }

                // Objects
                foreach (var (obj, _, ramAddr) in script.Objects)
                {
                    if (!ids.ContainsKey(obj.ModelId) || obj.ModelId != 0)
                        continue;

                    ScriptModel model = ids[obj.ModelId];

                    if (!objects.ContainsKey(obj.BhvName))
                        objects[obj.BhvName] = new ScriptObject(ramAddr, script.B2P(ramAddr), script);

                    if (!objects[obj.BhvName].Models.Contains(model))
                        objects[obj.BhvName].Models.Add(model);
                }
            }

            return (banks, models, objects);
        }

        static (string, ScriptModel)? ProcessModel(Script script, int id, Model model)
        {
            uint segment = model.Segment >> 24;

            // I'm skipping seg 14 for now, which is menu geo stuff
            if (segment == 0x14)
                return null;

            string folder = string.Empty;
            AddressRange bank = script.Banks[segment];
            string label = string.Empty;
            string group = string.Empty;

            // A custom bank will be one that is loaded well after
            // all other banks are. This is not guaranteed, but nominal bhv
            if (segment != 0)
            {
                if (bank.Start > 0x1220000)
                {
                    if (model.Type == "geo")
                        label = $"custom_geo_{model.Segment:x8}";
                    else
                        label = $"custom_DL_{model.Segment:x8}";
                    group = $"custom_{bank.Start:x}";
                    folder = $"custom_{model.Segment:x8}";
                }
                // These are in Seg C, D, F, 16, 17
                else if (segment != 7 && segment != 0x12 && segment != 0xE)
                {
                    // Catch group0/common0/1 f3d/geo loads. F3d loads happen most often in these
                    if (segment == 8 || segment == 0xF)
                        group = "common0";
                    else if (segment == 3 || segment == 0x16)
                        group = "common1";
                    else if (segment == 4 || segment == 0x17)
                        group = "group0";
                    else
                        group = ((string)Utils.ClosestKeyInDict(bank.Start, Data.Groups)[0])[1..];
                    if (Data.ActorGroups[group].TryGetValue((id, $"0x{model.Segment:x8}"), out var actorGroup))
                    {
                        label = actorGroup[1];
                        folder = actorGroup[2];
                    }
                }
                // These are all in bank 7 with geo layouts in bank 12. Bank 0xE is used for vanilla levels
                else
                {
                    uint md = model.Segment;

                    // If bank 0x19 doesn't exist, it's a vanilla level and segE
                    if (!script.Banks[0x19].IsSet)
                        md += 0x04000000;

                    if (Utils.ClosestKeyInDict(script.Banks[7].Start, Data.LevelSpecificBanks) is string grp)
                    {
                        group = grp;
                        if (Data.LevelGroups[grp].TryGetValue((id, $"0x{md:x8}"), out var modelGroup))
                        {
                            label = modelGroup[1];
                            folder = modelGroup[2];
                        }
                    }
                }
            }

            // Attempt to guess rom address based on generic ram map. Might work for RM, unlikely for editor
            // Something extra added to existing bank. Its a good idea to check for MOP here aswell
            // some part of MOP is inserted into seg3 while others are in 0xF and some just loaded directly to ram
            // like a caveman would.
            if (segment == 0 || label.Length == 0)
            {
                // Check for mop first before giving it null status
                foreach (var (k, v) in Data.MOPModels)
                {
                    if ((id, (int)model.Segment) == k)
                    {
                        group = "MOP";
                        label = v;
                        folder = v;
                        break;
                    }
                }

                if (group.Length == 0)
                {
                    if (segment != 0)
                    {
                        label = "unk";
                        group = "unk";
                        folder = $"unk_{script.CurrLevel.Name}";
                    }
                    else
                    {
                        label = "Null";
                        group = "Null";
                        folder = "Null";
                    }

                    if (model.Type == "geo")
                        label += $"_geo_{model.Segment:x8}";
                    else
                        label += $"_DL_{model.Segment:x8}";
                    folder += $"_{model.Segment:x8}";
                }
            }

            return (group, new ScriptModel(segment, label, model.Type, model.RomAddr, model.Segment, id, folder, model.Script));
        }

        public void ExportActors(ActorsOption option, Dictionary<string, List<ScriptModel>> allModels)
        {
            Actors actors = new(option);
            List<string> levels = Data.Num2Name.Values.ToList();

            Dictionary<string, List<ScriptModel>> models = allModels;

            switch (option.Type)
            {
                case ActorsOptionType.OLD:
                    foreach (string group in models.Keys)
                        models[group] = models[group].Where(m => !m.Label.Contains("custom") && !m.Label.Contains("unk")).ToList();
                    break;

                case ActorsOptionType.GROUP:
                    foreach (string group in models.Keys)
                    {
                        if (group != option.Group)
                            models.Remove(group);
                    }
                    if (models.Count == 0)
                    {
                        Logger.Error($"Actor group '{option.Group}' is not valid.");
                        return;
                    }
                    break;

                case ActorsOptionType.GROUPS:
                    foreach (string group in models.Keys)
                    {
                        if (!option.Groups.Contains(group))
                            models.Remove(group);
                    }
                    if (models.Count == 0)
                    {
                        Logger.Error("Actor groups invalid.");
                        return;
                    }
                    break;
            }

            foreach (var (group, groupModels) in models)
            {
                foreach (ScriptModel model in groupModels)
                {
                    if (model.Label.Length > 0)
                        actors.EvalModel(group, model);
                }
            }

            actors.MakeFolders(this);
        }

        public void ExportObjects(ObjectsOption option, Dictionary<string, ScriptObject> allObjects, bool editor)
        {
            using StreamWriter bhvDataFile = new(File.Open(Path.Join(Globals.DataDir, "custom.behavior_data.inc.h"), FileMode.Create));

            bhvDataFile.WriteLine("//Include this file at the bottom of behavior_data.c");

            List<(uint, ScriptObject, string)> collisions = new();
            List<BehaviorFunction> functions = new();
            Dictionary<string, ScriptObject> objects = allObjects;

            switch (option.Type)
            {
                case ObjectsOptionType.BEHAVIORS:
                    foreach (string key in objects.Keys)
                    {
                        if (!option.Behaviors.Contains(key))
                            objects.Remove(key);
                    }
                    break;

                case ObjectsOptionType.BEHAVIOR:
                    foreach (string key in objects.Keys)
                    {
                        if (key != option.Behavior)
                            objects.Remove(key);
                    }
                    break;
            }

            foreach (var (bhv, obj) in objects)
            {
                var (cols, funcs) = ExportBehavior(obj, bhvDataFile, bhv, editor);
                foreach (var col in cols)
                {
                    foreach (var c in col)
                        collisions.Add((c, obj, bhv));
                }
                if (funcs.Count > 0)
                    functions.AddRange(funcs);
            }

            foreach (var col in collisions)
                ExportActorCol(col, option);

            // TODO: Export ASM Functions
            // if (functions.Count > 0)
                // ExportFunctions(functions);
        }

        void ExportActorCol((uint, ScriptObject, string) tuple, ObjectsOption option)
        {
            var (col, obj, bhv) = tuple;

            string cname = string.Empty;
            string cid = string.Empty;
            string cdir;

            // Sometimes they have no model
            if (obj.Models.Count > 0)
            {
                cname = obj.Models[0].Folder;
                cid = obj.Models[0].Label;
            }

            if (cname.Length == 0 || cid.Length == 0)
            {
                cname = $"Unk_Collision_{col}";
                cid = cname;
            }

            cdir = Path.Join(Globals.ActorsDir, cname);
            Directory.CreateDirectory(cdir);

            if (cid.Contains("custom") || cid.Contains("Unk"))
                Logger.Warn($"Collision {cid} in folder {cname} is unknown or found to be new. Used with Behavior {bhv}.");

            _ = Collision.WriteActor(Path.Join(cdir, "custom.collision.inc.c"), obj.Script, this, col, $"{cid}_");
            // TODO: Check Col

            Logger.Info($"{cname} collision exported");
        }

        (List<List<uint>>, List<BehaviorFunction>) ExportBehavior(ScriptObject obj, StreamWriter bhvDataFile, string bhvName, bool editor)
        {
            // Behaviors are scripts and can jump around. This keeps track of all jumps and gotos;
            Stack<BehaviorPointer> gotos = new();
            gotos.Push(new BehaviorPointer(obj.RomAddr, obj.Script, bhvName));
            List<List<uint>> collisions = new();
            List<BehaviorFunction> funcs = new();

            while (gotos.Count > 0)
            {
                BehaviorPointer bhvPtr = gotos.Pop();
                Behavior bhv = new(bhvPtr, obj);

                // There is absolutely no reason to believe bhvs cannot be stubbed or just destroyed by random data
                // in a romhack and then never touched.
                List<string> statements = bhv.Parse(this, gotos);

                // Do some hardcoded col pointers for things that are abstracted such as platform on tracks
                List<uint> cols = Behavior.FindHardcodedCols(this, bhv, editor);
                if (cols.Count > 0)
                    collisions.Add(cols);
                funcs.AddRange(bhv.Funcs);

                // Compare the output behavior here, and write it to the log
                // TODO: Checksums

                bhvDataFile.WriteLine($"const BehaviorScript{bhv}[] = {{");
                foreach (string st in statements)
                    bhvDataFile.WriteLine($"{st},");
                bhvDataFile.WriteLine("};");
                bhvDataFile.WriteLine();
            }

            return (collisions, funcs);
        }

        public void ExportSkyboxes(bool editor, Dictionary<uint, List<AddressRange>> banks)
        {
            // There are several different banks of textures, all are in bank 0xA or 0xB or 0x2
            // Editor and RM have different bank load locations, this is because editor didn't follow alignment
            // Seg2 func accounts for this by detecting the asm load, other banks will have to use different dicts
            // Skyboxes are first. Each skybox has its own bank. This algo will export each skybox tile, then merge
            // them into one skybox and delete them all. It's pretty slow.
            Dictionary<uint, string> skyboxes = Utils.MergeDict(editor ? Data.SkyboxesEditor : Data.SkyboxesRM, FindCustomSkyboxes(banks));

            foreach (var (start, skybox) in skyboxes)
            {
                List<MagickImage> tiles = new();
                string name = skybox.Split('_')[1];

                if (name == "cloud")
                    name = "cloud_floor";

                for (uint i = 0; i < 0x40; ++i)
                    tiles.Add(ExportSkyTiles(start, name, i));

                byte[] pixels = new byte[248 * 248 * 4];

                for (int i = 0; i < tiles.Count; ++i)
                {
                    var tilePixels = tiles[i].GetPixels();

                    for (int y = 0; y < 31; ++y)
                    {
                        for (int x = 0; x < 31; ++x)
                        {
                            int xOffset = x + (i % 8 * 31);
                            int yOffset = y + (i / 8 * 31);
                            tilePixels[x, y]?.ToArray().CopyTo(pixels, yOffset * 248 * 4 + xOffset * 4);
                        }
                    }
                }

                MagickImage fullSkybox = new();
                PixelReadSettings settings = new(248, 248, StorageType.Char, PixelMapping.RGBA);
                fullSkybox.ReadPixels(pixels.ToArray(), settings);
                fullSkybox.Write(Path.Join(Globals.SkyboxesDir, $"{name}.png"));
                Logger.Info($"skybox {name} done");
            }
        }

        static Dictionary<uint, string> FindCustomSkyboxes(Dictionary<uint, List<AddressRange>> banks)
        {
            Dictionary<uint, string> customSkyboxes = new();

            if (banks.Count == 0)
                return customSkyboxes;

            foreach (AddressRange b in banks[0xA])
            {
                if (b.Start > 0x1220000)
                    customSkyboxes[b.Start] = $"_SkyboxCustom{b.Start}";
            }

            using StreamWriter skyboxLdFile = new(File.Open(Path.Join(Globals.SkyboxesDir, "Skybox_Rules.ld"), FileMode.Create));
            foreach (string val in customSkyboxes.Values)
                skyboxLdFile.WriteLine($"   MIO0_SEG({val[1..]}_skybox, 0x0A000000)");

            return customSkyboxes;
        }

        MagickImage ExportSkyTiles(uint start, string skyboxName, uint tile)
        {
            //string tileFileName = $"{skyboxName}{tile}.png";
            BitStream bin = new(GetBytes(start + tile * 0x800, 0x800));
            MagickImage img = BinPNG.RGBA16(32, 32, bin);
            //img.Write(Path.Join(Globals.SkyboxesDir, tileFileName));

            return img;
        }

        public void ExportSegment2()
        {
            Script script = new(9);
            script.Seg2(this);

            // Segment2 textures have a few sections.

            // HUD glyphs, 16x16, 0x200 length each
            ExportSegment2Section(script, 0, 0, 0x4A00, 0x200, 16, 16, "segment2", ImgFmt.RGBA16);

            // Credits font. It's 8x8 RGBA16, 0x80 length each
            ExportSegment2Section(script, 0x6200 - 0x4A00, 0x4A00, 0x5900, 0x80, 8, 8, "segment2", ImgFmt.RGBA16);

            // Dialog chars. They are 16x8 IA4, 0x40 length each.
            ExportSegment2Section(script, 0, 0x5900, 0x7000, 0x40, 16, 8, "font_graphics", ImgFmt.IA4);

            // Cam glyphs are separate
            ExportSegment2Section(script, 0xB50, 0x7000, 0x7600, 0x200, 16, 16, "segment2", ImgFmt.RGBA16);
            // Cam up/down are 8x8
            ExportSegment2Section(script, 0xB50, 0x7600, 0x7700, 0x80, 8, 8, "segment2", ImgFmt.RGBA16);

            // Shadows. 16x16 IA8, 0x100 length each.
            ExportSegment2Texture(script, 0, 0x120B8, 0x100, 16, 16, "", ImgFmt.IA8, "shadow_quarter_circle");
            ExportSegment2Texture(script, 0, 0x121B8, 0x100, 16, 16, "", ImgFmt.IA8, "shadow_quarter_square");

            // Warp transitions. 32x64 or 64x64 (see Seg2WarpTrans data)
            foreach (Segment2WarpTransition warp in Data.Seg2WarpTrans)
                ExportSegment2Texture(script, 0, warp.Start, warp.Length, warp.Size.Item1, warp.Size.Item2, "", ImgFmt.IA8, $"segment2.{warp.Name}");

            // Water boxes. 32x32 RGBA16, except mist IA16
            for (uint i = 0; i < 5; ++i)
            {
                uint texLoc = 0x14AB8 + i * 0x800;

                if (i == 3)
                    ExportSegment2Texture(script, 0x11C58 - 0x14AB8, texLoc, 0x800, 32, 32, "segment2", ImgFmt.IA16);
                else
                    ExportSegment2Texture(script, 0x11C58 - 0x14AB8, texLoc, 0x800, 32, 32, "segment2", ImgFmt.RGBA16);
            }
        }

        enum ImgFmt { RGBA16, IA4, IA8, IA16 };
        void ExportSegment2Section(Script script, int nameOffset, uint start, uint end, uint step, int imgWidth, int imgHeight, string namePrefix, ImgFmt format, string? nameOverride = null)
        {
            for (uint i = start; i < end; i += step)
            {
                if (Data.Seg2Glyphs.ContainsKey(i))
                    nameOffset += Data.Seg2Glyphs[i];
                ExportSegment2Texture(script, nameOffset, i, step, imgWidth, imgHeight, namePrefix, format, nameOverride);
            }
        }

        void ExportSegment2Texture(Script script, int nameOffset, uint start, uint length, int imgWidth, int imgHeight, string namePrefix, ImgFmt format, string? nameOverride = null)
        {
            BitStream bin = new(GetBytes(script.B2P(0x02000000 + start), length));
            MagickImage img = format switch
            {
                ImgFmt.RGBA16 => BinPNG.RGBA16(imgWidth, imgHeight, bin),
                ImgFmt.IA4 => BinPNG.IA4(imgWidth, imgHeight, bin),
                ImgFmt.IA8 => BinPNG.IA8(imgWidth, imgHeight, bin),
                ImgFmt.IA16 => BinPNG.IA16(imgWidth, imgHeight, bin),
                _ => throw new NotSupportedException($"Format {format} is not supported.")
            };
            string fmtName = format switch
            {
                ImgFmt.RGBA16 => "rgba16",
                ImgFmt.IA16 => "ia16",
                _ => "ia4"
            };
            string fileName = (nameOverride ?? $"{namePrefix}.{start + nameOffset:X5}") + $".{fmtName}.png";
            img.Write(Path.Join(Globals.Segment2Dir, fileName));
        }

        public static void ExportWaterBoxes(List<WaterBox> waterBoxes)
        {
            if (waterBoxes.Count == 0)
            {
                Logger.Info("No WaterBoxes");
                return;
            }
            waterBoxes.Sort();

            using StreamWriter movtexFile = new(File.Open(Path.Join(Globals.GameDir, "moving_texture.inc.c"), FileMode.Create));

            movtexFile.WriteLine("#include <ultra64.h>");
            movtexFile.WriteLine("#include \"sm64.h\"");
            movtexFile.WriteLine("#include \"moving_texture.h\"");
            movtexFile.WriteLine("#include \"area.h\"");
            movtexFile.WriteLine("/*");
            movtexFile.Write("This is an include meant to help with the addition of moving textures for water boxes.");
            movtexFile.Write(" Moving textures are hardcoded in vanilla, but in hacks they're procedural.");
            movtexFile.Write(" Every hack uses 0x5000 +Type (0 for water, 1 for toxic mist, 2 for mist) to locate the tables for their water boxes.");
            movtexFile.WriteLine(" I will replicate this by using a 3 dimensional array of pointers. This wastes a little bit of memory but is way easier to manage.");
            movtexFile.WriteLine("To use this, simply place this file inside your source directory after exporting.");
            movtexFile.WriteLine("*/");

            foreach (WaterBox wb in waterBoxes)
                movtexFile.WriteLine($"extern u8 {wb.Movtex}[];");

            movtexFile.WriteLine();
            movtexFile.WriteLine("static void *RM2C_Water_Box_Array[33][8][3] = {");

            string areaNull = "{" + string.Join("", Enumerable.Range(0, 3).Select(_ => "NULL,")) + "},";
            string levelNull = "{ " + string.Join("", Enumerable.Range(0, 8).Select(_ => areaNull)) + " },\n";
            int lastLevel = 3;
            int lastArea = -1;
            int lastType = 0;
            bool first = true;

            foreach (WaterBox wb in waterBoxes)
            {
                if ((wb.LevelId != lastLevel || wb.AreaId != lastArea) && !first)
                    movtexFile.Write(string.Join("", Enumerable.Range(0, 2 - lastType).Select(_ => "NULL,")) + "},");

                if (wb.LevelId != lastLevel && !first)
                {
                    lastType = 0;
                    movtexFile.WriteLine(string.Join("", Enumerable.Range(0, 7 - lastArea).Select(_ => areaNull)) + " },");
                    lastArea = -1;
                }

                if (wb.LevelId - lastLevel - 1 > 0)
                    movtexFile.Write(string.Join("", Enumerable.Range(0, wb.LevelId - lastLevel - 1).Select(_ => levelNull)));

                if (wb.LevelId != lastLevel || first)
                    movtexFile.Write("{ ");

                if (wb.AreaId - lastArea - 1 > 0)
                    movtexFile.Write(string.Join("", Enumerable.Range(0, (int)wb.AreaId - lastArea - 1).Select(_ => areaNull)));

                if (wb.MovtexType - lastType - 1 > 0)
                    movtexFile.Write(string.Join("", Enumerable.Range(0, wb.MovtexType - lastType - 1).Select(_ => "NULL,")));

                if (wb.MovtexType == 0)
                    movtexFile.Write("{");

                movtexFile.Write($"&{wb.Movtex},");
                lastLevel = wb.LevelId;
                lastArea = (int)wb.AreaId;
                lastType = wb.MovtexType;

                if (first)
                    first = false;
            }

            movtexFile.Write(string.Join("", Enumerable.Range(0, 2 - lastType).Select(_ => "NULL,")) + "},");
            movtexFile.WriteLine(string.Join("", Enumerable.Range(0, 7 - lastArea).Select(_ => areaNull)) + " }");
            movtexFile.WriteLine("};");
            movtexFile.WriteLine();
            movtexFile.WriteLine("void *GetRomhackWaterBox(u32 id){");
            movtexFile.WriteLine("id = id&0xF;");
            movtexFile.WriteLine("return RM2C_Water_Box_Array[gCurrLevelNum-4][gCurrAreaIndex][id];");
            movtexFile.Write("};");
        }

        (string, uint) RipSequence(uint seqNum, uint musicExtend)
        {
            // audio_dma_copy_immediate loads gSeqFileHeader in audio_init at 0x80319768
            // the line of asm is at 0xD4768 which sets the arg to this
            // This is LUI asm cmd
            uint gSeqFileHeader = (GetUInt32(0xD4768) & 0xFFFF) << 16;
            // This is addiu asm cmd
            gSeqFileHeader += GetUInt32(0xD4770) & 0xFFFF;

            // format is tbl,m64s[]
            // tbl format is [len,offset][]
            uint gSeqFileOffset = gSeqFileHeader + seqNum * 8 + 4;
            uint len = GetUInt32(gSeqFileOffset + 4);
            uint offset = GetUInt32(gSeqFileOffset);

            byte[] m64 = GetBytes(gSeqFileHeader + offset, len);
            string m64Name = $"{seqNum + musicExtend:X2}_Seq_{_romName}_custom";
            string m64File = Path.Join(Globals.M64Dir, m64Name + ".m64");

            File.WriteAllBytes(m64File, m64);

            return (m64Name, seqNum + musicExtend);
        }

        public void RipNonLevelSeq(List<string> m64Files, List<uint> seqNums, uint musicExtend)
        {
            uint[] nonLevels = new uint[] { 1, 2, 11, 13, 14, 15, 16, 18, 20, 21, 22, 23, 27, 28, 29, 30, 31, 32, 33 };

            foreach (uint i in nonLevels)
            {
                if (!seqNums.Contains(i))
                {
                    var (m64, seqNum) = RipSequence(i, musicExtend);
                    m64Files.Add(m64);
                    seqNums.Add(seqNum);
                }
            }
        }

        public void CreateSeqJSON(List<string> m64Files, List<uint> seqNums, uint musicExtend)
        {
            List<(string, uint)> m64s = m64Files.Zip(seqNums).OrderBy(t => t.Second).ToList();

            string[] origJson = File.ReadAllLines(Path.Join(Globals.OriginalsDir, "sequences.json"));

            // This is the location of the Bank to Sequence table.
            uint seqMagic = 0x7F0000;

            // Format is u8 len banks (always 1), u8 bank.
            // Maintain the comment/bank 0 data of the original sequences.json

            using StreamWriter seqJsonFile = new(File.Open(Path.Join(Globals.SoundDir, "sequences.json"), FileMode.Create));
            int last = 0;

            for (int i = 0; i < m64s.Count; ++i)
            {
                var (m64, seqNum) = m64s[i];
                short bankAddr = GetInt16(seqMagic + (seqNum - musicExtend) * 2);
                byte bank = GetByte(seqMagic + bankAddr + 1);

                if (bank > 37)
                {
                    Logger.Error("Soundbank error, try exporting with different rom type (e.g. editor). Seq json may not work properly.");
                    break;
                }

                if (musicExtend > 0)
                {
                    seqJsonFile.WriteLine($"\t\"{m64}\": [\"{Data.Soundbanks[bank]}\"],");
                    continue;
                }

                // Fill in missing sequences
                for (int j = last; j < seqNum + 2; ++j)
                {
                    if (j > 36)
                        break;
                    else if (j == 36)
                    {
                        seqJsonFile.WriteLine($"{origJson[j]},");
                        break;
                    }
                    seqJsonFile.WriteLine(origJson[j]);
                }

                seqJsonFile.Write($"\t\"{m64}\": [\"{Data.Soundbanks[bank]}\"]");
                if (i < m64s.Count - 1)
                    seqJsonFile.Write(',');
                seqJsonFile.WriteLine();

                if (seqNum < 0x23)
                    seqJsonFile.WriteLine($"{origJson[seqNum + 2].Split(':')[0]}: null,");

                last = (int)seqNum + 3;
            }

            seqJsonFile.Write("}");
        }

        // Bytes Manipulation
        public byte GetByte(long start) => _romBytes[start];
        public byte[] GetBytes(long start, long len)
        {
            byte[] bytes = new byte[len];
            for (long i = 0; i < len; ++i)
                bytes[i] = _romBytes[start + i];
            return bytes;
        }
        public short GetInt16(long start) => BitConverter.ToInt16(GetBytes(start, sizeof(short)).Reverse().ToArray());
        public ushort GetUInt16(long start) => BitConverter.ToUInt16(GetBytes(start, sizeof(ushort)).Reverse().ToArray());
        int GetInt32(long start) => BitConverter.ToInt32(GetBytes(start, sizeof(int)).Reverse().ToArray());
        public uint GetUInt32(long start) => BitConverter.ToUInt32(GetBytes(start, sizeof(uint)).Reverse().ToArray());
        float GetFloat32(long start) => BitConverter.ToSingle(GetBytes(start, sizeof(float)).Reverse().ToArray());
    }
}
