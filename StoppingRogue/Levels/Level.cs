using Stride.Core.Mathematics;
using System.Collections.Generic;

namespace StoppingRogue.Levels
{
    /// <summary>
    /// Describes a level (puzzle).
    /// </summary>
    public struct Level
    {
        /// <summary>
        /// Room width.
        /// </summary>
        public int Width;

        /// <summary>
        /// Room height.
        /// </summary>
        public int Height;

        /// <summary>
        /// Description of entities in the room.
        /// </summary>
        public TileType[,] Tiles;

        /// <summary>
        /// Pattern of actions executed by the <see cref="Robot.RobotBrain"/>.
        /// </summary>
        public Action[] ActionPattern;

        /// <summary>
        /// Actions available to the user.
        /// </summary>
        public ActionType[] UserActions;

        /// <summary>
        /// Connection between a switch and doors.
        /// </summary>
        public Dictionary<Int2,List<Int2>> SwitchMapping;
    }
}
