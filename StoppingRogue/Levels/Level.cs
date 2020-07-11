using System;
using System.Collections.Generic;
using System.Text;

namespace StoppingRogue.Levels
{
    public struct Level
    {
        public int Width;
        public int Height;
        public TileType[,] Tiles;
        public Action[] ActionPattern;
    }
}
