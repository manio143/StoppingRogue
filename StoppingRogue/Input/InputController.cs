using StoppingRogue.Levels;
using StoppingRogue.Turns;
using Stride.Audio;
using Stride.Engine;
using Stride.Input;
using System.Linq;
using System.Threading.Tasks;

namespace StoppingRogue.Input
{
    /// <summary>
    /// Receives user input and broadcasts actions to <see cref="ActionController"/>.
    /// </summary>
    public class InputController : AsyncScript
    {
        /// <summary>
        /// Available actions. Always includes (<see cref="ActionType"/>)0.
        /// </summary>
        public ActionType[] userActions;

        /// <summary>
        /// User selected action to be broadcast on the next turn.
        /// </summary>
        public Action NextAction { get; private set; }

        /// <summary>
        /// Played when player tries an unavailable action.
        /// </summary>
        public Sound MistakeSound { get; set; }

        /// <summary>
        /// Currently pressed key.
        /// </summary>
        private Keys? downKey;

        public override async Task Execute()
        {
            Script.AddTask(ProcessInput);
            while (true)
            {
                if (await TurnSystem.NextTurn())
                {
                    Reset();
                    continue;
                }

                var action = NextAction;

                // Check if the action is allowed before broadcasting it.
                if(userActions.Contains(action.GetActionType()))
                    ActionController.Broadcast(action, user: true);

                Reset();
            }
        }

        private void Reset()
        {
            downKey = null;
            NextAction = Action.Nop;
        }

        private async Task ProcessInput()
        {
            var mistake = MistakeSound?.CreateInstance();
            while (true)
            {
                await Script.NextFrame();

                // Read new input, if there's none, preserve previous value.
                downKey = Input.HasDownKeys ? Input.DownKeys.First() : downKey;
                
                if (!userActions.Contains(GetAction().GetActionType()))
                {
                    mistake?.Play();
                    downKey = null;
                }
                
                NextAction = GetAction();
            }
        }

        /// <summary>
        /// Converts pressed <see cref="Keys"/> to <see cref="Action"/> according to
        /// the action's <see cref="ActionCharAttribute"/>.
        /// </summary>
        /// <returns></returns>
        private Action GetAction()
        {
            switch (downKey)
            {
                case Keys.W:
                    return Action.MoveUp;
                case Keys.S:
                    return Action.MoveDown;
                case Keys.A:
                    return Action.MoveLeft;
                case Keys.D:
                    return Action.MoveRight;
                case Keys.F:
                    return Action.SwitchLight;
                case Keys.L:
                    return Action.ShootLaser;
                case Keys.H:
                    return Action.GrabRelease;
                default:
                    return Action.Nop;
            }
        }
    }
}
