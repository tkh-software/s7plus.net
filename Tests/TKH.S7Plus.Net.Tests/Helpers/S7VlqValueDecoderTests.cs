using Microsoft.VisualStudio.TestTools.UnitTesting;
using TKH.S7Plus.Net.Helpers;
using System;
using System.IO;

namespace TKH.S7Plus.Net.Tests.Helpers
{
    [TestClass]
    public class S7VlqValueDecoderTests
    {
        [TestMethod]
        public void DecodeUInt32Vlq_ValidInput_WritesExpectedValue()
        {
            byte[] input = new byte[] { 0x81, 0x01 }; // Encoded VLQ for 129
            using (var stream = new MemoryStream(input))
            {
                UInt32 result = S7VlqValueDecoder.DecodeUInt32Vlq(stream);
                Assert.AreEqual((UInt32)129, result);
            }
        }

        [TestMethod]
        public void DecodeInt32Vlq_ValidInput_WritesExpectedValue()
        {
            byte[] input = new byte[] { 0x6C, 0x00 }; // Encoded VLQ for -20
            using (var stream = new MemoryStream(input))
            {
                Int32 result = S7VlqValueDecoder.DecodeInt32Vlq(stream);
                Assert.AreEqual(-20, result);
            }
        }

        [TestMethod]
        public void DecodeUInt64Vlq_ValidInput_WritesExpectedValue()
        {
            byte[] input = new byte[] { 0x81, 0x01 }; // Encoded VLQ for 129
            using (var stream = new MemoryStream(input))
            {
                UInt64 result = S7VlqValueDecoder.DecodeUInt64Vlq(stream);
                Assert.AreEqual((UInt64)129, result);
            }
        }

        [TestMethod]
        public void DecodeInt64Vlq_ValidInput_WritesExpectedValue()
        {
            byte[] input = new byte[] { 0x6C, 0x00 }; // Encoded VLQ for -20
            using (var stream = new MemoryStream(input))
            {
                Int64 result = S7VlqValueDecoder.DecodeInt64Vlq(stream);
                Assert.AreEqual(-20, result);
            }
        }
    }
}