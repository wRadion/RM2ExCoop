using RM2ExCoop.RM2C;
using System.Collections.Generic;

namespace RM2ExCoop
{
    internal static class Globals
    {
        public static string ActorsDir = "";
        public static string DataDir = "";
        public static string GameDir = "";
        public static string InstsDir = "";
        public static string LevelsDir = "";
        public static string M64Dir = "";
        public static string OriginalsDir = "";
        public static string OutputDir = "";
        public static string RootDir = "";
        public static string SamplesDir = "";
        public static string Segment2Dir = "";
        public static string SkyboxesDir = "";
        public static string SoundDir = "";
        public static string SrcDir = "";
        public static string TextDir = "";
        public static string TexturesDir = "";

        public static int Cycle = 0;
        public static bool Fog = false;

        public static List<(int, int, TexScroll)> BadScrolls = new();
        public static HashSet<string> AreasWithFog = new();
    }
}
