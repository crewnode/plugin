using CrewNodePlugin.Manager;
using CrewNodePlugin.Utils;
using CrewNodePlugin.Utils.Packets;
using CrewNodePlugin.Utils.Types;
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
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandlePlayerJoin called");
        }

        public virtual async ValueTask HandlePlayerDestroyed(IPlayerDestroyedEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandlePlayerDestroyed called");

            if (CrewNodePlugin.debug)
            {
                // Sort out packets
                PlayerRemove removePlayerPacket = new PlayerRemove(
                    e.Game.Code,
                    e.ClientPlayer.Character.PlayerInfo.PlayerName,
                    e.ClientPlayer.Client.Connection.EndPoint.ToString(),
                    ""
                );
                ApiUtils.Queue(new ApiPacket(ApiUtils.PacketType.PlayerRemove, removePlayerPacket), "");
            }
        }

        public virtual async ValueTask HandlePlayerLeft(IGamePlayerLeftEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandlePlayerLeft called");
            
            // TODO (Simple): Fix IGamePlayerLeftEvent access to e.Player
        }

        public virtual async ValueTask HandlePlayerSpawned(IPlayerSpawnedEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandlePlayerSpawned called");

            if (CrewNodePlugin.debug)
            {
                // Sort out packets
                var player = e.ClientPlayer.Character.PlayerInfo;
                PlayerAdd newPlayerPacket = new PlayerAdd(
                    e.Game.Code,
                    player.PlayerName,
                    player.ColorId,
                    (int)player.SkinId,
                    (int)player.HatId,
                    (int)player.PetId,
                    0.0f,
                    0.0f,
                    e.ClientPlayer.Client.Connection.EndPoint.ToString(),
                    ""
                );
                ApiUtils.Queue(new ApiPacket(ApiUtils.PacketType.PlayerAdd, newPlayerPacket), "");
            }
        }

        public virtual async ValueTask HandlePlayerDied(IPlayerExileEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandlePlayerDied called");
        }

        public virtual async ValueTask HandlePlayerMurdered(IPlayerMurderEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandlePlayerMurdered called");
        }

        public virtual async ValueTask HandlePlayerMovement(IPlayerMovementEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandlePlayerMovement called");

            // TODO: Move "Tag" PlayerLocation overrides to the PlayerManager
        }

        public virtual async ValueTask HandlePlayerReported(IPlayerReportedBodyEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandlePlayerReported called");
        }

        public virtual async ValueTask HandlePlayerChat(IPlayerChatEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandlePlayerChat called");
        }

        // Game Calls
        public virtual async ValueTask HandleGameCreated(IGameCreatedEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandleGameCreated called");

            if (CrewNodePlugin.debug)
            {
                // Sort out packets
                GameModeManager.Identity currentGamemode = GameManager.GetGame(e.Game.Code).GetGameModeManager().GetGameMode();
                GameNew newGamePacket = new GameNew(e.Game.HostId.ToString(), e.Game.Code, currentGamemode, e.Game.GameState);
                ApiUtils.Queue(new ApiPacket(ApiUtils.PacketType.GameNew, newGamePacket), "");
            }
        }

        public virtual async ValueTask HandleGameAltered(IGameAlterEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandleGameAltered called");
        }

        public virtual async ValueTask HandleGameStarting(IGameStartingEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandleGameStarting called");
        }

        public virtual async ValueTask HandleGameStarted(IGameStartedEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandleGameStarted called");
        }

        public virtual async ValueTask HandleGameEnded(IGameEndedEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandleGameEnded called");
        }

        public virtual async ValueTask HandleGameDestroyed(IGameDestroyedEvent e)
        {
            if (CrewNodePlugin.debug && CrewNodePlugin.verbose) Console.WriteLine("HandleGameDestroyed called");

            if (CrewNodePlugin.debug)
            {
                // Sort out packets
                try
                {
                    GameDestroy destroyGamePacket = new GameDestroy(e.Game.HostId.ToString(), e.Game.Code);
                    ApiUtils.Queue(new ApiPacket(ApiUtils.PacketType.GameDestroy, destroyGamePacket), "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("died because of: " + ex.Message + "\n\n" + ex.StackTrace);
                }
            }
        }

        // Logger
        public static void SetLogger(ILogger<CrewNodePlugin> logger)
        {
            Logger = logger;
        }
    }
}
