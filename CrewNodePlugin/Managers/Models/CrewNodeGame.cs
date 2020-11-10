using Impostor.Api.Games;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrewNodePlugin.Manager.Models
{
    class CrewNodeGame
    {
        IGame _game;
        GameModeManager _gmm;
        PlayerManager _pm;

        public CrewNodeGame(IGame game)
        {
            _game = game;
            _gmm = new GameModeManager(game);
            _pm = new PlayerManager(game);
        }

        public IGame GetGameInstance() => this._game;
        public GameModeManager GetGameModeManager() => this._gmm;
        public PlayerManager GetPlayerManager() => this._pm;
        public void UpdateState(IGame game) => this._gmm.UpdateGame(game);

    }
}
