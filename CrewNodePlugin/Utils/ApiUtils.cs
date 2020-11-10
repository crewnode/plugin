using CrewNodePlugin.Utils.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrewNodePlugin.Utils
{
    class ApiUtils
    {
        private static Dictionary<string, InstancePacket> _queue = new Dictionary<string, InstancePacket>();

        /// <summary>
        ///     The API Key which communicates with the CrewNode Web API,
        ///     we're not going to use a WebSocket because that's fucking stupid.
        /// </summary>
        private static readonly string ApiKey = "Qj3B%p0nHgr@Qy1wwsVCR8$3@lQ4vbfAZb&%uKVfjPe$#fS&ciPN*by4JPh7J&WxR*@qMy*MpGZn*Hi6pm@cW^20ChZj7u6HdqJRR6vr1$a$UCO%faZQ$WmHm386xv";

        private static void Queue(ApiPacket packet, string instanceId) =>
            _queue.Add(Guid.NewGuid().ToString(), new InstancePacket(packet, instanceId));

        private static void ClearQueue() => ApiUtils._queue.Clear();

        public static void FlushQueue()
        {
            lock (ApiUtils._queue)
            {
                foreach (var (id, pkt) in _queue)
                {
                    // TODO: Process each item in the queue to the API
                }

                // Clear the queue
                ApiUtils.ClearQueue();
            }
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
    }
}
