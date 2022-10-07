using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    public class F3DDecodeException : Exception { }

    internal abstract class F3DCommand
    {
        // F3D.py : Bin2C(cmd,id)
        public static F3DCommand? FromBytes(byte[] bytes, string idPrefix)
        {
            byte code = bytes[0];
            byte[] argsBin = bytes[1..];

            F3DCommand? command = null;
            try
            {
                command = F3D.GetCommand(code, argsBin, idPrefix);
            }
            catch
            {
                Logger.Error("Unknown F3D Command (id: " + code + ")");
                return null;
            }

            command.Setup();

            return command;
        }

        public readonly byte Code;
        public string Name { get; protected set; }
        public dynamic[] Args { get; set; }
        public string Suffix;

        public F3DCommand(byte code, string name)
        {
            Code = code;
            Name = name;
            Args = Array.Empty<dynamic>();
            Suffix = string.Empty;
        }

        public void SetArgs()
        {
            Args = GetArgs();
        }

        public void DecodeAndSetArgs(BitStream bin, string idPrefix)
        {
            try
            {
                Decode(bin, idPrefix);
            }
            catch (EndOfStreamException)
            {
                throw new F3DDecodeException();
            }
            SetArgs();
        }

        public bool ArgsEquals(F3DCommand other)
        {
            bool equal = true;
            for (int i = 0; i < Args.Length; ++i)
            {
                if (Args[i].GetType() != other.Args[i].GetType() || Args[i] != other.Args[i])
                {
                    equal = false;
                    break;
                }
            }
            return equal;
        }

        protected abstract void Decode(BitStream bin, string idPrefix);
        protected abstract dynamic[] GetArgs();
        public virtual void Setup() { }

        public string ArgsToString() => string.Join(", ", Args.Select(arg => arg.ToString()));
        public new virtual string ToString() => $"{Name}({ArgsToString()}){Suffix}";
    }
}
