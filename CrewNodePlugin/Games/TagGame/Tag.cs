using CrewNodePlugin.Games.TagGame;
using CrewNodePlugin.Manager;
using CrewNodePlugin.Manager.Models;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using static CrewNodePlugin.Games.TagGame.Utils;

namespace CrewNodePlugin.Games
{
    class Tag : GameModeType
    {
        private ConcurrentDictionary<int, Player> _players = new ConcurrentDictionary<int, Player>();
        private CancellationTokenSource _cts;
        private int _originalPlayerCount = -1;
        private int _gameEnds = -1;
        private int _roundStarts = -1;
        public bool _roundStarted = false;

        // Configurables
        private const int _kickAfterSeconds = 30; // Best: 30
        private const int _updatePlayerPosAfter = 5; // Best: 5
        private const int _minNumOfMovements = 8; //Best: 8

        /// <summary>
        ///     Handle player movement for the Tag gamemode
        /// </summary>
        /// <param name="e"></param>
        public override async ValueTask HandlePlayerMovement(IPlayerMovementEvent e)
        {
            await base.HandlePlayerMovement(e);
            if (!this._roundStarted || _players.Count < 1) return;

            Player movedPlayer = UpdatePlayerPosition(e);
            foreach (var player in _players)
            {
                if (!e.Game.Players.Any((p) => player.Value.client.Character.PlayerInfo.PlayerId == p.Character.PlayerInfo.PlayerId)) continue;
                if (movedPlayer.isTagged == player.Value.isTagged) continue;
                if (Vector2.Distance(player.Value.position, movedPlayer.position) >= Utils.infectionRange) continue;
                if (PlayersRequireCooldown(movedPlayer, player.Value)) continue;

                // Update flags
                movedPlayer.isTagged = !movedPlayer.isTagged;
                player.Value.isTagged = !player.Value.isTagged;

                // Update states
                var updatedCooldown = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + Utils.tagTimer;
                movedPlayer.cooldown = (player.Value.client.Character.PlayerInfo.PlayerId, updatedCooldown);
                player.Value.cooldown = (movedPlayer.client.Character.PlayerInfo.PlayerId, updatedCooldown);

                // Update outfits
                await movedPlayer.client.Character.SetOutfitAsync(movedPlayer.isTagged ? Utils.TaggedOutfit : Utils.RegularOutfit);
                await player.Value.client.Character.SetOutfitAsync(player.Value.isTagged ? Utils.TaggedOutfit: Utils.RegularOutfit);
            }
        }

        /// <summary>
        ///     Handle player disconnection
        /// </summary>
        /// <param name="e"></param>
        public override async ValueTask HandlePlayerDestroyed(IPlayerDestroyedEvent e)
        {
            await base.HandlePlayerDestroyed(e);
            if (_players[e.PlayerControl.PlayerInfo.PlayerId].isTagged)
                await SelectNewTagger();
            _players.TryRemove(e.PlayerControl.PlayerInfo.PlayerId, out _);
        }

        /// <summary>
        ///     Handle game starting
        /// </summary>
        /// <param name="e"></param>
        public override async ValueTask HandleGameStarting(IGameStartingEvent e)
        {
            if(CrewNodePlugin.debug) return;
            await base.HandleGameStarting(e);
            e.Game.Options.NumEmergencyMeetings = 0;
            e.Game.Options.ImpostorLightMod = .5f;
            e.Game.Options.CrewLightMod = .5f;
            await e.Game.SyncSettingsAsync();
        }

        /// <summary>
        ///     Handle game now that it has started
        /// </summary>
        /// <param name="e"></param>
        public override async ValueTask HandleGameStarted(IGameStartedEvent e)
        {
            await base.HandleGameStarted(e);
            Console.WriteLine("im in tag");
            this._originalPlayerCount = e.Game.PlayerCount;

            // Make sure all of our players exist
            foreach (var player in e.Game.Players)
                this.AddPlayerInstance(player.Character.PlayerInfo.PlayerId, player.Client.Player);

            // Start the Tag gamemode in another thread
            this._cts = new CancellationTokenSource();
            this._gameEnds = Utils.gameEnd;
            this._roundStarts = 30;
            Console.WriteLine("Hit");
            PluginUtils.RunTask(async () => { await HandleTagInterval(e); }, 1, _cts.Token);
        }

