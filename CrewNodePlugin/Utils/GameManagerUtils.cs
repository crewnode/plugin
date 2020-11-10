using CrewNodePlugin.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrewNodePlugin.Utils
{
    class GameManagerUtils
    {
        public static int GetTotalPlayerCount() => GameManager
                                                    .GetAllGames()
                                                    .Where(game => game.Value.GetGameInstance() != null)
                                                    .Select(game => game.Value.GetGameInstance().PlayerCount).Sum();

        public static int GetTotalLobbyCount() => GameManager.GetAllGames().Count;
    }
}
