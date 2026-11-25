using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TKH.S7Plus.Net.Tests.Network
{
    [TestClass]
    public class S7ClientTests
    {
        [TestMethod]
        public void Dispose_NeverConnected_DoesNotThrow()
        {
            new S7Client().Dispose();
            new S7Client().Disconnect();
            new S7Driver().Dispose();
        }
    }
}