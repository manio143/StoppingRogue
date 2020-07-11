using System;

namespace StoppingRogue.Levels
{
    public enum ActionType
    {
        Movement = 1,
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

    public static class ActionExtension
    {
        public static ActionType GetActionType(this Action action)
        {
            switch (action)
            {
                case Action.MoveUp:
                case Action.MoveDown:
                case Action.MoveRight:
                case Action.MoveLeft:
                    return ActionType.Movement;
                case Action.ShootLaser:
                    return ActionType.Laser;
                case Action.SwitchLight:
                    return ActionType.Light;
                case Action.GrabRelease:
                    return ActionType.Hold;
                default:
                    return 0;
            }
        }
    }
}
