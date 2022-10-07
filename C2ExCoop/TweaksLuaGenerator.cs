using System.IO;

namespace RM2ExCoop.C2ExCoop
{
    internal class TweaksLuaGenerator
    {
        readonly string _tweaksPath;

        public TweaksLuaGenerator(string tweaksPath)
        {
            _tweaksPath = tweaksPath;
        }

        public void Generate(string outputDir)
        {
            using StreamWriter writer = new(File.Open(Path.Join(outputDir, "tweaks.lua"), FileMode.Create));

            foreach (string line in File.ReadAllLines(_tweaksPath))
            {
                string value = line.Split(' ')[2].Replace("f", "");

                if (line.Contains("COIN_REQ_COINSTAR"))
                    writer.WriteLine($"gLevelValues.coinsRequiredForCoinStar = {value}");
                if (line.Contains("EXIT_COURSE"))
                {
                    string lvlId = line.Split(' ')[2].Replace(",", "");
                    string areaId = line.Split(' ')[3].Replace(",", "");
                    string warpId = line.Split(' ')[4].Replace(",", "");
                    writer.WriteLine($"gLevelValues.exitCastleLevel = {lvlId}");
                    writer.WriteLine($"gLevelValues.exitCastleArea = {areaId}");
                    writer.WriteLine($"gLevelValues.exitCastleWarpNode = {warpId}");
                }
                if (line.Contains("SLIDE_TIME"))
                    writer.WriteLine($"gLevelValues.pssSlideStarTime = {value}");
                if (line.Contains("MC_TIME"))
                    writer.WriteLine($"gLevelValues.metalCapDuration = {value}");
                if (line.Contains("WC_TIME"))
                    writer.WriteLine($"gLevelValues.wingCapDuration = {value}");
                if (line.Contains("VC_TIME"))
                    writer.WriteLine($"gLevelValues.vanishCapDuration = {value}");
                if (line.Contains("MC_LEVEL_TIME"))
                    writer.WriteLine($"gLevelValues.metalCapDurationCotmc = {value}");
                if (line.Contains("WC_LEVEL_TIME"))
                    writer.WriteLine($"gLevelValues.wingCapDurationTotwc = {value}");
                if (line.Contains("VC_LEVEL_TIME"))
                    writer.WriteLine($"gLevelValues.vanishCapDurationVcutm = {value}");
                if (line.Contains("KING_BOMB_FVEL"))
                    writer.WriteLine($"gBehaviorValues.KingBobombFVel = {value}");
                if (line.Contains("KING_BOMB_YAWVEL"))
                    writer.WriteLine($"gBehaviorValues.KingBobombYawVel = {value}");
                if (line.Contains("KING_BOMB_HEALTH"))
                    writer.WriteLine($"gBehaviorValues.KingBobombHealth = {value}");
                if (line.Contains("KING_WHOMP_HEALTH"))
                    writer.WriteLine($"gBehaviorValues.KingWhompHealth = {value}");
                if (line.Contains("KOOPA_SPEED_THI"))
                    writer.WriteLine($"gBehaviorValues.KoopaThiAgility = {value}");
                if (line.Contains("KOOPA_SPEED_BOB"))
                    writer.WriteLine($"gBehaviorValues.KoopaBobAgility = {value}");
                if (line.Contains("MIPS1_STAR_REQ"))
                    writer.WriteLine($"gBehaviorValues.MipsStar1Requirement = {value}");
                if (line.Contains("MIPS2_STAR_REQ"))
                    writer.WriteLine($"gBehaviorValues.MipsStar2Requirement = {value}");
                if (line.Contains("TOAD_STAR_1_REQUIREMENT"))
                    writer.WriteLine($"gBehaviorValues.ToadStar1Requirement = {value}");
                if (line.Contains("TOAD_STAR_2_REQUIREMENT"))
                    writer.WriteLine($"gBehaviorValues.ToadStar2Requirement = {value}");
                if (line.Contains("TOAD_STAR_3_REQUIREMENT"))
                    writer.WriteLine($"gBehaviorValues.ToadStar3Requirement = {value}");
            }
        }
    }
}