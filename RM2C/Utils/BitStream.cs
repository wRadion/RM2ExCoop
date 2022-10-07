using System;
using System.Text;
using System.IO;
using System.Linq;

namespace RM2ExCoop.RM2C
{
    public class BitStream
    {
        bool[] _bits;
        int _readPosition = 0;
        int _writePosition = 0;

        public int Position => _readPosition;

        public BitStream(params bool[] bits)
        {
            _bits = bits;
        }

        public BitStream(params byte[] bytes)
        {
            _bits = new bool[bytes.Length * 8];

            for (int i = 0; i < bytes.Length; i++)
            {
                for (int j = 0; j < 8; ++j)
                {
                    _bits[i * 8 + j] = ((bytes[i] << j) & 0b1000_0000) != 0;
                }
            }
        }

        void CheckCount(int count, int max)
        {
            if (_readPosition + count > _bits.Length)
                throw new EndOfStreamException("This operation would get beyond the end of the bit stream.");
            if (count <= 0 || count > max)
                throw new ArgumentOutOfRangeException($"The bit count for this operation should be between 1 and {max} but got {count}.");
        }

        void CheckCountForWrite(int count, int max)
        {
            if (_writePosition + count > _bits.Length)
            {
                bool[] newBits = new bool[_writePosition + count];
                _bits.CopyTo(newBits, 0);
                _bits = newBits;
            }
            if (count <= 0 || count > max)
                throw new ArgumentOutOfRangeException($"The bit count for this operation should be between 1 and {max} but got {count}.");
        }

        uint ReadUInt(int bitCount = 32)
        {
            uint result = 0;

            for (int i = 0; i < bitCount; ++i)
            {
                if (i != 0)
                    result <<= 1;
                result += (uint)(_bits[_readPosition++] ? 1 : 0);
            }

            return result;
        }

        int ReadInt(int bitCount = 32)
        {
            bool negative = ReadBit();
            --bitCount;

            if (!negative)
                return (int)ReadUInt(bitCount);

            int result = 0;

            for (int i = 0; i < bitCount; ++i)
            {
                if (i != 0)
                    result <<= 1;
                result += _bits[_readPosition++] ? 0 : 1;
            }
            result += 1;

            return -result;
        }

        void WriteUInt(uint value, int bitCount = 32)
        {
            for (int i = bitCount - 1; i >= 0; --i)
            {
                uint n = (uint)Math.Pow(2, i);

                if (value >= n)
                {
                    _bits[_writePosition++] = true;
                    value -= n;
                }
                else
                    _bits[_writePosition++] = false;
            }
        }

        void WriteInt(int value, int bitCount = 32)
        {
            bool neg = false;

            if (value < 0)
            {
                _bits[_writePosition++] = true;
                value = -value - 1;
                neg = true;
            }
            else
                _bits[_writePosition++] = false;

            --bitCount;

            for (int i = bitCount - 1; i >= 0; --i)
            {
                int n = (int)Math.Pow(2, i);

                if (value - n >= 0)
                {
                    _bits[_writePosition++] = !neg;
                    value -= n;
                }
                else
                    _bits[_writePosition++] = neg;
            }
        }

        public override string ToString()
        {
            StringBuilder str = new();

            for (int i = _readPosition; i < _bits.Length; i++)
                str.Append(_bits[i] ? '1' : '0');

            return str.ToString();
        }

        public string ToStringHex() => string.Join("", ToBytes().Select(b => $"{b:x2}"));

        public void Seek(int offset, SeekOrigin origin)
        {
            int oldReadPosition = _readPosition;

            _readPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _readPosition + offset,
                SeekOrigin.End => _bits.Length + offset,
                _ => throw new NotImplementedException()
            };

            if (_readPosition >= _bits.Length)
            {
                _readPosition = oldReadPosition;
                throw new EndOfStreamException("The offset seek would be beyond the end of the bit stream.");
            }
        }

        public void Pad(int bitCount)
        {
            CheckCount(bitCount, int.MaxValue);
            _readPosition += bitCount;
        }

        public bool ReadBit()
        {
            CheckCount(1, 1);
            return _bits[_readPosition++];
        }

        public byte ReadByte(int bitCount = 8)
        {
            CheckCount(bitCount, 8);
            return (byte)ReadUInt(bitCount);
        }

        public sbyte ReadSByte(int bitCount = 8)
        {
            CheckCount(bitCount, 8);
            return (sbyte)ReadInt(bitCount);
        }

        public ushort ReadUInt16(int bitCount = 16)
        {
            CheckCount(bitCount, 16);
            return (ushort)ReadUInt(bitCount);
        }

        public short ReadInt16(int bitCount = 16)
        {
            CheckCount(bitCount, 16);
            return (short)ReadInt(bitCount);
        }

        public uint ReadUInt32(int bitCount = 32)
        {
            CheckCount(bitCount, 32);
            return ReadUInt(bitCount);
        }

        public int ReadInt32(int bitCount = 32)
        {
            CheckCount(bitCount, 32);
            return ReadInt(bitCount);
        }

        public void Write(bool value)
        {
            CheckCountForWrite(1, 1);
            _bits[_writePosition++] = value;
        }

        public void WriteU(byte value, int bitCount = 8)
        {
            CheckCountForWrite(bitCount, 8);
            WriteUInt(value, bitCount);
        }

        public void WriteS(sbyte value, int bitCount = 8)
        {
            CheckCountForWrite(bitCount, 8);
            WriteInt(value, bitCount);
        }

        public void WriteU(ushort value, int bitCount = 16)
        {
            CheckCountForWrite(bitCount, 16);
            WriteUInt(value, bitCount);
        }

        public void WriteS(short value, int bitCount = 16)
        {
            CheckCountForWrite(bitCount, 16);
            WriteInt(value, bitCount);
        }

        public void WriteU(uint value, int bitCount = 32)
        {
            CheckCountForWrite(bitCount, 32);
            WriteUInt(value, bitCount);
        }

        public void WriteS(int value, int bitCount = 32)
        {
            CheckCountForWrite(bitCount, 32);
            WriteInt(value, bitCount);
        }

        public byte[] ToBytes()
        {
            int oldReadPosition = _readPosition;
            _readPosition = 0;

            byte[] bytes = new byte[(int)Math.Ceiling(_bits.Length / 8f)];

            for (int i = 0; i < bytes.Length; ++i)
            {
                try
                {
                    bytes[i] = ReadByte();
                }
                catch (EndOfStreamException)
                {
                    int lastByteCount = _bits.Length - _readPosition;
                    byte lastByte = ReadByte(lastByteCount);
                    bytes[i] = (byte)(lastByte << (8 - lastByteCount));
                }
            }

            _readPosition = oldReadPosition;

            return bytes;
        }
    }
}
