using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CrewNodePlugin.Games
{
    class GameModeType
    {
        public static ILogger<CrewNodePlugin> Logger;

        // Player Calls
        public virtual async ValueTask HandlePlayerJoin(IGamePlayerJoinedEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandlePlayerJoin called");
        }

        public virtual async ValueTask HandlePlayerDestroyed(IPlayerDestroyedEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandlePlayerDestroyed called");
        }

        public virtual async ValueTask HandlePlayerSpawn(IPlayerSpawnedEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandlePlayerSpawn called");
        }

        public virtual async ValueTask HandlePlayerDied(IPlayerExileEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandlePlayerDied called");
        }

        public virtual async ValueTask HandlePlayerMurdered(IPlayerMurderEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandlePlayerMurdered called");
        }

        public virtual async ValueTask HandlePlayerMovement(IPlayerMovementEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandlePlayerMovement called");
        }

        public virtual async ValueTask HandlePlayerReported(IPlayerReportedBodyEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandlePlayerReported called");
        }

        public virtual async ValueTask HandlePlayerChat(IPlayerChatEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandlePlayerChat called");
        }

        // Game Calls
        public virtual async ValueTask HandleGameStarting(IGameStartingEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandleGameStarting called");
        }

        public virtual async ValueTask HandleGameStarted(IGameStartedEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandleGameStarted called");
        }

        public virtual async ValueTask HandleGameEnded(IGameEndedEvent e)
        {
            if (CrewNodePlugin.debug) Console.WriteLine("HandleGameEnded called");
        }

        // Logger
        public static void SetLogger(ILogger<CrewNodePlugin> logger)
        {
            Logger = logger;
        }
    }
}
