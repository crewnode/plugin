using CrewNodePlugin.Games.TagGame;
using CrewNodePlugin.Manager;
using CrewNodePlugin.Manager.Models;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
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
        private const int _kickAfterSeconds = 5; // Best: 30
        private const int _updatePlayerPosAfter = 10; // Best: 5

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
            await base.HandleGameStarting(e);
            e.Game.Options.NumEmergencyMeetings = 0;
            e.Game.Options.ImpostorLightMod = .25f;
            e.Game.Options.CrewLightMod = .25f;
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
            this._roundStarts = Utils.roundStart;
            PluginUtils.RunTask(async () => { await HandleTagInterval(e); }, 1, _cts.Token);
        }

        /// <summary>
        ///     Ran every second, to update the game state of the game mode
        /// </summary>
        private async ValueTask HandleTagInterval(IGameStartedEvent e)
        {
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

                // Check if a player is in cooldown period
                if (this._roundStarts != 0) {
                    if (characterInfo.PlayerInfo.ColorId != Utils.NeutralOutfit.Color)
                        await characterInfo.SetOutfitAsync(Utils.NeutralOutfit);
                }
                else
                {
                    if (PlayerIsInCooldown(player.Value) && characterInfo.PlayerInfo.ColorId != Utils.ShouldBeKickedOutfitTagged.Color)
                        await characterInfo.SetOutfitAsync(pValue.isTagged ? Utils.ShouldBeKickedOutfitTagged : Utils.ShouldBeKickedOutfitRegular);
                    else
                    {
                        // We don't want to send OutfitAsync every interval, so we'll compare colours
                        if (pValue.isTagged && characterInfo.PlayerInfo.ColorId != Utils.TaggedOutfit.Color)
                            await characterInfo.SetOutfitAsync(Utils.TaggedOutfit);
                        else if (!pValue.isTagged && characterInfo.PlayerInfo.ColorId != Utils.RegularOutfit.Color)
                            await characterInfo.SetOutfitAsync(Utils.RegularOutfit);
                    }
                }

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
                // [TODO] Artic: Fix it
                if (
                    this._gameEnds == (Utils.gameEnd - Tag._kickAfterSeconds) &&
                    pValue.position == Extensions.GetSpawnLocation(
                        characterInfo.PlayerInfo.PlayerId,
                        this._originalPlayerCount
                    ))
                {
                    // Makes sure to only pick new taggers when an afk tagger gets kicked...
                    if (pValue.isTagged)
                    {
                        _logger.LogDebug($"{pValue.playerName} was the tagger, but has now been kicked.");
                        await this.SelectNewTagger();
                        if (!CrewNodePlugin.debug)
                            _players.TryRemove(player.Key, out _);
                    }

                    if (!CrewNodePlugin.debug) await pValue.client.KickAsync(); else await characterInfo.SetOutfitAsync(Utils.ShouldBeKickedOutfitRegular);
                    _logger.LogWarning($"{pValue.playerName} kicked");
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

            return (p1OnTagTimer || p2OnTagTimer);
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

            // Update player position
            Player player = _players[playerId];
            player.position = e.PlayerPosition;
            _logger.LogDebug($"Updated {e.PlayerControl.PlayerInfo.PlayerName}'s position to: {player.position.X},{player.position.Y}");
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
            _logger.LogDebug($"{taggedPlayer.Value.playerName} is the new tagger!");
            await taggedPlayer.Value.client.Character.SetOutfitAsync(Utils.TaggedOutfit);
        }
    }
}
