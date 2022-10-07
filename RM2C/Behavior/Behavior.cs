using System.Collections.Generic;
using System.Linq;
using RM2ExCoop.RM2C.BehaviorCommands;

namespace RM2ExCoop.RM2C
{
    internal class Behavior
    {
        public Script Script;
        public uint Start;
        public string Name;
        public uint NextStart;
        public string? ModelLabel;
        public uint? Col;
        public List<BehaviorFunction> Funcs;

        public Behavior(BehaviorPointer bhvPtr, ScriptObject obj)
        {
            Script = bhvPtr.Script;
            Start = bhvPtr.Start;
            Name = bhvPtr.BhvName;
            NextStart = 0;
            ModelLabel = obj.Models.Count > 0 ? obj.Models[0].Label : null;
            Col = null;
            Funcs = new();
        }

        public List<string> Parse(Rom rom, Stack<BehaviorPointer> bhvStack)
        {
            List<string> statements = new();
            int x = 0;

            while (true)
            {
                BitStream bin = new(rom.GetBytes(Start + x, 0x14));
                byte cmdId = bin.ReadByte();
                BehaviorCommand cmd = BehaviorCommand.Get(cmdId);
                dynamic[] args = cmd.GetArgs(bin);

                string addr;
                switch (cmd.Func)
                {
                    case BehaviorParamType.LIST:
                        args[0] = BehaviorCommand.ObjectList[args[0]];
                        break;

                    case BehaviorParamType.JUMP:
                        NextStart = Script.B2P((uint)args[0] & 0x7FFFFFFF);
                        addr = $"{(uint)args[0]:X8}";
                        string bhv = RomMap.GetLabel(addr);
                        if (bhv.Contains(addr))
                            bhv = $" Bhv_Custom_{bhv}";
                        args[0] = bhv;
                        break;

                    case BehaviorParamType.COL:
                        Col = (uint)args[0];
                        if (ModelLabel is not null)
                            args[0] = $"col_{ModelLabel}_{Utils.Hex(Script.B2P((uint)args[0] & 0x7FFFFFFF))}";
                        else
                            args[0] = $"col_Unk_Collision_{(uint)args[0]}_{Utils.Hex(Script.B2P((uint)args[0] & 0x7FFFFFFF))}";
                        break;

                    case BehaviorParamType.CALL:
                        addr = $"{(uint)args[0]:X8}";
                        string funcName = RomMap.GetLabel(addr);
                        if (funcName.Contains(addr))
                            funcName = $" Func_Custom_{funcName}";
                        Funcs.Add(new BehaviorFunction((uint)args[0], Name, funcName, Script));
                        args[0] = funcName;
                        break;

                    case BehaviorParamType.FIELD:
                        if (BehaviorCommand.Fields.ContainsKey((int)args[0]))
                            args[0] = BehaviorCommand.Fields[(int)args[0]];
                        break;

                    case BehaviorParamType.FIELD3:
                        if (BehaviorCommand.Fields.ContainsKey(args[0]))
                            args[0] = BehaviorCommand.Fields[args[0]];
                        if (BehaviorCommand.Fields.ContainsKey(args[1]))
                            args[1] = BehaviorCommand.Fields[args[1]];
                        if (BehaviorCommand.Fields.ContainsKey(args[2]))
                            args[2] = BehaviorCommand.Fields[args[2]];
                        break;
                }

                string statement = $"{cmd.Name}({string.Join(',', args.Select(a => a.ToString()))})";
                statements.Add(statement);
                x += bin.Position / 8;

                if (cmd is BhvBreak || cmd is BhvBreakUnused || cmd is BhvEndLoop || cmd is BhvEndRepeat || cmd is BhvEndRepeatContinue || cmd is BhvReturn)
                    break;
                if (cmd is BhvGoto bhvGoto)
                {
                    string bhvName = (string)args[0];
                    if (bhvName.Contains("Custom"))
                        bhvStack.Push(new BehaviorPointer(NextStart, Script, bhvName));
                    break;
                }
            }

            return statements;
        }

        public static List<uint> FindHardcodedCols(Rom rom, Behavior bhv, bool editor)
        {
            List<uint> cols = new();

            if (bhv.Name == " bhvPlatformOnTrack" && bhv.Col is null)
            {
                // I despise romhacks
                if (editor)
                    return new() { 0x07003780 };

                foreach (uint val in Data.TrackHardcodedCols.Values)
                    cols.Add(rom.GetUInt32(val));
            }
            else if (bhv.Col is not null)
                cols.Add(bhv.Col.Value);

            return cols;
        }
    }
}
