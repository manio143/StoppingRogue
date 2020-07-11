using System;
using System.Collections.Generic;
using System.Text;

namespace StoppingRogue.Levels
{
    public enum ActionType
    {
        [ActionChar('-')]
        Nop,

        [ActionChar('W')]
        MoveUp,
        [ActionChar('S')]
        MoveDown,
        [ActionChar('D')]
        MoveRight,
        [ActionChar('A')]
        MoveLeft,

        [ActionChar('L')]
        ShootLazer,
        [ActionChar('F')]
        SwitchLight,
        [ActionChar('H')]
        GrabRelease,
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class ActionCharAttribute : Attribute
    {
        public ActionCharAttribute(char character)
        {
            Character = character;
        }

        public char Character { get; }
    }
}
