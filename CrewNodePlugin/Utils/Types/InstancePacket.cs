using System;
using System.Collections.Generic;
using System.Text;

namespace CrewNodePlugin.Utils.Types
{
    class InstancePacket
    {
        public ApiPacket Packet { get; set; }
        public string InstanceId { get; set; }

        public InstancePacket(ApiPacket packet, string instanceId)
        {
            this.Packet = packet;
            this.InstanceId = instanceId;
        }
    }
}
