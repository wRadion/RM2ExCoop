using ImageMagick;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class Actors
    {
        // Key is folder, value is model+group
        public Dictionary<string, List<(ScriptModel, string)>> Folders;
        public ActorsOption Option;

        public Actors(ActorsOption option)
        {
            Folders = new();
            Option = option;
        }

        public void EvalModel(string group, ScriptModel model)
        {
            if (!Folders.ContainsKey(model.Folder))
                Folders[model.Folder] = new List<(ScriptModel, string)>();

            if (!Folders[model.Folder].Any(tuple => tuple.Item1.RomAddr == model.RomAddr))
                Folders[model.Folder].Add((model, group));
        }

        public void MakeFolders(Rom rom)
        {
            ScriptModel? model = null;

            foreach (var (folder, models) in Folders)
            {
                string dir = Path.Join(Globals.ActorsDir, folder);
                Directory.CreateDirectory(dir);

                ParseModels(models, folder, rom, dir);
                model = models[0].Item1;
            }

            if (model is not null)
                ExportPowerMeter(rom, model.Script);
        }

        public static void ExportPowerMeter(Rom rom, Script script)
        {
            string powerMeterDir = Path.Join(Globals.ActorsDir, "power_meter");
            Directory.CreateDirectory(powerMeterDir);

            BitStream bin;
            MagickImage img;

            Dictionary<int, string> nums = new()
            {
                { 0, "full" },
                { 1, "seven_segments" },
                { 2, "six_segments" },
                { 3, "five_segments" },
                { 4, "four_segments" },
                { 5, "three_segments" },
                { 6, "two_segments" },
                { 7, "one_segment" }
            };

            // Left side
            bin = new(rom.GetBytes(script.B2P(0x03000000 + 0x233E0), 0x1000));
            img = BinPNG.RGBA16(32, 64, bin);
            img.Write(Path.Join(powerMeterDir, $"power_meter_left_side.rgba16.png"));

            // Right side
            bin = new(rom.GetBytes(script.B2P(0x03000000 + 0x243E0), 0x1000));
            img = BinPNG.RGBA16(32, 64, bin);
            img.Write(Path.Join(powerMeterDir, $"power_meter_right_side.rgba16.png"));

            // Segments
            foreach (var (i, num) in nums)
            {
                bin = new(rom.GetBytes(script.B2P((uint)(0x03000000 + 0x253E0 + i * 0x800)), 0x800));
                img = BinPNG.RGBA16(32, 32, bin);
                img.Write(Path.Join(powerMeterDir, $"power_meter_{num}.rgba16.png"));
            }
        }

        public static void ParseModels(List<(ScriptModel, string)> models, string folder, Rom rom, string dir)
        {
            List<GeoActor> geos = new();
            List<List<(uint, uint)>> dls = new();
            List<string> ids = new();

            foreach (var (model, _) in models)
            {
                // Edit model to have ROM address. MOP seg 0 is mapped with 0x5F0000 = 0x7D0000
                if (model.Type == "geo")
                {
                    if (GeoActor.Parse(rom, model) is GeoActor geo)
                    {
                        geos.Add(geo);
                        dls.Add(geo.DLs);
                        ids.Add(geo.IdPrefix);
                    }
                }
                // Load via F3d
                else
                {
                    dls.Add(new List<(uint, uint)>() { (model.RomAddr, model.SegAddr) });
                    ids.Add($"{model.Label}_");
                }
            }

            using StreamWriter geoFile = new(File.Open(Path.Join(dir, "custom.geo.inc.c"), FileMode.Append));
            if (geos.Count > 0)
                GeoActor.WriteAll(geos, dir, geoFile);

            WriteModels(rom, dls, models[0].Item1.Script, folder, ids, dir, models[0].Item2);
            Logger.Info($"{folder} exported");
        }

        public static void WriteModels(Rom rom, List<List<(uint, uint)>> dls, Script script, string folder, List<string> ids, string dir, string group)
        {
            if (dls.Count == 0 || ids.Count == 0)
                return;

            List<ModelData> modelData = new();

            for (int i = 0; i < dls.Count; ++i)
                modelData.AddRange(rom.WriteModel(ids[i], dls[i], script, false));

            string outputDir = dir;
            if (group != "castle_inside" && Data.Num2Name.ContainsValue(group))
            {
                outputDir = Path.Join(Globals.LevelsDir, group);
                Directory.CreateDirectory(outputDir);
            }
            List<string> refs = F3D.ModelWrite(rom, modelData, dir, ids[0], outputDir, false);

            // TODO: Checksums

            string hName = folder.Split('/')[0] + '_' + folder.Split('/')[^1] + "_model";
            Rom.WriteModelHeader(refs, dir, hName);
        }
    }
}
