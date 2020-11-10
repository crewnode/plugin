using CrewNodePlugin.Utils.Types;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Packets
{
    class PlayerRemove : ApiPacket
    {
        public override PacketType GetApiType() => PacketType.PlayerRemove;

        /// <summary>
        ///     Override the GetData response
        /// </summary>
        /// <returns></returns>
        public override string GetData()
        {
            return base.GetData();
        }

        private class PlayerRemovePacket
        {
            // TODO
        }
    }
}
