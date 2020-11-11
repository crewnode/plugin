using CrewNodePlugin.Manager;
using CrewNodePlugin.Utils.Types;
using Impostor.Api.Innersloth;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Packets
{
    class GameNew : ApiPacketData
    {
        public string creator { get; set; }
        public string gameCode { get; set; }
        public string gameMode { get; set; }
        public string gameState { get; set; }

        public GameNew(string creator, string gameCode, GameModeManager.Identity gameMode, GameStates gameState)
        {
            this.creator = creator;
            this.gameCode = gameCode;
            this.gameMode = gameMode.ToString();
            this.gameState = gameState.ToString();
        }
    }
}
