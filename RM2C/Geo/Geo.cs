using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal abstract class Geo<T>
    {
        public readonly string IdPrefix;
        public readonly List<(List<string>, T)> G;
        public readonly List<(uint, uint)> DLs;

        public Geo(string idPrefix, T beginGeo)
        {
            IdPrefix = idPrefix;
            G = new() { new(new List<string>(), beginGeo) };
            DLs = new();
        }

        protected virtual void AfterCommandExecute(string geoMacro, ref List<dynamic> F) { }
        protected virtual void OnLabelReplace(Script script, string label, ushort arg) { }
        protected abstract T TransformPushArg(uint b);

        protected bool ParseGeneric(Rom rom, Script script, uint start)
        {
            List<uint> starts = new() { start };
            int x = 0;
            int t = 0;

            while (true)
            {
                BitStream q = new(rom.GetBytes(starts.Last() + x, 24));
                byte cmdId = q.ReadByte();
                GeoCommands.GeoCommand action = Data.GeoCmds[cmdId];
                List<dynamic> F = action(q, IdPrefix, script);
                string geoMacro = F[0];

                AfterCommandExecute(geoMacro, ref F);

                if ((F.Last() as string) == "ext")
                {
                    string type = F[2];
                    uint b = (uint)F[3];

                    if (type == "CVASM")
                    {
                        int r = (int)b;
                        string f = Utils.Hexx(r);

                        q.Seek(0, SeekOrigin.Begin);
                        q.Pad(16);
                        ushort arg = q.ReadUInt16();

                        if (r != 0)
                        {
                            string label = RomMap.GetLabel(f);
                            OnLabelReplace(script, label, arg);
                            geoMacro = geoMacro.Replace(r.ToString(), label);
                        }
                    }
                    else if (type == "STOREDL")
                    {
                        if (b != 0)
                            DLs.Add((script.B2P(b), b));
                    }
                    else if (type == "PUSH")
                    {
                        G[t].Item1.Add(geoMacro);
                        b = script.B2P(b);
                        starts[^1] += x + F[1];
                        starts.Add(b);
                        G.Add((new List<string>(), TransformPushArg(b)));
                        t = G.Count - 1;
                        x = 0;
                        continue;
                    }
                    else if (type == "POP")
                    {
                        G[t].Item1.Add(geoMacro);
                        starts.RemoveAt(starts.Count - 1);
                        t = starts.Count - 1;
                        x = 0;
                        continue;
                    }
                }

                x += F[1];
                G[t].Item1.Add(geoMacro);
                if (geoMacro == "GEO_END()") break;
            }

            return true;
        }
    }
}
