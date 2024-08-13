using Microsoft.VisualStudio.TestTools.UnitTesting;
using S7Plus.Net.Helpers;
using System;
using System.IO;

namespace S7Plus.Net.Tests.Helpers
{
    [TestClass]
    public class S7VlqValueEncoderTests
    {
        [TestMethod]
        public void EncodeUInt32Vlq_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7VlqValueEncoder.EncodeUInt32Vlq(stream, 129); // 0x81, 0x01
                byte[] expected = new byte[] { 0x81, 0x01 };

                Assert.AreEqual(2, bytesWritten);
                CollectionAssert.AreEqual(expected, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeInt32Vlq_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7VlqValueEncoder.EncodeInt32Vlq(stream, -20); // 0x6C
                byte[] expected = new byte[] { 0x6C };

                Assert.AreEqual(1, bytesWritten);
                CollectionAssert.AreEqual(expected, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeUInt64Vlq_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7VlqValueEncoder.EncodeUInt64Vlq(stream, 129); // 0x81, 0x01
                byte[] expected = new byte[] { 0x81, 0x01 };

                Assert.AreEqual(2, bytesWritten);
                CollectionAssert.AreEqual(expected, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeInt64Vlq_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7VlqValueEncoder.EncodeInt64Vlq(stream, -20); // 0x6C
                byte[] expected = new byte[] { 0x6C };

                Assert.AreEqual(1, bytesWritten);
                CollectionAssert.AreEqual(expected, stream.ToArray());
            }
        }
    }
}
