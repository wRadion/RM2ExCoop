using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class Script
    {
        public HashSet<(uint, uint, uint)> Asm = new () {
            new(0x80400000, 0x1200000, 0x1220000),
            new(0x80246000, 0x1000, 0x21f4c0)
        };
        public Level CurrLevel { get; private set; }
        public AddressRange[] Banks;
        public Stack<uint?> Stack;
        public uint? Base;
        public uint? Top;
        public byte Aoffset = 0;
        public bool Editor;
        public uint? CurrArea;
        public Level?[] Levels;
        public Model?[] Models;
        public List<(Obj, uint, uint)> Objects;
        public List<TexScroll> TexScrolls;
        public (byte, short, short, short, short) MStart;
        public List<string> Header;
        public List<(uint, uint, uint)> Verts;
        public List<(string, int)> ScrollArray;

#pragma warning disable CS8603 // Possible null reference return.
        public Area Area => CurrArea.HasValue ? CurrLevel.Areas[CurrArea.Value] : null;
#pragma warning restore CS8603 // Possible null reference return.

        public Script(int level)
        {
            CurrLevel = new Level(level);
            Banks = new AddressRange[32];
            for (int i = 0; i < Banks.Length; i++)
                Banks[i] = new AddressRange();
            Stack = new();
            Base = null;
            Top = 0;
            CurrArea = null;
            Levels = new Level?[25];
            Models = new Model?[256];
            Objects = new();
            TexScrolls = new();
            Header = new();
            Verts = new();
            ScrollArray = new();
        }

        public uint B2P(uint B)
        {
            uint bank = B >> 24;
            uint offset = B & 0xFFFFFF;

            if (bank == 0)
            {
                if (offset > 0x400000 && offset < 0x420000)
                    return 0x1200000 + (B & 0xFFFFF);
                else
                {
                    if (offset > 0x5F0000 && offset < 0x620000)
                        return offset + 0x1E0000;
                    return offset;
                }
            }

            AddressRange seg = Banks[bank];

            if (!seg.IsSet)
                throw new Exception("");

            return seg.Start + offset;
        }

        public void Seg2(Rom rom)
        {
            int start = rom.GetUInt16(0x3AC2) << 16;
            start += rom.GetUInt16(0x3ACE);
            int end = rom.GetUInt16(0x3AC6) << 16;
            end += rom.GetUInt16(0x3ACA);
            Banks[2] = new AddressRange((uint)(start + 0x3156), (uint)(end + 0x3156));
        }

        public void RME(Area area)
        {
            if (Editor) return;

            Rom rom = area.Rom;
            uint start = B2P(0x19005F00) + (area.Id * 16);

            uint begin = rom.GetUInt32(start);
            uint end = rom.GetUInt32(start + 4);

            Banks[0x0E] = new AddressRange(begin, end);
        }

        public void MakeDec(string name)
        {
            Header.Add(name);
        }

        public string? DetLevelSpecificBank()
        {
            if (!Banks[7].IsSet)
                return null;

            // RM Custom Bank 7 check
            if (Banks[7].Start > 0x1220000)
                return null;

            return Utils.ClosestKeyInDict(Banks[7].Start, Data.LevelSpecificBanks);
        }

        public dynamic[] InsertBankLoads(StreamWriter file)
        {
            dynamic[] banks = new dynamic[] { Banks[10], Banks[15], Banks[12], Banks[13] };
            Dictionary<uint, string> dict;

            for (int i = 0; i < banks.Length; ++i)
            {
                AddressRange bank = banks[i];
                if (i == 0)
                {
                    dict = Editor ? Data.SkyboxesEditor : Data.SkyboxesRM;

                    if (bank.IsSet && bank.IsValid)
                    {
                        banks[i] = Utils.ClosestKeyInDict(bank.Start, dict);

                        // Custom skybox
                        if (bank.Start > 0x1220000)
                        {
                            string name = $"_SkyboxCustom{bank.Start}_skybox_mio0";
                            file.WriteLine($"LOAD_MIO0(0xA,{name}SegmentRomStart,{name}SegmentRomEnd),");
                        }
                        else
                            file.WriteLine($"LOAD_MIO0(0xA,{banks[i]}SegmentRomStart,{banks[i]}SegmentRomEnd),");
                    }
                }
                else
                {
                    if (bank.IsSet && bank.IsValid)
                    {
                        banks[i] = Utils.ClosestKeyInDict(bank.Start, Data.Groups);

                        file.WriteLine($"LOAD_MIO0({banks[i][1]},{banks[i][0]}_mio0SegmentRomStart,{banks[i][0]}_mio0SegmentRomEnd),");
                        file.WriteLine($"LOAD_RAW({banks[i][2]},{banks[i][0]}_geoSegmentRomStart,{banks[i][0]}_geoSegmentRomEnd),");
                    }
                }
            }

            return banks;
        }

        public void LoadUnspecifiedModels(StreamWriter file, string level)
        {
            for (int i = 0; i < Models.Length; ++i)
            {
                if (Models[i] is not Model model)
                    continue;

                // Bank 0x14 is for menus, I will ignore it
                int segment = (int)(model.Segment >> 24);
                if (segment == 0x14)
                    continue;

                // Model loads need to use groups because seg addresses are repeated so you can get the wrong ones
                // If you just use the map which has no distinction on which bank is loaded
                string addr = $"{model.Segment:x8}";
                string label = string.Empty;

                if (segment == 0x12)
                {
                    if (Data.LevelGroups[level].TryGetValue((i, $"0x{addr}"), out string[]? modelInfo))
                        label = modelInfo[1];
                }
                // Actor groups, unlikely to exist outside existing goup loads
                else if (segment == 0xD || segment == 0xC)
                {
                    dynamic[]? values = Utils.ClosestKeyInDict(Banks[segment].Start, Data.Groups);
                    if (values is not null)
                    {
                        string group = ((string)values[0])[1..];

                        if (Data.ActorGroups[group].TryGetValue((i, $"0x{addr}"), out string[]? modelInfo))
                            label = modelInfo[1];
                    }
                }
                // Generally MOP
                else if (segment == 0 || segment == 3 || segment == 0xF)
                {
                    foreach (var (k, v) in Data.MOPModels)
                    {
                        if (k == (i, model.Segment))
                        {
                            // Mops are loaded in entry script
                            label = "MOP";
                            break;
                        }
                    }
                }

                // group0, common0, common1 banks that have unique geo layouts (or label was not set)
                if (label.Length == 0)
                    label = RomMap.GetLabel(addr);

                if (label == "MOP")
                    continue;

                string comment = string.Empty;
                if (label.Length == 0 || label.Contains("0x"))
                    comment = "// ";

                bool isLevelSpecific = Data.LevelSpecificModels.ContainsKey(level) && segment == 0x12;

                if ((isLevelSpecific && !Data.LevelSpecificModels[level].Contains(label)) ||
                    (!isLevelSpecific && !Data.Group_Models.Any(s => s.Contains(label))))
                {
                    if (model.Type == "geo")
                        file.WriteLine($"{comment}LOAD_MODEL_FROM_GEO({i},{label}),");
                    else
                        // It's just a guess but I think 4 will lead to the least issues
                        file.WriteLine($"{comment}LOAD_MODEL_FROM_DL({i},{label},4),");
                }
            }
        }
    }
}
