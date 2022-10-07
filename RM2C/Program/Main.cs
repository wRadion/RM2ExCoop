using System.Collections.Generic;
using System.IO;

namespace RM2ExCoop.RM2C
{
    internal class Main
    {
        public static void Run(string romPath, Options options)
        {
            // Init variables
            Globals.RootDir = Directory.GetCurrentDirectory();
            Globals.OutputDir = Path.Join(Globals.RootDir, "output");

            if (Directory.Exists(Globals.OutputDir))
                Directory.Delete(Globals.OutputDir, true);
            Directory.CreateDirectory(Globals.OutputDir);

            Rom rom = new(romPath);
            RomMap.LoadMap();

            // Export dialogs + courses names
            if (options.Text || options.Levels.Type == LevelsOptionType.ALL)
            {
                Logger.Info("Starting Text");
                Globals.TextDir = Path.Join(Globals.OutputDir, "text", "us");
                Directory.CreateDirectory(Globals.TextDir);

                rom.ExportText();
                Logger.Info("Text done");
            }

            Globals.SrcDir = Path.Join(Globals.OutputDir, "src");
            Globals.GameDir = Path.Join(Globals.SrcDir, "game");
            Directory.CreateDirectory(Globals.GameDir);

            // Export Misc data (trajectories, star positions, ...)
            if (options.Misc || options.Levels.Type == LevelsOptionType.ALL)
            {
                Logger.Info("Starting Misc");
                rom.ExportMisc(options.Editor);
                Logger.Info("Misc done");
            }

            // Clean Sound + Levels dir
            Globals.SoundDir = Path.Join(Globals.OutputDir, "sound");
            if (Directory.Exists(Globals.SoundDir))
                Directory.Delete(Globals.SoundDir, true);
            Directory.CreateDirectory(Globals.SoundDir);

            Globals.M64Dir = Path.Join(Globals.OutputDir, "sound", "sequences", "us");
            Directory.CreateDirectory(Globals.M64Dir);

            Globals.LevelsDir = Path.Join(Globals.OutputDir, "levels");
            if (Directory.Exists(Globals.LevelsDir))
                Directory.Delete(Globals.LevelsDir, true);
            Directory.CreateDirectory(Globals.LevelsDir);

            Globals.OriginalsDir = Path.Join(Globals.RootDir, "originals");

            // Export Levels
            List<Script> scripts = new();
            List<WaterBox> allWaterBoxes = new();
            List<string> m64Files = new();
            List<uint> seqNums = new();

            Logger.Info("Starting Levels");
            using (StreamWriter lvldefs = new(File.Open(Path.Join(Globals.LevelsDir, "custom_level_defines.h"), FileMode.Create)))
            {
                if (options.Levels.Type == LevelsOptionType.ALL)
                {
                    foreach (var pair in Data.Num2Name)
                    {
                        Logger.Info($"Exporting Level {pair.Key}");
                        scripts.Add(rom.ExportLevel(pair.Key, lvldefs, options, allWaterBoxes, m64Files, seqNums));
                        Logger.Info($"{pair.Value} done");
                    }
                }
                else
                {
                    foreach (int id in options.Levels.Levels)
                    {
                        Logger.Info($"Exporting Level {id}");
                        scripts.Add(rom.ExportLevel(id, lvldefs, options, allWaterBoxes, m64Files, seqNums));
                        Logger.Info($"{Data.Num2Name[id]} done");
                    }
                }
            }
            Logger.Info("Levels done");

            // Export Texture Scrolls
            Logger.Info("Starting Textures scrolls");
            Rom.ExportTextureScrolls(scripts);
            Logger.Info("Textures scrolls done");

            // TODO: Export Title Screen (useless for ex-coop?)

            // Process returned scripts to view certain custom data such as custom banks/actors for actor/texture exporting
            Logger.Info("Starting processing scripts");
            var (banks, models, objects) = Rom.ProcessScripts(scripts);
            Logger.Info("Processing scripts done");

            // Export Actors
            if (options.Actors.Type != ActorsOptionType.NONE)
            {
                Logger.Info("Starting Actors");
                Globals.ActorsDir = Path.Join(Globals.OutputDir, "actors");
                if (Directory.Exists(Globals.ActorsDir))
                    Directory.Delete(Globals.ActorsDir, true);
                Directory.CreateDirectory(Globals.ActorsDir);

                rom.ExportActors(options.Actors, models);
                Logger.Info("Actors done");
            }

            // Export Objects
            if (options.Objects.Type != ObjectsOptionType.NONE)
            {
                Logger.Info("Starting Objects");
                Globals.DataDir = Path.Join(Globals.OutputDir, "data");
                Directory.CreateDirectory(Globals.DataDir);

                rom.ExportObjects(options.Objects, objects, options.Editor);
                Logger.Info("Objects done");
            }

            // Export Textures
            if (options.Skyboxes || options.Segment2)
            {
                Globals.TexturesDir = Path.Join(Globals.OutputDir, "textures");
                if (Directory.Exists(Globals.TexturesDir) && !options.Inherit)
                    Directory.Delete(Globals.TexturesDir, true);
                Directory.CreateDirectory(Globals.TexturesDir);
            }

            // -- Export Skyboxes
            if (options.Skyboxes)
            {
                Logger.Info("Starting Skyboxes");
                Globals.SkyboxesDir = Path.Join(Globals.TexturesDir, "skyboxes");
                Directory.CreateDirectory(Globals.SkyboxesDir);

                rom.ExportSkyboxes(options.Editor, banks);
                Logger.Info("Skyboxes done");
            }

            // -- Export Segment2
            if (options.Segment2)
            {
                Logger.Info("Starting Segment2");
                Globals.Segment2Dir = Path.Join(Globals.TexturesDir, "segment2");
                Directory.CreateDirectory(Globals.Segment2Dir);

                rom.ExportSegment2();
                Logger.Info("Segment2 done");
            }

            // Export Water Boxes
            if (!(options.MusicOnly || options.ObjectOnly))
            {
                Logger.Info("Starting WaterBoxes");
                Rom.ExportWaterBoxes(allWaterBoxes);
                Logger.Info("WaterBoxes done");
            }

            // Export Sequences
            if (!(options.WaterOnly || options.ObjectOnly))
            {
                rom.RipNonLevelSeq(m64Files, seqNums, options.MusicExtend);
                rom.CreateSeqJSON(m64Files, seqNums, options.MusicExtend);

                //if (options.Sound)
                //{
                //    Globals.SamplesDir = Path.Join(Globals.SoundDir, "samples");
                //    Directory.CreateDirectory(Globals.SamplesDir);

                //    Globals.InstsDir = Path.Join(Globals.SoundDir, "sound_banks");
                //    Directory.CreateDirectory(Globals.InstsDir);

                //    rom.RipInstBanks();
                //}
            }

            Logger.Info("Export Completed");
        }
    }
}
