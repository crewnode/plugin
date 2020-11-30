using System;
using System.Numerics;
using System.Threading.Tasks;
using Impostor.Api.Net.Inner.Objects;

namespace CrewNodePlugin.Games.TagGame
{
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
