using System.Collections.Generic;
using System.IO;

namespace RM2ExCoop.RM2C
{
    internal class GeoActor : Geo<string>
    {
        public GeoActor(string idPrefix, string beginGeo)
            : base(idPrefix, beginGeo)
        {
        }

        protected override string TransformPushArg(uint b) => $"Geo_{IdPrefix}{Utils.Hex(b)}";

        public static GeoActor? Parse(Rom rom, ScriptModel model)
        {
            GeoActor geo = new($"{model.Label}_", model.Label);
            if (geo.ParseGeneric(rom, model.Script, model.RomAddr))
                return geo;
            return null;
        }

        public static void WriteAll(List<GeoActor> geos, string dir, StreamWriter geoFile)
        {
            // Actor geo layouts reuse DLs under different IDs
            List<string> geoSymbs = new();
            List<string> geoRep = new();

            foreach (GeoActor geo in geos)
            {
                foreach (var g in geo.G)
                {
                    geoFile.WriteLine("#include \"custom.model.inc.h\"");
                    geoFile.WriteLine($"const GeoLayout {g.Item2}[]= {{");

                    foreach (var c in g.Item1)
                    {
                        string line = c;
                        string[] split = c.Split('(')[^1].Split('_');

                        if (split.Length > 1)
                        {
                            string addr = split[^1];
                            if (geoSymbs.Contains(addr))
                                line = geoRep[geoSymbs.IndexOf(addr)];
                            else
                            {
                                geoSymbs.Add(addr);
                                geoRep.Add(c);
                            }
                        }

                        geoFile.WriteLine($"{line},");
                    }

                    geoFile.WriteLine("};");
                }
            }
        }
    }
}
