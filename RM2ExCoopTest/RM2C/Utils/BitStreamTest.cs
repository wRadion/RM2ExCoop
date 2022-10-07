using RM2ExCoop.RM2C;

namespace RM2ExCoopTest.RM2C
{
    [TestClass]
    public class BitStreamTest
    {
        [TestMethod]
        public void Test_BitStream_Read()
        {
            BitStream bin = new(0b10110010, 0b10001110, 0b10101001, 0b10100111, 0b01101001, 0b10110000, 0b00001000, 0b10110110, 0b00110101, 0b00011111);

            Assert.AreEqual((ushort)2856, bin.ReadUInt16(12));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => bin.ReadByte(10));
            Assert.AreEqual(true, bin.ReadBit());
            Assert.AreEqual((byte)6, bin.ReadByte(3));
            Assert.AreEqual((short)-173, bin.ReadInt16(9));
            Assert.AreEqual((sbyte)78, bin.ReadSByte());
            Assert.AreEqual((uint)865793, bin.ReadUInt32(20));
            Assert.ThrowsException<EndOfStreamException>(() => bin.ReadInt32());
            Assert.AreEqual("000101101100011010100011111", bin.ToString());
            bin.Pad(3);
            Assert.AreEqual((ushort)22, bin.ReadUInt16(5));
            Assert.AreEqual(-117473, bin.ReadInt32(19));
            Assert.ThrowsException<EndOfStreamException>(() => bin.ReadBit());
        }

        [TestMethod]
        public void Test_BitStream_Write()
        {
            BitStream bin = new();

            bin.WriteU(32, 6);
            Assert.AreEqual("100000", bin.ToString());
            bin.WriteS(42, 10);
            Assert.AreEqual("1000000000101010", bin.ToString());
            bin.Write(true);
            Assert.AreEqual("10000000001010101", bin.ToString());
            bin.WriteS(-4, 12);
            Assert.AreEqual("10000000001010101111111111100", bin.ToString());

            Assert.AreEqual((uint)32, bin.ReadUInt32(6));
            Assert.AreEqual(42, bin.ReadInt16(10));

            CollectionAssert.AreEqual(new byte[] { 0x80, 0x2A, 0xFF, 0xE0 }, bin.ToBytes());

            Assert.AreEqual(true, bin.ReadBit());
            Assert.AreEqual(-4, bin.ReadInt16(12));
        }
    }
}
