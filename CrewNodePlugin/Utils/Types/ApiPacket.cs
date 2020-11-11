using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Types
{
    class ApiPacket
    {
        public ApiPacketData data { get; set; }
        public string type { get; set; }

        public ApiPacket(PacketType type, ApiPacketData data)
        {
            this.type = type.ToString();
            this.data = data;
        }
    }
}
