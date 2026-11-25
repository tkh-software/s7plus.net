using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TKH.S7Plus.Net.Tests.Network
{
    [TestClass]
    public class CotpNetworkStreamTests
    {
        private static byte[] Frame(byte[] payload)
        {
            int length = payload.Length + 7;
            return new byte[] { 0x03, 0x00, (byte)(length >> 8), (byte)length, 0x02, 0xF0, 0x80 }.Concat(payload).ToArray();
        }

        [TestMethod]
        public async Task ReceivePacket_FrameSplitByTcp_ReturnsCompletePayload()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                using var client = new TcpClient();
                Task<Socket> accept = listener.AcceptSocketAsync();
                await client.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
                using Socket plc = await accept;
                using var stream = new CotpNetworkStream(client.GetStream());

                byte[] payload = Enumerable.Range(0, 3000).Select(i => (byte)(i * 7)).ToArray();
                byte[] frame = Frame(payload);
                // Pieces end inside the TPKT header, inside the COTP header and twice inside the data.
                Task send = Task.Run(async () =>
                {
                    foreach (var (start, end) in new[] { (0, 2), (2, 5), (5, 900), (900, 2000), (2000, frame.Length) })
                    {
                        plc.Send(frame, start, end - start, SocketFlags.None);
                        await Task.Delay(30);
                    }
                });

                byte[] packet = Array.Empty<byte>();
                DateTime deadline = DateTime.UtcNow.AddSeconds(10);
                while (packet.Length == 0 && DateTime.UtcNow < deadline)
                {
                    stream.Update();
                    packet = stream.ReceivePacket();
                    if (packet.Length == 0)
                        await Task.Delay(5);
                }
                await send;

                CollectionAssert.AreEqual(payload, packet);
            }
            finally
            {
                listener.Stop();
            }
        }
    }
}