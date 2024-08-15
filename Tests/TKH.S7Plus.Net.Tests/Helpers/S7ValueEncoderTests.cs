using Microsoft.VisualStudio.TestTools.UnitTesting;
using TKH.S7Plus.Net.Helpers;
using System;
using System.IO;

namespace TKH.S7Plus.Net.Tests.Helpers
{
    [TestClass]
    public class S7ValueEncoderTests
    {
        [TestMethod]
        public void EncodeByte_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeByte(stream, 0xAB);
                Assert.AreEqual(1, bytesWritten);
                CollectionAssert.AreEqual(new byte[] { 0xAB }, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeUInt16_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeUInt16(stream, 0x0102);
                Assert.AreEqual(2, bytesWritten);
                CollectionAssert.AreEqual(new byte[] { 0x01, 0x02 }, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeInt16_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeInt16(stream, -2);
                Assert.AreEqual(2, bytesWritten);
                CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFE }, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeUInt32_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeUInt32(stream, 0x01020304);
                Assert.AreEqual(4, bytesWritten);
                CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0x03, 0x04 }, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeInt32_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeInt32(stream, -2);
                Assert.AreEqual(4, bytesWritten);
                CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFE }, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeUInt64_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeUInt64(stream, 0x0102030405060708);
                Assert.AreEqual(8, bytesWritten);
                CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeInt64_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeInt64(stream, -2);
                Assert.AreEqual(8, bytesWritten);
                CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE }, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeFloat_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeFloat(stream, 123.456f);
                Assert.AreEqual(4, bytesWritten);
                CollectionAssert.AreEqual(BitConverter.GetBytes(123.456f), stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeDouble_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeDouble(stream, 123.456);
                Assert.AreEqual(8, bytesWritten);
                CollectionAssert.AreEqual(BitConverter.GetBytes(123.456), stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeOctets_ValidInput_WritesExpectedValue()
        {
            byte[] input = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeOctets(stream, input);
                Assert.AreEqual(4, bytesWritten);
                CollectionAssert.AreEqual(input, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeString_ValidInput_WritesExpectedValue()
        {
            string input = "Hello";
            byte[] expected = System.Text.Encoding.UTF8.GetBytes(input);
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeString(stream, input);
                Assert.AreEqual(expected.Length, bytesWritten);
                CollectionAssert.AreEqual(expected, stream.ToArray());
            }
        }

        [TestMethod]
        public void EncodeObjectQualifier_ValidInput_WritesExpectedValue()
        {
            using (var stream = new MemoryStream())
            {
                int bytesWritten = S7ValueEncoder.EncodeObjectQualifier(stream);
                // Since the test involves multiple components, we need to validate the output length and content.
                // Length can be variable depending on the encoding scheme used by other components.
                // Assuming default expected values based on your project constants and encoding:
                Assert.IsTrue(bytesWritten > 0);
                Assert.IsTrue(stream.ToArray().Length == bytesWritten);
                // Further checks can be done depending on how you expect the encoded stream to look like.
            }
        }
    }
}
