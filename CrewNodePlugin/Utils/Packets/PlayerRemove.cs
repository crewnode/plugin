using CrewNodePlugin.Utils.Types;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Packets
{
    class PlayerRemove : ApiPacketData
    {
        public string gameCode { get; set; }
        public string playerName { get; set; }
        public string ipAddress { get; set; }
        public string discordUid { get; set; }

        public PlayerRemove(string gameCode, string playerName, string ipAddress, string discordUid)
        {
            this.gameCode = gameCode;
            this.playerName = playerName;
            this.ipAddress = ipAddress;
            this.discordUid = discordUid;
        }
    }
}
