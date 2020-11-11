using CrewNodePlugin.Games;
using Impostor.Api.Events;
using Impostor.Api.Games;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CrewNodePlugin.Manager
{
    class GameModeManager
    {
        private IGame _game;
        private Identity _selectedGameMode = Identity.Tag;
        private GameModeType _gameMode = null;

        /// <summary>
        ///     GameModeManager is used to manage an individual lobbies settings.
        /// </summary>
        /// <param name="game"></param>
        public GameModeManager(IGame game)
        {
            _game = game;
            this.ResetGameMode();
        }

        /// <summary>
        ///     Update the Game state reference.
        /// </summary>
        /// <param name="game"></param>
        public void UpdateGame(IGame game)
        {
            this._game = game;
        }

        /// <summary>
        ///     Set the game mode of this lobby.
        /// </summary>
        /// <param name="gameMode"></param>
        public void SetGameMode(Identity gameMode)
        {
            this._selectedGameMode = gameMode;
            switch (gameMode)
            {
                case Identity.Regular: break;
                case Identity.HideAndSeek: break;
                case Identity.HundredPlayer: break;
                case Identity.Tag:
                    _gameMode = new Games.Tag();
                    break;
            }
        }

        /// <summary>
        ///     Reset the game mode (i.e. when a game has ended)
        /// </summary>
        public void ResetGameMode() => this.SetGameMode(_selectedGameMode);

        /// <summary>
        ///     Returns the selected gamemode.
        /// </summary>
        public Identity GetGameMode() => _selectedGameMode;

        /// <summary>
        ///     Handle an event passed from Impostor's API
        /// </summary>
        /// <param name="e">A Game Event</param>
        /// <param name="eventName">The Event name to handle</param>
        /// <returns></returns>
        public async ValueTask HandleEvent(IGameEvent e, string eventName)
        {
            // Check if the game mode has been initiated before
            // attempting to handle an invalid event
            if (_gameMode == null) return;

            MethodInfo method = _gameMode.GetType().GetMethod(eventName);
            if (method == null) return;

            // TODO: Figure out why this errors, even though it does not
            //       cause any adverse side affects...
            try { await method.InvokeAsync(_gameMode, new object[] { e }); }
            catch { }
        }

        /// <summary>
        ///     Gamemode Identifiers
        /// </summary>
        public enum Identity : int
        {
            Regular = 0x00,
            Tag = 0x01,
            HideAndSeek = 0x02,
            HundredPlayer = 0x03
        };
    }
}
