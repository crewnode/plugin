using CrewNodePlugin.Utils.Packets;
using CrewNodePlugin.Utils.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CrewNodePlugin.Utils
{
    class ApiUtils
    {
        private static Dictionary<string, InstancePacket> _queue = new Dictionary<string, InstancePacket>();

        /// <summary>
        ///     The API Key which communicates with the CrewNode Web API,
        ///     we're not going to use a WebSocket because that's fucking stupid.
        /// </summary>
        private static readonly string ApiUrl = "https://crewnode.net/api/server/push";
        private static readonly string ApiKey = "Qj3B%p0nHgr@Qy1wwsVCR8$3@lQ4vbfAZb&%uKVfjPe$#fS&ciPN*by4JPh7J&WxR*@qMy*MpGZn*Hi6pm@cW^20ChZj7u6HdqJRR6vr1$a$UCO%faZQ$WmHm386xv";
        private static HttpClient _client = new HttpClient();

        public static void Queue(ApiPacket packet, string instanceId) =>
            _queue.Add(Guid.NewGuid().ToString(), new InstancePacket(packet, instanceId));

        private static void ClearQueue() => ApiUtils._queue.Clear();

        private static List<ApiPacket> FlushQueue()
        {
            lock (ApiUtils._queue)
            {
                List<ApiPacket> _toSync = new List<ApiPacket>();
                foreach (var (id, pkt) in _queue)
                {
                    // TODO: Use InstanceId somewhere... needs refactoring
                    _toSync.Add(pkt.Packet);
                }

                // Clear the queue
                ApiUtils.ClearQueue();
                return _toSync;
            }
        }

        public static async void ExecuteSync()
        {
            List<ApiPacket> _toSync = FlushQueue();
            if (_toSync.Count == 0) return;
            Console.WriteLine($"[ExecuteSync]: {_toSync.Count} items in the queue.");

            OutgoingPacket fullPacket = new OutgoingPacket(_toSync);
            string pkt;
            HttpRequestMessage _req = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(ApiUrl),
                Headers =
                {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer {ApiKey}" }
                },
                Content = new StringContent(pkt = JsonConvert.SerializeObject(fullPacket, Formatting.Indented), Encoding.UTF8, "application/json")
            };

            _ = await _client.SendAsync(_req);
            // TODO: Requeue items that were dropped if possible [?]
            // if (response.Content != null)
            //     Console.WriteLine("Received back:\n" + await response.Content.ReadAsStringAsync());
        }

        public enum PacketType
        {
            None = 0x00,
            GameNew = 0x01,
            GameUpdate = 0x02,
            GameDestroy = 0x03,
            PlayerAdd = 0x04,
            PlayerUpdate = 0x05,
            PlayerRemove = 0x06
        };

        private class OutgoingPacket
        {
            public List<ApiPacket> packets { get; private set; }

            public OutgoingPacket(List<ApiPacket> packets)
            {
                this.packets = packets;
            }
        }
    }
}
