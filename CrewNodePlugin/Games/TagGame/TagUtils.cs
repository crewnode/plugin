using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using Impostor.Api.Net;

namespace CrewNodePlugin.Games.TagGame
{
    public class TagUtils
    {
        /// <summary>
        ///     The constants set below are hard-set values that
        ///     are used for reference by an actual game entity
        /// </summary>
        public const float infectionRange = 0.5f;
        public const long tagTimer = 3000;
        public const int roundStart = 10;
        public const int gameEnd = 600;

        /// <summary>
        ///     The outfit for the player who is "Tagged"
        /// </summary>
        public static readonly Outfit TaggedOutfit = new Outfit
        {
            Color = 0x06,
            Hat = 72,
            Skin = 9,
            Pet = 0,
        };

        /// <summary>
        ///     The outfit for players who are not "Tagged"
        /// </summary>
        public static readonly Outfit RegularOutfit = new Outfit
        {
            Color = 0x07,
            Hat = 30,
            Skin = 0,
            Pet = 0,
        };

        public static readonly Outfit FrozenOutfit = new Outfit
        {
            Color = 0x021,
            Hat = 5,
            Skin = 5,
            Pet = 5,
        };

        /// <summary>
        ///     The outfit for players who neutral
        /// </summary>
        public static readonly Outfit NeutralOutfit = new Outfit
        {
            Color = 0x05,
            Hat = 32,
            Skin = 0,
            Pet = 0,
        };

        /// <summary>
        ///     Used for debugging, outfit shows players who should've been kicked
        /// </summary>
        public static readonly Outfit CooldownTaggedOutfit = new Outfit
        {
            Color = 0x00,
            Hat = 72,
            Skin = 9,
            Pet = 0,
        };

        /// <summary>
        ///     Used for debugging, outfit shows players who should've been kicked
        /// </summary>
        public static readonly Outfit CoolDownRegularOutfit = new Outfit
        {
            Color = 0x00,
            Hat = 30,
            Skin = 0,
            Pet = 0,
        };

        /// <summary>
        ///     Format X seconds to "Xm, Ys"
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns>A formatted time string</returns>
        public static string SecondsToFormat(int seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return seconds > 60
                    ? string.Format("{0:D1}m {1:D2}s", t.Minutes, t.Seconds)
                    : string.Format("{0:D2} seconds", t.Seconds);
        }


        /// <summary>
        ///     Helper Class to set a player's outfit
        /// </summary>
        public class Outfit
        {
            public byte Color;
            public uint Hat;
            public uint Skin;
            public uint Pet;
        }

        /// <summary>
        ///     Helper Class to record a player state
        /// </summary>
        public class Player
        {
            public string playerName;
            public bool isTagged;
            public bool isFrozen;
            public IClientPlayer client;
            public (byte, long) cooldown;
            public Vector2 position;
            public int timesMoved = 0;

            public Outfit Outfit => new Outfit
            {
                Color = client.Character.PlayerInfo.ColorId,
                Hat = client.Character.PlayerInfo.HatId,
                Skin = client.Character.PlayerInfo.SkinId,
                Pet = client.Character.PlayerInfo.PetId
            };
        }
    }
}
