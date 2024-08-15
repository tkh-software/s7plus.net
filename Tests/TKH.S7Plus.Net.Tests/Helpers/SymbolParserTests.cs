using Microsoft.VisualStudio.TestTools.UnitTesting;
using TKH.S7Plus.Net.Helpers;
using System;

namespace TKH.S7Plus.Net.Tests.Helpers
{
    [TestClass]
    public class SymbolParserTests
    {
        [TestMethod]
        public void GetRootLevel_SimpleSymbol_ReturnsExpectedValue()
        {
            string symbol = "RootLevel";
            string result = SymbolParser.GetRootLevel(symbol);
            Assert.AreEqual("RootLevel", result);
        }

        [TestMethod]
        public void GetRootLevel_QuotedSymbol_ReturnsExpectedValue()
        {
            string symbol = "\"Root.Level\".SubLevel";
            string result = SymbolParser.GetRootLevel(symbol);
            Assert.AreEqual("Root.Level", result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Symbol syntax error")]
        public void GetRootLevel_UnterminatedQuote_ThrowsException()
        {
            string symbol = "\"RootLevel.SubLevel";
            SymbolParser.GetRootLevel(symbol);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Symbol syntax error")]
        public void GetRootLevel_EmptySymbol_ThrowsException()
        {
            string symbol = ".SubLevel";
            SymbolParser.GetRootLevel(symbol);
        }

        [TestMethod]
        public void GetRootLevel_NestedSymbol_ReturnsExpectedValue()
        {
            string symbol = "RootLevel.SubLevel";
            string result = SymbolParser.GetRootLevel(symbol);
            Assert.AreEqual("RootLevel", result);
        }

        [TestMethod]
        public void GetRootLevel_SymbolWithoutSubLevel_ReturnsWholeSymbol()
        {
            string symbol = "RootLevel";
            string result = SymbolParser.GetRootLevel(symbol);
            Assert.AreEqual("RootLevel", result);
        }

        [TestMethod]
        public void GetNextLevel_SimpleSymbol_ReturnsEmpty()
        {
            string symbol = "RootLevel";
            string result = SymbolParser.GetNextLevel(symbol);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetNextLevel_NestedSymbol_ReturnsNextLevel()
        {
            string symbol = "RootLevel.SubLevel";
            string result = SymbolParser.GetNextLevel(symbol);
            Assert.AreEqual("SubLevel", result);
        }

        [TestMethod]
        public void GetNextLevel_QuotedSymbol_ReturnsNextLevel()
        {
            string symbol = "\"Root.Level\".SubLevel";
            string result = SymbolParser.GetNextLevel(symbol);
            Assert.AreEqual(".SubLevel", result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Symbol syntax error")]
        public void GetNextLevel_UnterminatedQuote_ThrowsException()
        {
            string symbol = "\"RootLevel.SubLevel";
            SymbolParser.GetNextLevel(symbol);
        }

        [TestMethod]
        public void GetNextLevel_SymbolWithTrailingDot_ReturnsEmpty()
        {
            string symbol = "RootLevel.";
            string result = SymbolParser.GetNextLevel(symbol);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetNextLevel_SymbolWithOnlyRootLevel_ReturnsEmpty()
        {
            string symbol = "RootLevel";
            string result = SymbolParser.GetNextLevel(symbol);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetNextLevel_MultipleLevels_ReturnsNextLevel()
        {
            string symbol = "RootLevel.SubLevel.SubSubLevel";
            string result = SymbolParser.GetNextLevel(symbol);
            Assert.AreEqual("SubLevel.SubSubLevel", result);
        }

        [TestMethod]
        public void GetRootLevel_QuotedSymbolWithDotAtEnd_ReturnsExpectedValue()
        {
            string symbol = "\"RootLevel.\"";
            string result = SymbolParser.GetRootLevel(symbol);
            Assert.AreEqual("RootLevel.", result);
        }

        [TestMethod]
        public void GetNextLevel_QuotedSymbolWithDotAtEnd_ReturnsEmpty()
        {
            string symbol = "\"RootLevel.\"";
            string result = SymbolParser.GetNextLevel(symbol);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void GetRootLevel_Array_ReturnsExpectedValue()
        {
            string symbol = "RootLevel.[0]";
            string result = SymbolParser.GetRootLevel(symbol);
            Assert.AreEqual("RootLevel", result);
        }

        [TestMethod]
        public void GetNextLevel_Array_ReturnsExpectedValue()
        {
            string symbol = "[0]";
            string result = SymbolParser.GetNextLevel(symbol);
            Assert.AreEqual(string.Empty, result);
        }
    }
}
