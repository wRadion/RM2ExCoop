using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RM2ExCoop.C2ExCoop
{
    internal class MovingTexturesLuaGenerator
    {
        readonly string _movingTexturePath;
        readonly string _scrollTargetsPath;

        public MovingTexturesLuaGenerator(string movingTexturePath, string scrollTargetsPath)
        {
            _movingTexturePath = movingTexturePath;
            _scrollTargetsPath = scrollTargetsPath;
        }

        public void Generate(string outputDir)
        {
            using StreamWriter writer = new(File.Open(Path.Join(outputDir, "moving_textures.lua"), FileMode.Create));

            writer.WriteLine("-- Moving Textures (WaterBoxes)");
            writer.WriteLine("--   Every movtext type is set to 1 (normal water texture)");
            writer.WriteLine("--   If you want to configure and setup other water type, change the value here");
            foreach (string line in File.ReadAllLines(_movingTexturePath))
            {
                if (!line.StartsWith("extern u8")) continue;

                string movtex = line.Split("u8 ")[1].Split("[]")[0];
                string lvl = new Regex("_\\d_Movtex_\\d$").Split(movtex)[0];
                int lvlId = RM2C.Data.Num2Name.First(pair => pair.Value == lvl).Key;

                // TODO: Customize the 1 for the different water boxes
                writer.WriteLine($"movtexqc_register('{movtex}', {lvlId}, 1, 0)");
            }

            writer.WriteLine();
            writer.WriteLine("-- Scroll Textures");
            writer.WriteLine("--   Uncomment and replace <id> and <count> with the proper values");
            writer.WriteLine("--   if you want to have scroll textures in your mod.");
            writer.WriteLine("--[[");
            foreach (string line in File.ReadAllLines(_scrollTargetsPath))
            {
                if (!line.Trim().StartsWith("&VB")) continue;

                string vb = line.Split('&')[1].Split('[')[0];
                string offset = line.Split('[')[1].Split(']')[0];

                writer.WriteLine($"add_scroll_targets(<id>, \"{vb}\", {offset}, <count>)");
            }
            writer.WriteLine("--]]");
        }
    }
}
