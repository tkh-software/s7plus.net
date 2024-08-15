using Microsoft.VisualStudio.TestTools.UnitTesting;
using TKH.S7Plus.Net.Helpers;
using System;
using System.IO;

namespace TKH.S7Plus.Net.Tests.Helpers
{
    [TestClass]
    public class S7ValueDecoderTests
    {
        [TestMethod]
        public void DecodeByte_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(new byte[] { 0xAB }))
            {
                byte result = S7ValueDecoder.DecodeByte(stream);
                Assert.AreEqual(0xAB, result);
            }
        }

        [TestMethod]
        public void DecodeUInt16_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(new byte[] { 0x01, 0x02 }))
            {
                UInt16 result = S7ValueDecoder.DecodeUInt16(stream);
                Assert.AreEqual(0x0102, result);
            }
        }

        [TestMethod]
        public void DecodeInt16_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(new byte[] { 0xFF, 0xFE }))
            {
                Int16 result = S7ValueDecoder.DecodeInt16(stream);
                Assert.AreEqual(-2, result);
            }
        }

        [TestMethod]
        public void DecodeUInt32_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03, 0x04 }))
            {
                UInt32 result = S7ValueDecoder.DecodeUInt32(stream);
                Assert.AreEqual<UInt32>(0x01020304, result);
            }
        }

        [TestMethod]
        public void DecodeInt32_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(new byte[] { 0xFF, 0xFF, 0xFF, 0xFE }))
            {
                Int32 result = S7ValueDecoder.DecodeInt32(stream);
                Assert.AreEqual(-2, result);
            }
        }

        [TestMethod]
        public void DecodeUInt64_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }))
            {
                UInt64 result = S7ValueDecoder.DecodeUInt64(stream);
                Assert.AreEqual<UInt64>(0x0102030405060708, result);
            }
        }

        [TestMethod]
        public void DecodeInt64_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE }))
            {
                Int64 result = S7ValueDecoder.DecodeInt64(stream);
                Assert.AreEqual(-2, result);
            }
        }

        [TestMethod]
        public void DecodeUInt32LE_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(new byte[] { 0x04, 0x03, 0x02, 0x01 }))
            {
                UInt32 result = S7ValueDecoder.DecodeUInt32LE(stream);
                Assert.AreEqual<UInt32>(0x01020304, result);
            }
        }

        [TestMethod]
        public void DecodeFloat_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(BitConverter.GetBytes(123.456f)))
            {
                float result = S7ValueDecoder.DecodeFloat(stream);
                Assert.AreEqual(123.456f, result);
            }
        }

        [TestMethod]
        public void DecodeDouble_ValidInput_ReturnsExpectedValue()
        {
            using (var stream = new MemoryStream(BitConverter.GetBytes(123.456)))
            {
                double result = S7ValueDecoder.DecodeDouble(stream);
                Assert.AreEqual(123.456, result);
            }
        }

        [TestMethod]
        public void DecodeOctets_ValidInput_ReturnsExpectedValue()
        {
            byte[] expected = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            using (var stream = new MemoryStream(expected))
            {
                byte[] result = S7ValueDecoder.DecodeOctets(stream, 4);
                CollectionAssert.AreEqual(expected, result);
            }
        }

        [TestMethod]
        public void DecodeString_ValidInput_ReturnsExpectedValue()
        {
            string expected = "Hello";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(expected);
            using (var stream = new MemoryStream(bytes))
            {
                string result = S7ValueDecoder.DecodeString(stream, bytes.Length);
                Assert.AreEqual(expected, result);
            }
        }
    }
}
