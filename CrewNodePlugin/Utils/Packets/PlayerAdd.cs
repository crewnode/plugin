using CrewNodePlugin.Utils.Types;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Packets
{
    class PlayerAdd : ApiPacket
    {
        public override PacketType GetApiType() => PacketType.PlayerAdd;

        /// <summary>
        ///     Override the GetData response
        /// </summary>
        /// <returns></returns>
        public override string GetData()
        {
            return base.GetData();
        }

        private class PlayerAddPacket
        {
            // TODO
        }
    }
}
