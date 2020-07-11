using System;
using System.Collections.Generic;
using System.Text;

namespace StoppingRogue.Levels
{
    public enum TileType
    {
        /// <summary>
        /// No tile, i.e. empty space outside
        /// </summary>
        [TileChar('-')]
        None = 0,

        [TileChar(' ')]
        Floor,

        [TileChar('w')]
        WallLower,
        [TileChar('e')]
        WallLowerFancy,
        [TileChar('W')]
        WallUpper,
        [TileChar('R')]
        RightFacingWall,
        [TileChar('L')]
        LeftFacingWall,
        [TileChar('!')]
        WallEdgeUL,
        [TileChar('@')]
        WallEdgeUR,
        [TileChar('#')]
        WallEdgeLL,
        [TileChar('$')]
        WallEdgeLR,
        [TileChar(';')]
        BackFacingWall,

        [TileChar('D')]
        Door,
        [TileChar('d')]
        OpenedDoor,

        [TileChar('T')]
        PressurePlate,
        [TileChar('J')]
        PressurePlateWithBox,
        [TileChar('F')]
        StepOnSwitch,

        [TileChar('Y')]
        SlotForBox,
        [TileChar('U')]
        SlotForPipe,

        [TileChar('l')]
        LightSwitchWall,

        [TileChar('M')]
        Mainframe,
        [TileChar('c')]
        Counter,
        [TileChar('x')]
        CounterEdgeLeft,
        [TileChar('X')]
        CounterEdgeRight,
        [TileChar('K')]
        CounterVerticalLeft,
        [TileChar('k')]
        CounterVerticalRight,

        [TileChar('V')]
        WoodCrate,
        [TileChar('v')]
        WoodBox,
        [TileChar('B')]
        MetalBox,
        [TileChar('P')]
        LongPipe,
        [TileChar('p')]
        LongPipeVertical,
        [TileChar('Q')]
        CutPipe,

        [TileChar('G')]
        GlassPane,

        [TileChar('H')]
        HoleInFloor,

        [TileChar('r')]
        Robot,
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class TileCharAttribute : Attribute
    {
        public TileCharAttribute(char character)
        {
            Character = character;
        }

        public char Character { get; }
    }
}
