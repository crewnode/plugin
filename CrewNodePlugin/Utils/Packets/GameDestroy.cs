using CrewNodePlugin.Utils.Types;
using static CrewNodePlugin.Utils.ApiUtils;

namespace CrewNodePlugin.Utils.Packets
{
    class GameDestroy : ApiPacketData
    {
        public string creator { get; set; }
        public string gameCode { get; set; }

        public GameDestroy(string creator, string gameCode)
        {
            this.creator = creator;
            this.gameCode = gameCode;
        }
    }
}
