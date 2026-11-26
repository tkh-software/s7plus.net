using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TKH.S7Plus.Net.Models;

namespace TKH.S7Plus.Net.Tests.Models
{
    [TestClass]
    public class S7VarNameListTests
    {
        private static MemoryStream Names(params string[] names)
        {
            var block = new List<byte>();
            foreach (string name in names)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(name);
                block.Add((byte)bytes.Length);
                block.AddRange(bytes);
                block.Add(0); // terminator
            }

            var buffer = new List<byte> { (byte)(block.Count >> 8), (byte)block.Count };
            buffer.AddRange(block);
            buffer.AddRange(new byte[] { 0, 0 }); // end of the list
            return new MemoryStream(buffer.ToArray());
        }

        [TestMethod]
        public void Deserialize_Names_ReturnsNames()
        {
            S7VarNameList list = S7VarNameList.Deserialize(Names("Lifebit", "MasterTime"));
            CollectionAssert.AreEqual(new[] { "Lifebit", "MasterTime" }, list.Names);
        }

        [TestMethod]
        public void Deserialize_NamesPaddedWithNul_ReturnsNamesWithoutPadding()
        {
            // As sent by PLCSIM V17 for some tags.
            S7VarNameList list = S7VarNameList.Deserialize(Names("E_P0800_ROB_\0", "Station\0\0\0"));
            CollectionAssert.AreEqual(new[] { "E_P0800_ROB_", "Station" }, list.Names);
        }
    }
}