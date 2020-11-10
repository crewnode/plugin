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

        [EventListener]
        public async void OnPlayerMovementAsync(IPlayerMovementEvent e)
        {
            // Debugging
            if (CrewNodePlugin.debug)
                _logger.LogDebug(GameManagerUtils.GetTotalLobbyCount() + " lobbies, " + GameManagerUtils.GetTotalPlayerCount() + " players.");

            // Has the game even started?
            if (e.Game.GameState != GameStates.Started) return;

            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) return;
            await game.GetGameModeManager().HandleEvent(e, "HandlePlayerMovement");
        }

        /// <summary>
        /// Runs on the player disconnect, makes sure that if the tagger left it reassigns it to somebody else.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task OnPlayerDisconnectAsync(IPlayerDestroyedEvent e) {
            // Has the game even started?
            if (e.Game.GameState != GameStates.Started) return;

            // Manager Takeover
            CrewNodeGame game = GameManager.GetGame(e.Game.Code);
            if (game == null) return;
            await game.GetGameModeManager().HandleEvent(e, "HandlePlayerDestroyed");
        }
    }
}