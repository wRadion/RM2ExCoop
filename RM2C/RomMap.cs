using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RM2ExCoop.RM2C
{
    internal class RomMap
    {
        /* Load ROM Map file */
        static string[] Map = Array.Empty<string>();

        public static void LoadMap()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("sm64.us.map"));
#pragma warning disable CS8604 // Possible null reference argument.
            using StreamReader mapReader = new(assembly.GetManifestResourceStream(resourceName));
#pragma warning restore CS8604 // Possible null reference argument.
            Map = mapReader.ReadToEnd().Split('\n');
        }

        public static string GetLabel(string addr)
        {
            addr = addr.ToLower();

            foreach (string line in Map)
            {
                if (line.Contains(addr))
                    return line[line.LastIndexOf(' ')..];
            }

            return "0x" + addr;
        }
    }
}
