using System;
using System.Collections.Generic;
using CrewNodePlugin.Manager.Models;
using Impostor.Api.Games;

namespace CrewNodePlugin.Manager
{
    class GameManager
    {
        private static Dictionary<string, CrewNodeGame> _games = new Dictionary<string, CrewNodeGame>();

        /// <summary>
        ///     Create a new game for game management.
        /// </summary>
        /// <param name="game">Game Object</param>
        /// <returns></returns>
        public static CrewNodeGame NewGame(IGame game)
        {
            if (String.IsNullOrEmpty(game.Code))
                return null;
            else if (_games.ContainsKey(game.Code))
                return _games[game.Code];

            _games.Add(game.Code, new CrewNodeGame(game));
            return _games[game.Code];
        }

        /// <summary>
        ///     Get a lobby instance from the game code.
        /// </summary>
        /// <param name="gameCode">Game Code</param>
        /// <returns>CrewNodeGame</returns>
        public static CrewNodeGame GetGame(string gameCode)
        {
            if (!_games.ContainsKey(gameCode))
                return null;
            return _games[gameCode];
        }

        /// <summary>
        ///     Update the game state of an existing game.
        /// </summary>
        /// <param name="game">Game Object</param>
        public static void UpdateGameState(IGame game)
        {
            if (_games.ContainsKey(game.Code))
                NewGame(game);

            // Update state
            CrewNodeGame cnGame = _games[game.Code];
            cnGame.UpdateState(game);
        }

        /// <summary>
        ///     Destroy a game.
        /// </summary>
        /// <param name="game">Game Object</param>
        public static void DestroyGame(IGame game)
        {
            if (!_games.ContainsKey(game.Code))
                return;

            // Remove the game
            _games.Remove(game.Code);
        }

        /// <summary>
        ///     Get all running games.
        /// </summary>
        public static Dictionary<string, CrewNodeGame> GetAllGames()
        {
            return _games;
        }
    }
}
