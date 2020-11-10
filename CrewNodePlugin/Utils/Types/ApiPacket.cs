using System;
using System.Collections.Generic;
using System.Text;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Types
{
    class ApiPacket
    {
        public virtual PacketType GetApiType() { return PacketType.None; }

        public virtual string GetData() { return "{}"; }
    }
}
