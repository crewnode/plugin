using CrewNodePlugin.Games;
using CrewNodePlugin.Manager;
using CrewNodePlugin.Manager.Models;
using Impostor.Api.Events;
using Impostor.Api.Innersloth;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace CrewNodePlugin
{
    public class GameEventListener : IEventListener
    {
        private readonly ILogger<CrewNodePlugin> _logger;

        /// <summary>
        ///     Constructor for the class GameEventListener
        /// </summary>
        /// <param name="logger"></param>
        public GameEventListener(ILogger<CrewNodePlugin> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Creates an instance of the CrewNodeGame for the new lobby.
        /// </summary>
        /// <param name="e"></param>
        [EventListener]
        public void OnGameCreated(IGameCreatedEvent e)
        {
            // We need to initialise our game manager!
            GameManager.NewGame(e.Game);
        }

        /// <summary>
        ///     Updates our game manager when the game lobby is altered.
        /// </summary>
        /// <param name="e"></param>
        [EventListener]
        public void OnGameAltered(IGameAlterEvent e)
        {
            // Update our existing game
            GameManager.UpdateGameState(e.Game);
        }

        /// <summary>
        ///     Sets the game options just before the round starts.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        [EventListener]
        public async void OnGameStartingAsync(IGameStartingEvent e)
        {
            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) return;

            GameManager.UpdateGameState(e.Game);
            await game.GetGameModeManager().HandleEvent(e, "HandleGameStarting");
        }

        /// <summary>
        ///     Runs once the game has started.
        /// </summary>
        /// <param name="e"></param>
        [EventListener]
        public async void OnGameStartedAsync(IGameStartedEvent e)
        {
            // Has the game even started?
            if (e.Game.GameState != GameStates.Started) return;

            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) return;
            if (game.GetGameModeManager() == null) return;

            GameManager.UpdateGameState(e.Game);
            await game.GetGameModeManager().HandleEvent(e, "HandleGameStarted");
        }

        /// <summary>
        ///     Runs once a game has been destroyed
        /// </summary>
        /// <param name="e"></param>
        [EventListener]
        public void OnGameDestroyed(IGameDestroyedEvent e)
        {
            // Cleanup
            GameManager.DestroyGame(e.Game);
        }
    }
}

