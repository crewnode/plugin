using Impostor.Api.Games;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrewNodePlugin.Manager
{
    class PlayerManager
    {
        private IGame _game;

        /// <summary>
        ///     PlayerManager is used to manage an individual lobbies players.
        /// </summary>
        /// <param name="game"></param>
        public PlayerManager(IGame game)
        {
            _game = game;
        }
    }
}
