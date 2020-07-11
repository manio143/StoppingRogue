using System;

namespace StoppingRogue.Levels
{
    public enum ActionType
    {
        Movement,
        Laser,
        Light,
        Hold,
    }
    public enum Action
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
        ShootLaser,
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