        /// <summary>
        ///     Ran every second, to update the game state of the game mode
        /// </summary>
        private async ValueTask HandleTagInterval(IGameStartedEvent e)
        {
            Console.WriteLine(DateTime.UtcNow);
            // Player Name Updates
            string message = this._roundStarts != 0 ? $"Starting In {this._roundStarts--}" : $"Ends {Utils.SecondsToFormat(this._gameEnds--)}";

            // Handle Each Player
            foreach (var player in _players)
            {
                // Quick-access variables
                var pValue = player.Value;
                var characterInfo = pValue.client.Character;

                // Check for invalid game state
                if (e.Game.GameState != Impostor.Api.Innersloth.GameStates.Started) break;

                // Validate each player
                if (!e.Game.Players.Any((p) => characterInfo.PlayerInfo.PlayerId == p.Character.PlayerInfo.PlayerId))
                    continue;

                // Kill each player if the game timer has reached zero
                if (this._gameEnds <= 0)
                {
                    await characterInfo.SetMurderedAsync();
                    continue;
                }
                // Or, if the game has just started, set everyones attire
                else if (this._gameEnds == Utils.gameEnd)
                {
                    await characterInfo.SetOutfitAsync(pValue.isTagged ? Utils.TaggedOutfit : Utils.RegularOutfit);
                    this._roundStarted = true;
                }

                // Checks if the beginning round is still running, and makes sure everybody is assigned the neutral outfit.
                if (this._roundStarts != 0 && characterInfo.PlayerInfo.ColorId != Utils.NeutralOutfit.Color)
                    await characterInfo.SetOutfitAsync(Utils.NeutralOutfit);
                // If the game has started makes sure that the tagger/regular on cooldown has a blinking red color. 
                else if (this.PlayerIsInCooldown(pValue) && Tag.ValidAttireCooldown(pValue) && !Tag.OnSelfCooldown(pValue))
                    await characterInfo.SetOutfitAsync(pValue.isTagged ? Utils.CooldownTaggedOutfit : Utils.CoolDownRegularOutfit);
                // If the player isnt on cooldown makes sure they have the correct attire
                else if (Tag.ValidAttire(pValue) && !Tag.OnSelfCooldown(pValue))
                    await characterInfo.SetOutfitAsync(pValue.isTagged ? Utils.TaggedOutfit : Utils.RegularOutfit);

                // Update name
                await characterInfo.SetNameAsync($"{pValue.playerName} {message}");

                // Reset spawn locations
                if (this._roundStarts == Utils.roundStart - Tag._updatePlayerPosAfter)
                {
                    var updatedPos = Extensions.GetSpawnLocation(characterInfo.PlayerInfo.PlayerId, this._originalPlayerCount);
                    pValue.position = updatedPos;
                    await characterInfo.NetworkTransform.SnapToAsync(updatedPos);
                }

                // Check for AFK
                if (this._gameEnds == (Utils.gameEnd - Tag._kickAfterSeconds) && Tag.IsAFK(pValue, _originalPlayerCount))
                {
                    // Makes sure to only pick new taggers when an afk tagger gets kicked...
                    if (pValue.isTagged)
                    {
                        Logger.LogDebug($"{pValue.playerName} was the tagger, but has now been kicked.");
                        await this.SelectNewTagger();
                        if (!CrewNodePlugin.debug)
                            _players.Remove(player.Key, out _);
                    }

                    if (!CrewNodePlugin.debug) await pValue.client.KickAsync(); else await characterInfo.SetOutfitAsync(Utils.CoolDownRegularOutfit);
                    Logger.LogWarning($"{pValue.playerName} kicked");
                }
            }

            // Check for game ended
            if (this._gameEnds == 0 || e.Game.GameState == Impostor.Api.Innersloth.GameStates.Ended)
            {
                // Cleanup for the hell of it
                _originalPlayerCount = -1;
                _players.Clear();
                this._cts.Cancel();

                // Reset the gamemode back to default settings
                GameManager.GetGame(e.Game.Code).GetGameModeManager().ResetGameMode();
            }
        }

        private static bool OnSelfCooldown(Player player)
        {
            long timeInMilliseconds = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

            var (id, cooldown) = player.cooldown;

            var onTagTimer = timeInMilliseconds <= cooldown && id == player.client.Character.PlayerInfo.PlayerId;

            return onTagTimer;
        }



        /// <summary>
        ///     Handle a game that has ended
        /// </summary>
        /// <param name="e"></param>
        public override async ValueTask HandleGameEnded(IGameEndedEvent e)
        {
            await base.HandleGameEnded(e);
            GameManager.GetGame(e.Game.Code).GetGameModeManager().ResetGameMode();
        }

