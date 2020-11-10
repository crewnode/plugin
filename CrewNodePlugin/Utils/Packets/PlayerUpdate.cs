using CrewNodePlugin.Utils.Types;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Packets
{
    class PlayerUpdate : ApiPacket
    {
        public override PacketType GetApiType() => PacketType.PlayerUpdate;

        /// <summary>
        ///     Override the GetData response
        /// </summary>
        /// <returns></returns>
        public override string GetData()
        {
            return base.GetData();
        }

        private class PlayerUpdatePacket
        {
            // TODO
        }
    }
}
