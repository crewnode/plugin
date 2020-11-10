using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Impostor.Api.Net;
using Impostor.Api.Net.Inner.Objects;

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
        public const int roundStart = 30;
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

    public static class Extensions
    {
        public async static ValueTask SetOutfitAsync(this IInnerPlayerControl playerControl, TagUtils.Outfit outfit)
        {
            await playerControl.SetColorAsync(outfit.Color);
            await playerControl.SetHatAsync(outfit.Hat);
            await playerControl.SetSkinAsync(outfit.Skin);
            await playerControl.SetPetAsync(outfit.Pet);
        }

        public static Vector2 GetSpawnLocation(int playerId, int numPlayer)
        {
            Vector2 up = new Vector2(0f, 1f);
            up = up.Rotate((playerId - 1) * (360f / numPlayer));
            up *= 1.55f;
            return new Vector2(-0.72f, 0.62f) + up + new Vector2(0f, 0.3636f);
        }

        public static Vector2 Rotate(this Vector2 self, float degrees)
        {
            float f = 0.0174532924f * degrees;
            float num = MathF.Cos(f);
            float num2 = MathF.Sin(f);
            return new Vector2(self.X * num - num2 * self.Y, self.X * num2 + num * self.Y);
        }
    }
}