        /// <summary>
        ///     Checks whether the player would be considered AFK.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns></returns>
        private static bool IsAFK(Player player, int orginPlayCount)
        {
            bool afkCheck1 = player.position == Extensions.GetSpawnLocation(player.client.Character.PlayerInfo.PlayerId, orginPlayCount);
            bool afkCheck2 = player.timesMoved < Tag._minNumOfMovements;
            return afkCheck1 || afkCheck2;
        }

        /// <summary>
        /// Checks if the player is wearing the proper attire while off cooldown.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns></returns>
        private static bool ValidAttire(Player player)
        {
            byte colorId = player.client.Character.PlayerInfo.ColorId;

            bool properAttireTagged = (player.isTagged && colorId != Utils.TaggedOutfit.Color);
            bool properAttireRegular = (!player.isTagged && colorId != Utils.RegularOutfit.Color);

            return properAttireTagged || properAttireRegular;
        }

        /// <summary>
        /// Checks if the player is wearing the proper attire while on cooldown.
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns></returns>
        private static bool ValidAttireCooldown(Player player)
        {
            return player.client.Character.PlayerInfo.ColorId != Utils.CooldownTaggedOutfit.Color;
        }

        /// <summary>
        ///     Check if a player is in cooldown mode
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool PlayerIsInCooldown(Player player)
        {
            long timeInMilliseconds = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
            var (_, pCooldown) = player.cooldown;
            return timeInMilliseconds <= pCooldown;
        }

        /// <summary>
        ///     Check to see if Player One and Player Two have recently been tagged
        /// </summary>
        /// <param name="playerOne"></param>
        /// <param name="playerTwo"></param>
        /// <returns></returns>
        private bool PlayersRequireCooldown(Player playerOne, Player playerTwo)
        {
            long timeInMilliseconds = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);

            var (pOne, p1cooldown) = playerOne.cooldown;
            var (pTwo, p2cooldown) = playerTwo.cooldown;

            var p1OnTagTimer = timeInMilliseconds <= p1cooldown && pOne == playerTwo.client.Character.PlayerInfo.PlayerId;
            var p2OnTagTimer = timeInMilliseconds <= p2cooldown && pTwo == playerOne.client.Character.PlayerInfo.PlayerId;

            return (p1OnTagTimer || p2OnTagTimer || OnSelfCooldown(playerOne) || OnSelfCooldown(playerTwo));
        }

        /// <summary>
        ///     Update the specified player position
        /// </summary>
        /// <param name="e"></param>
        private Player UpdatePlayerPosition(IPlayerMovementEvent e)
        {
            int playerId = e.ClientPlayer.Character.PlayerInfo.PlayerId;
            if (!_players.ContainsKey(playerId))
                this.AddPlayerInstance(playerId, e.ClientPlayer);

            //Update timesMoved
            _players[playerId].timesMoved++;

            // Update player position
            Player player = _players[playerId];
            player.position = e.PlayerPosition;
            Logger.LogDebug($"Updated {e.PlayerControl.PlayerInfo.PlayerName}'s position to: {player.position.X},{player.position.Y}");
            return player;
        }

        /// <summary>
        ///     Add a player instance to this game mode
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="player"></param>
        private void AddPlayerInstance(int playerId, IClientPlayer player)
        {
            // Try and see if this client already exists
            if (_players.ContainsKey(playerId) && _players[playerId].playerName == player.Character.PlayerInfo.PlayerName)
                return;

            // Client doesn't exist, add them
            _players.TryAdd(playerId, new Player()
            {
                playerName = player.Character.PlayerInfo.PlayerName,
                client = player,
                isTagged = player.Character.PlayerInfo.IsImpostor,
                cooldown = (player.Character.PlayerInfo.PlayerId, (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + (Utils.roundStart * 1000)),
            });
        }

        /// <summary>
        ///     Set the new tagger
        /// </summary>
        /// <returns></returns>
        public async Task SelectNewTagger()
        {
            var regularPlayers = _players.Where((player) => !player.Value.isTagged);
            if (regularPlayers.Count() <= 0) return;

            // Update the tagger to be someone else
            var taggedPlayer = regularPlayers.ElementAt(new Random().Next(0, regularPlayers.Count() - 1));
            taggedPlayer.Value.isTagged = true;
            Logger.LogDebug($"{taggedPlayer.Value.playerName} is the new tagger!");
            await taggedPlayer.Value.client.Character.SetOutfitAsync(Utils.TaggedOutfit);
        }
    }
}
