using CrewNodePlugin.Games.TagGame;
using CrewNodePlugin.Manager;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Impostor.Api.Net;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using static CrewNodePlugin.Games.FreezeTagGame.FreezeTagUtils;
using static CrewNodePlugin.Games.TagGame.TagUtils;

namespace CrewNodePlugin.Games
{
    class FreezeTag : GameModeType
    {
        private ConcurrentDictionary<int, Player> _players = new ConcurrentDictionary<int, Player>();
        private CancellationTokenSource _cts;
        private int _originalPlayerCount = -1;
        private int _gameEnds = -1;
        private int _roundStarts = -1;
        public bool _roundStarted = false;

        // Configurables
        private const int _kickAfterSeconds = 20;       // Best: 20
        private const int _updatePlayerPosAfter = 5;    // Best: 5
        //private const int _minNumOfMovements = 8;     // Best: 8

        /// <summary>
        ///     Handle player movement for the Tag gamemode
        /// </summary>
        /// <param name="e"></param>
        public override async ValueTask HandlePlayerMovement(IPlayerMovementEvent e)
        {
            await base.HandlePlayerMovement(e);

            // Has the game even started?
            if (e.Game.GameState != GameStates.Started) return;

            Player movedPlayer = UpdatePlayerPosition(e);

            if (!this._roundStarted || _players.Count < 1) return;

            foreach (var player in _players)
            {
                if (!e.Game.Players.Any((p) => player.Value.client.Character.PlayerInfo.PlayerId == p.Character.PlayerInfo.PlayerId)) continue;
                if (movedPlayer.client.Character.PlayerInfo.PlayerId == player.Value.client.Character.PlayerInfo.PlayerId) continue;
                if (!OnlyOneTrue(movedPlayer.isTagged, movedPlayer.isFrozen, player.Value.isTagged, player.Value.isFrozen)) continue;
                if (Vector2.Distance(player.Value.position, movedPlayer.position) >= TagUtils.infectionRange) continue;
                if (OnSelfCooldown(player.Value) || OnSelfCooldown(movedPlayer)) continue;

                //Check if either of the players are tagged.
                if (movedPlayer.isTagged || player.Value.isTagged)
                {
                    movedPlayer.isFrozen = !movedPlayer.isTagged;
                    player.Value.isFrozen = !player.Value.isTagged;
                }
                else /*if (movedPlayer.isFrozen || player.Value.isFrozen)*/
                {
                    movedPlayer.isFrozen = false;
                    player.Value.isFrozen = false;
                }

                await movedPlayer.client.Character.SetOutfitAsync(movedPlayer.isTagged ? TagUtils.TaggedOutfit : movedPlayer.isFrozen ? TagUtils.FrozenOutfit : TagUtils.RegularOutfit);
                await player.Value.client.Character.SetOutfitAsync(player.Value.isTagged ? TagUtils.TaggedOutfit : player.Value.isFrozen ? TagUtils.FrozenOutfit : TagUtils.RegularOutfit);
            }
        }

        /// <summary>
        ///     Handle player disconnection
        /// </summary>
        /// <param name="e"></param>
        public override async ValueTask HandlePlayerDestroyed(IPlayerDestroyedEvent e)
        {
            if (e == null) return;
            await base.HandlePlayerDestroyed(e);

            // Has the game even started?
            if (e.Game.GameState != GameStates.Started) return;

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
            this._originalPlayerCount = e.Game.PlayerCount;

            // Make sure all of our players exist
            foreach (var player in e.Game.Players)
                this.AddPlayerInstance(player.Character.PlayerInfo.PlayerId, player.Client.Player);

            // Start the Tag gamemode in another thread
            this._cts = new CancellationTokenSource();
            this._gameEnds = TagUtils.gameEnd;
            this._roundStarts = TagUtils.roundStart;
            PluginUtils.RunTask(async () => await HandleTagInterval(e), 1, _cts.Token);
        }

        /// <summary>
        ///     Ran every second, to update the game state of the game mode
        /// </summary>
        private async ValueTask HandleTagInterval(IGameStartedEvent e)
        {
            // Player Name Updates
            string message = this._roundStarts != 0 ? $"Starting In {this._roundStarts--}" : $"Ends {TagUtils.SecondsToFormat(this._gameEnds--)}";

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
                else if (this._gameEnds == TagUtils.gameEnd && _roundStarts <= 0)
                {
                    await characterInfo.SetOutfitAsync(pValue.isTagged ? TagUtils.TaggedOutfit : TagUtils.RegularOutfit);
                    this._roundStarted = true;
                }

                if (this._roundStarts != 0 && characterInfo.PlayerInfo.ColorId != TagUtils.NeutralOutfit.Color)
                    await characterInfo.SetOutfitAsync(TagUtils.NeutralOutfit);
               
                // Update name
                await characterInfo.SetNameAsync($"{pValue.playerName} {message}");

                // Reset spawn locations
                if (this._roundStarts == TagUtils.roundStart - _updatePlayerPosAfter)
                {
                    var updatedPos = Extensions.GetSpawnLocation(characterInfo.PlayerInfo.PlayerId, this._originalPlayerCount);
                    pValue.position = updatedPos;
                    await characterInfo.NetworkTransform.SnapToAsync(updatedPos);
                }

                // Check for AFK
                if (this._gameEnds == (TagUtils.gameEnd - _kickAfterSeconds) && IsAFK(pValue, _originalPlayerCount) && !CrewNodePlugin.debug)
                {
                    // Makes sure to only pick new taggers when an afk tagger gets kicked...
                    if (pValue.isTagged)
                    {
                        Logger.LogDebug($"{pValue.playerName} was the tagger, but has now been kicked.");
                        await this.SelectNewTagger();
                        _players.Remove(player.Key, out _);
                    }

                    await pValue.client.KickAsync(); //else await characterInfo.SetOutfitAsync(TagUtils.CoolDownRegularOutfit);
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

        /// <summary>
        ///     Check if a player is on cooldown
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private static bool OnSelfCooldown(Player player)
        {
            long timeInMilliseconds = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
            var (id, cooldown) = player.cooldown;
            return timeInMilliseconds <= cooldown && id == player.client.Character.PlayerInfo.PlayerId;
        }

        /// <summary>
        ///     Handle a game that has ended
        /// </summary>
        /// <param name="e"></param>
        public override async ValueTask HandleGameEnded(IGameEndedEvent e)
        {
            Console.WriteLine("HANDLE GAME ENDED");
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
            //bool afkCheck2 = player.timesMoved < _minNumOfMovements;
            return afkCheck1 /*|| afkCheck2*/;
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

            if (player.isFrozen)
            {
                player.client.Character.NetworkTransform.SnapToAsync(player.position);
            }
            else
            {
                player.position = e.PlayerPosition;
            }

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
                cooldown = (player.Character.PlayerInfo.PlayerId, (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) + (TagUtils.roundStart * 1000)),
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
            taggedPlayer.Value.isFrozen = false;
            Logger.LogDebug($"{taggedPlayer.Value.client.Game.Code}: {taggedPlayer.Value.playerName} was set as the new tagger!");
            await taggedPlayer.Value.client.Character.SetOutfitAsync(TagUtils.TaggedOutfit);
        }
    }
}
