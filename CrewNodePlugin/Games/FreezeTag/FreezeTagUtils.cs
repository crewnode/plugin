using Impostor.Api.Innersloth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrewNodePlugin.Games.FreezeTagGame
{
    public static class FreezeTagUtils
    {
        public static bool OnlyOneTrue(params bool[] bools) => bools.Sum((bool value) => value ? 1 : 0) == 1;

        //public static readonly GameOptionsData frozenOptions =
    }
}
