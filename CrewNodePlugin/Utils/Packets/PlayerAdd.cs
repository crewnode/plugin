using CrewNodePlugin.Utils.Types;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Packets
{
    class PlayerAdd : ApiPacketData
    {
        public string gameCode { get; set; }
        public string playerName { get; set; }
        public int playerColor { get; set; }
        public int playerSkin { get; set; }
        public int playerHat { get; set; }
        public int playerPet { get; set; }
        public double playerPosX { get; set; }
        public double playerPosY { get; set; }
        public string ipAddress { get; set; }
        public string discordUid { get; set; }

        public PlayerAdd(string gameCode, string playerName, int playerColor, int playerSkin, int playerHat, int playerPet, double playerPosX, double playerPosY, string ipAddress, string discordUid)
        {
            this.gameCode = gameCode;
            this.playerName = playerName;
            this.playerColor = playerColor;
            this.playerSkin = playerSkin;
            this.playerHat = playerHat;
            this.playerPet = playerPet;
            this.playerPosX = playerPosX;
            this.playerPosY = playerPosY;
            this.ipAddress = ipAddress;
            this.discordUid = discordUid;
        }
    }
}
