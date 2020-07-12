using Stride.Core.Mathematics;
using System.Collections.Generic;

namespace StoppingRogue.Levels
{
    public struct Level
    {
        public int Width;
        public int Height;
        public TileType[,] Tiles;
        public Action[] ActionPattern;
        public ActionType[] UserActions;
        public Dictionary<Int2,(bool,Int2)> SwitchMapping;
    }
}
