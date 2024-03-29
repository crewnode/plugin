﻿using CrewNodePlugin.Games;
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
        public async void OnGameCreatedAsync(IGameCreatedEvent e)
        {
            // We need to initialise our game manager!
            CrewNodeGame game = GameManager.NewGame(e.Game);
            if (game == null) return;

            await game.GetGameModeManager().HandleEvent(e, "HandleGameCreated");
        }

        /// <summary>
        ///     Updates our game manager when the game lobby is altered.
        /// </summary>
        /// <param name="e"></param>
        [EventListener]
        public async void OnGameAlteredAsync(IGameAlterEvent e)
        {
            // Update our existing game
            CrewNodeGame game = GameManager.UpdateGameState(e.Game);
            if (game == null) return;

            await game.GetGameModeManager().HandleEvent(e, "HandleGameAltered");
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
        ///     Runs once a game has finished playing, and returning back to lobby.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        [EventListener]
        public async void OnGameEndedAsync(IGameEndedEvent e)
        {
            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) return;

            GameManager.UpdateGameState(e.Game);
            await game.GetGameModeManager().HandleEvent(e, "HandleGameEnded");
        }

        /// <summary>
        ///     Runs once a game has been destroyed
        /// </summary>
        /// <param name="e"></param>
        [EventListener]
        public async void OnGameDestroyedAsync(IGameDestroyedEvent e)
        {
            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) return;
            await game.GetGameModeManager().HandleEvent(e, "HandleGameDestroyed");

            // Cleanup
            GameManager.DestroyGame(e.Game);
        }
    }
}

