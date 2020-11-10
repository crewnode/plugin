using CrewNodePlugin.Manager;
using CrewNodePlugin.Manager.Models;
using Impostor.Api.Games;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrewNodePlugin.Utils
{
    class DiscordUtils
    {
        public static void Setup()
        {
            GameManager.GetAllGames().OnDictionaryChanged += DiscordUtils_OnDictionaryChanged;
        }

        private static void DiscordUtils_OnDictionaryChanged(object sender, DictChangedEventArgs<string, CrewNodeGame> e)
        {
            // TODO
            Console.WriteLine("Games dictionary has been modified.");
        }

        public static void CreateChannel(IGame game, CrewNodeGame cnGame)
        {
            // TODO: Create Voice Channel from game code
        }

        public static void UpdateChannel(IGame game, CrewNodeGame cnGame, string type)
        {
            // TODO: Update Voice Channel for game code (mute / unmute / etc)
        }

        public static void RemoveChannel(IGame game, CrewNodeGame cnGame)
        {
            // TODO: Remove Voice Channel from server by game code
        }

    }
}
