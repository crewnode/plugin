using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Numerics;
using System.Collections.Concurrent;
using Impostor.Api.Innersloth.Customization;
using Impostor.Api.Net;
using CrewNodePlugin.Games;
using Impostor.Api.Innersloth;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using System;
using Impostor.Api.Net.Inner.Objects;
using CrewNodePlugin.Manager.Models;
using CrewNodePlugin.Manager;
using CrewNodePlugin.Utils;

namespace CrewNodePlugin
{
    public class PlayerEventListener : IEventListener
    {
        private readonly ILogger<CrewNodePlugin> _logger;

        public PlayerEventListener(ILogger<CrewNodePlugin> logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Runs on player move.
        /// </summary>
        /// <param name="e"></param>
        [EventListener]
        public async void OnPlayerMovementAsync(IPlayerMovementEvent e)
        {
            // Debugging
            if (CrewNodePlugin.debug)
                _logger.LogDebug(GameManagerUtils.GetTotalLobbyCount() + " lobbies, " + GameManagerUtils.GetTotalPlayerCount() + " players.");

            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) { Console.WriteLine("movement: game doesn't exist"); return; }
            await game.GetGameModeManager().HandleEvent(e, "HandlePlayerMovement");
        }

        /// <summary>
        ///     Runs on the player disconnect, makes sure that if the tagger left it reassigns it to somebody else.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        [EventListener]
        public async void OnPlayerDestroyedAsync(IPlayerDestroyedEvent e) {
            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) { Console.WriteLine("destroyed: game doesn't exist"); return; }
            await game.GetGameModeManager().HandleEvent(e, "HandlePlayerDestroyed");
        }

        /// <summary>
        ///     Runs on player joined
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        [EventListener]
        public async void OnPlayerJoinedAsync(IGamePlayerJoinedEvent e)
        {
            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) { Console.WriteLine("joined: game doesn't exist"); return; }
            await game.GetGameModeManager().HandleEvent(e, "HandlePlayerJoined");
        }

        /// <summary>
        ///     Runs on player left
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        [EventListener]
        public async void OnPlayerLeftAsync(IGamePlayerLeftEvent e)
        {
            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) { Console.WriteLine("left: game doesn't exist"); return; }
            await game.GetGameModeManager().HandleEvent(e, "HandlePlayerLeft");
        }

        /// <summary>
        ///     Runs on player spawned
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        [EventListener]
        public async void OnPlayerSpawnedAsync(IPlayerSpawnedEvent e)
        {
            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) { Console.WriteLine("spawned: game doesn't exist"); return; }
            await game.GetGameModeManager().HandleEvent(e, "HandlePlayerSpawned");
        }
    }
}