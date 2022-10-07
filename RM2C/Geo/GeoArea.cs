using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RM2ExCoop.RM2C
{
    internal class GeoArea : Geo<uint>
    {
        public readonly List<List<List<int>>> WaterBoxes;
        public readonly Area Area;
        public readonly string Cskybox;
        public readonly bool CBG;
        public bool Envfx;

        public GeoArea(string idPrefix, uint beginGeo, Area area, string cskybox, bool cbg, bool envfx = false)
            : base(idPrefix, beginGeo)
        {
            WaterBoxes = new();
            Area = area;
            Cskybox = cskybox;
            CBG = cbg;
            Envfx = envfx;
        }

        public void Write(string filePath)
        {
            using StreamWriter file = new(File.Open(filePath, FileMode.Create));

            foreach (var t in G)
            {
                file.WriteLine("#include \"custom.model.inc.h\"");
                file.WriteLine($"const GeoLayout Geo_{IdPrefix}{Utils.Hex(t.Item2)}[]= {{");
                foreach (var c in t.Item1)
                    file.WriteLine($"{c},");
                file.WriteLine("};");
            }
        }

        public override string ToString()
        {
            StringBuilder str = new();
            string g = string.Join(", ", G.Select(g1 => "[[" + string.Join(", ", g1.Item1.Select(s => $"'{s}'")) + $"], {g1.Item2}]"));
            str.AppendLine($"[{g}]");

            string dls = string.Join(", ", DLs.Select(dl => $"[{dl.Item1}, {dl.Item2}]"));
            str.AppendLine($"[{dls}]");

            string wbs = string.Join(", ", WaterBoxes.Select(wb1 => "[" + string.Join(", ", wb1.Select(wb2 => "[" + string.Join(", ", wb2) + "]")) + "]"));
            str.AppendLine($"[{wbs}]");

            str.AppendLine(Envfx ? "1" : "0");

            return str.ToString();
        }

        protected override void AfterCommandExecute(string geoMacro, ref List<dynamic> F)
        {
            if (geoMacro.Contains("GEO_BACKGROUND") && CBG)
                F[0] = $"GEO_BACKGROUND({Cskybox}+10, geo_skybox_main)";
        }

        protected override void OnLabelReplace(Script script, string label, ushort arg)
        {
            if (label.Contains("geo_movtex_draw_water_regions"))
                WaterBoxes.Add(GetWaterData(Area.Rom, script, arg, Area.Id));
            if (label.Contains("geo_envfx_main") && arg > 0)
                Envfx = true;
        }

        protected override uint TransformPushArg(uint b) => b;

        public static GeoArea? Parse(Area area, Script script, string idPrefix, string cskybox, bool cbg)
        {
            GeoArea geo = new GeoArea(idPrefix, area.Geo, area, cskybox, cbg);
            if (geo.ParseGeneric(area.Rom, script, area.Geo))
                return geo;
            return null;
        }

        static List<List<int>> GetWaterData(Rom rom, Script script, uint arg, uint areaId)
        {
            List<List<int>> waterBoxes = new();

            // For editor water tables are at 0x19001800, but that might not be gauranteed
            uint type = arg & 0xFF; // 0 for water, 1 for toxic mist, 2 for mist, all start with 0x50 for msb

            uint waterTable;
            if (script.Editor)
            {
                try { waterTable = script.B2P(0x19001800 + 0x50 * type); }
                catch { return waterBoxes; }
            }
            else // For RomManager they are at 0x19006000
            {
                try { waterTable = script.B2P(0x19006000 + 0x280 * type + 0x50 * areaId); }
                catch { return waterBoxes; }
            }

            // Because I don't really know how many water boxes there are as thats set by collision or something
            // I'm just going to detect a bad ptr and go off that
            List<uint> ptrs = new();
            int x = 0;

            while (true)
            {
                uint dat = rom.GetUInt32(waterTable + x + 4);

                if (dat == 0) break;

                uint loc;
                try { loc = script.B2P(dat);  }
                catch { break; }

                ptrs.Add(loc);
                x += 8;
            }

            // Now ptrs should be an array of my water data
            foreach (uint p in ptrs)
            {
                List<int> waterBox = new();
                for (uint i = 0; i < 0x20; i += 2)
                    waterBox.Add(rom.GetInt16(p + i));
                waterBoxes.Add(waterBox);
            }

            return waterBoxes;
        }
    }
}
