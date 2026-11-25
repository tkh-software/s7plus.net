using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TKH.S7Plus.Net.Tests.Network
{
    [TestClass]
    public class CotpNetworkStreamWaitTests
    {
        private static readonly TimeSpan _limit = TimeSpan.FromSeconds(10);
        private TcpListener _listener = null!;
        private TcpClient _client = null!;
        private Socket _plc = null!;
        private CotpNetworkStream _stream = null!;

        [TestInitialize]
        public async Task Connect()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            _client = new TcpClient();
            Task<Socket> accept = _listener.AcceptSocketAsync();
            await _client.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)_listener.LocalEndpoint).Port);
            _plc = await accept;
            _stream = new CotpNetworkStream(_client.GetStream());
        }

        [TestCleanup]
        public void Close()
        {
            _stream.Dispose();
            _client.Dispose();
            _plc.Dispose();
            _listener.Stop();
        }

        private static byte[] Frame(byte[] payload)
        {
            int length = payload.Length + 7;
            return new byte[] { 0x03, 0x00, (byte)(length >> 8), (byte)length, 0x02, 0xF0, 0x80 }.Concat(payload).ToArray();
        }

        [TestMethod]
        public async Task WaitForWork_IdleConnection_WakesAboutTenTimesPerSecond()
        {
            int wakeups = 0;
            var watch = Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds < 1000)
            {
                await _stream.WaitForWorkAsync(TimeSpan.FromMilliseconds(100), CancellationToken.None);
                _stream.Update();
                Assert.AreEqual(0, _stream.ReceivePacket().Length);
                wakeups++;
            }

            // Polling with Task.Delay(1) wakes up about 1000 times per second.
            Assert.IsTrue(wakeups >= 5 && wakeups <= 20, $"{wakeups} wake-ups");
        }

        [TestMethod]
        public async Task WaitForWork_DataAndQueuedSends_WakeAtOnce()
        {
            Task waitForData = _stream.WaitForWorkAsync(TimeSpan.FromSeconds(30), CancellationToken.None);
            await Task.Delay(50);
            Assert.IsFalse(waitForData.IsCompleted);
            _plc.Send(Frame(new byte[] { 1, 2, 3 }));
            await waitForData.WaitAsync(_limit);
            _stream.Update();
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, _stream.ReceivePacket());

            Task waitForSend = _stream.WaitForWorkAsync(TimeSpan.FromSeconds(30), CancellationToken.None);
            await Task.Delay(50);
            Assert.IsFalse(waitForSend.IsCompleted);
            _stream.SendPacket(new byte[] { 9, 8, 7 });
            await waitForSend.WaitAsync(_limit);
            _stream.Update();

            byte[] received = new byte[10];
            _plc.ReceiveTimeout = (int)_limit.TotalMilliseconds;
            int count = 0;
            while (count < received.Length)
                count += _plc.Receive(received, count, received.Length - count, SocketFlags.None);
            CollectionAssert.AreEqual(Frame(new byte[] { 9, 8, 7 }), received);
        }

        [TestMethod]
        public async Task WaitForWork_PeerClosed_ThrowsInsteadOfWakingForever()
        {
            _plc.Shutdown(SocketShutdown.Both);
            _plc.Close();

            IOException? error = null;
            try
            {
                for (int i = 0; i < 100; i++)
                    await _stream.WaitForWorkAsync(TimeSpan.FromSeconds(30), CancellationToken.None).WaitAsync(_limit);
            }
            catch (IOException ex)
            {
                error = ex;
            }

            Assert.IsNotNull(error);
            Assert.AreEqual("Connection closed by the PLC.", error.Message);
        }
    }
}