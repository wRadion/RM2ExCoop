using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    internal class Utils
    {
        public static void CopyDirectory(string source, string target)
        {
            DirectoryInfo sourceDir = new(source);
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);
            foreach (DirectoryInfo dir in sourceDir.EnumerateDirectories())
                CopyDirectory(dir.FullName, Path.Join(target, dir.Name));
            foreach (FileInfo file in sourceDir.EnumerateFiles())
                file.CopyTo(Path.Join(target, file.Name));
        }

        public static string AsciiConvert(byte num)
        {
            if (num < 10)
                return ((char)(num + 0x30)).ToString();
            if (num < 0x24)
                return ((char)(num + 0x37)).ToString();
            if (num < 0x3E)
                return ((char)(num + 0x3D)).ToString();
            return Data.TextMap[num];
        }

        // Unsigned to Signed
        //public static short U2S(ushort u) => (short)u;

        /*public static uint TcH(byte[] bytes) => bytes.Length switch
        {
            1 => bytes[0],
            2 => BitConverter.ToUInt16(bytes.Reverse().ToArray()),
            4 => BitConverter.ToUInt32(bytes.Reverse().ToArray()),
            _ => throw new NotImplementedException()
        };
        public static ushort TcHH(byte[] bytes) => BitConverter.ToUInt16(bytes.Reverse().ToArray());
        public static uint TcHL(byte[] bytes) => BitConverter.ToUInt32(bytes.Reverse().ToArray());*/

        public static (byte, byte, byte[]) ULC(Rom rom, uint start)
        {
            byte cmd = rom.GetByte(start);
            byte len = rom.GetByte(start + 1);

            byte[] args = new byte[len - 2];
            for (int i = 0; i < args.Length; ++i)
                args[i] = rom.GetByte(start + 2 + i);

            return (cmd, len, args);
        }

        public static Command PLC(Rom rom, uint start)
        {
            var (cmd, len, args) = ULC(rom, start);
            start += len;
            if (Data.Jumps.ContainsKey(cmd))
                return new Command(cmd, len, args, start);
            return PLC(rom, start);
        }

        public static byte PosByte(short x) => (byte)((x - 0x2000) / 0x40);

        /*public static uint UPW(byte[] bytes) => BitConverter.ToUInt32(bytes.Reverse().ToArray());
        public static short UPH(byte[] bytes) => BitConverter.ToInt16(bytes.Reverse().ToArray());

        public static int B2I(byte[] x) => BitConverter.ToInt32(x.Reverse().ToArray());
        public static ushort B2S(byte[] x) => BitConverter.ToUInt16(x.Reverse().ToArray());*/

        public static string Hex(int i) => $"0x{i:x}";
        public static string Hex(uint i) => $"0x{i:x}";
        public static string Hex(long i) => $"0x{i:x}";
        public static string Hexx(int i) => $"{i:x}";
        public static string Hexx(uint i) => $"{i:x}";

        public static Dictionary<T, U> MergeDict<T, U>(params Dictionary<T, U>[] dicts) where T : notnull
        {
            Dictionary<T, U> result = new();

            foreach (var dict in dicts)
            {
                foreach (var (k, v) in dict)
                    result[k] = v;
            }

            return result;
        }

        public static T? ClosestKeyInDict<T>(uint num, Dictionary<uint, T> dict)
        {
            uint min = uint.MaxValue;
            T? res = default;

            foreach (var (key, v) in dict)
            {
                uint val = (uint)Math.Abs(key - num);
                if (val < min)
                {
                    min = val;
                    res = v;
                }
            }

            return res;
        }
    }
}
