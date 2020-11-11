using CrewNodePlugin.Manager;
using CrewNodePlugin.Utils.Types;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Packets
{
    class GameUpdate : ApiPacketData
    {
        public string creator { get; set; }
        public string gameCode { get; set; }
        public string gameMode { get; set; }
        public string gameState { get; set; }

        public GameUpdate(string creator, string gameCode, GameModeManager.Identity gameMode, string gameState)
        {
            this.creator = creator;
            this.gameCode = gameCode;
            this.gameMode = gameMode.ToString();
            this.gameState = gameState;
        }
    }
}
