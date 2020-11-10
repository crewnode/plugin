using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrewNodePlugin.Manager.Models;
using CrewNodePlugin.Utils;
using Impostor.Api.Games;

namespace CrewNodePlugin.Manager
{
    class GameManager
    {
        private static CrewNodeDictionary<string, CrewNodeGame> _games = new CrewNodeDictionary<string, CrewNodeGame>();

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
            Task.Run(() => DiscordUtils.CreateChannel(game, _games[game.Code]));
            return _games[game.Code];
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
            Task.Run(() => DiscordUtils.UpdateChannel(game, cnGame, "update"));
            cnGame.UpdateState(game);
        }

        /// <summary>
        ///     End a game.
        /// </summary>
        /// <param name="game"></param>
        public static void EndGame(IGame game)
        {
            if (_games.ContainsKey(game.Code))
                NewGame(game);

            // Update state
            CrewNodeGame cnGame = _games[game.Code];
            Task.Run(() => DiscordUtils.UpdateChannel(game, cnGame, "end"));
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
            Task.Run(() => DiscordUtils.RemoveChannel(game, _games[game.Code]));
            _games.Remove(game.Code);
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
        ///     Get all running games.
        /// </summary>
        public static CrewNodeDictionary<string, CrewNodeGame> GetAllGames()
        {
            return _games;
        }
    }
}
