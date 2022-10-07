using System.Collections.Generic;
using System.IO;

namespace RM2ExCoop.RM2C
{
    internal class Area
    {
        public uint Id;
        public Level Level;

        public uint Geo;
        public Rom Rom;
        public List<Obj> Objects;
        public List<Warp> Warps;
        public uint Col;
        public byte? Music;
        public byte Terrain;
        public List<Macro> Macros;

        public Area(uint id, Level level, uint geo, Rom rom)
        {
            Id = id;
            Level = level;
            Geo = geo;
            Rom = rom;
            Objects = new();
            Warps = new();
            Col = 0;
            Music = null;
            Terrain = 0;
            Macros = new List<Macro>();
        }

        public void Write(StreamWriter file, Script script, string idPrefix)
        {
            // Begin area
            string areaDec = $"const LevelScript local_area_{idPrefix}[]";
            file.WriteLine(areaDec + " = {");
            script.MakeDec(areaDec);

            file.WriteLine($"AREA({Id},Geo_{idPrefix}{Utils.Hex(Geo)}),");
            file.WriteLine($"TERRAIN(col_{idPrefix}{Utils.Hex(Col)}),");
            file.WriteLine($"SET_BACKGROUND_MUSIC(0,{Music}),");
            file.WriteLine($"TERRAIN_TYPE({Terrain}),");
            file.WriteLine($"JUMP_LINK(local_objects_{idPrefix}),");
            file.WriteLine($"JUMP_LINK(local_warps_{idPrefix}),");

            if (Macros.Count > 0)
                file.WriteLine($"MACRO_OBJECTS(local_macro_objects_{idPrefix}),");

            file.WriteLine("END_AREA(),");
            file.WriteLine("RETURN()");
            file.WriteLine("};");

            string objectsDec = $"const LevelScript local_objects_{idPrefix}[]";
            file.WriteLine(objectsDec + " = {");
            script.MakeDec(objectsDec);

            // Write objects
            for (int i = 0; i < Objects.Count; ++i)
            {
                Obj? obj = Objects[i];
                if (script.TexScrolls.Count > 0)
                {
                    if (obj.BhvName.Contains("Scroll_Texture"))
                    {
                        foreach (TexScroll scroll in script.TexScrolls)
                        {
                            if (scroll.Obj == obj && scroll.AreaId == Id)
                            {
                                obj = scroll.FormatScrollObject(script);
                                break;
                            }
                        }
                    }
                }

                if (obj is not null)
                {
                    string comment = string.Empty;
                    if (obj.RX == 255 && obj.BhvName.Contains("Scroll_Texture"))
                        comment = "// ";
                    file.WriteLine($"{comment}OBJECT_WITH_ACTS({obj}),");
                }
            }
            file.WriteLine("RETURN()");
            file.WriteLine("};");

            // Write Warps
            string warpsDec = $"const LevelScript local_warps_{idPrefix}[]";
            file.WriteLine(warpsDec + " = {");
            script.MakeDec(warpsDec);

            foreach (Warp warp in Warps)
            {
                if (warp is WarpPainting paint)
                    file.WriteLine($"PAINTING_WARP_NODE({paint}),");
                else if (warp is WarpConnected con)
                    file.WriteLine($"WARP_NODE({con}),");
                else if (warp is WarpInstant ins)
                    file.WriteLine($"INSTANT_WARP({ins}),");
            }

            // Write Macro Objects if they exist
            if (Macros.Count > 0)
            {
                string macrosDec = $"const MacroObject local_macro_objects_{idPrefix}[]";
                file.WriteLine(macrosDec + " = {");

                foreach (Macro macro in Macros)
                    file.WriteLine($"MACRO_OBJECT_WITH_BEH_PARAM({macro}),");

                file.WriteLine($"MACRO_OBJECT_END(),");
                file.WriteLine("};");
            }

            file.WriteLine("RETURN()");
            file.WriteLine("};");
        }

        public void WriteModel(GeoArea geoObj, Script script, string areaDir, string hName, string levelDir)
        {
            List<ModelData> modelData = Rom.WriteModel(geoObj.IdPrefix, geoObj.DLs, script, true, true);

            if (modelData.Count == 0)
                return;

            List<string> refs = F3D.ModelWrite(Rom, modelData, areaDir, geoObj.IdPrefix, levelDir, script.Editor);

            Rom.WriteModelHeader(refs, areaDir, hName);
        }
    }
}
