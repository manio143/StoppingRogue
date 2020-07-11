using StoppingRogue.Levels;
using StoppingRogue.Turns;
using Stride.Engine;
using Stride.Input;
using System.Linq;
using System.Threading.Tasks;

namespace StoppingRogue.Input
{
    public class InputController : AsyncScript
    {
        public ActionType[] userActions;
        private Keys? downKey;
        public override async Task Execute()
        {
            this.Priority = 10;
            Script.AddTask(ProcessInput);
            while (true)
            {
                await TurnSystem.NextTurn();
                var action = GetAction();
                if(userActions.Contains(action.GetActionType()))
                    ActionController.Broadcast(action, user: true);
                downKey = null;
            }
        }

        private async Task ProcessInput()
        {
            while(true)
            {
                await Script.NextFrame();
                downKey = Input.HasDownKeys ? Input.DownKeys.First() : downKey;
                if (!userActions.Contains(GetAction().GetActionType()))
                {
                    // TODO: play mistake sound
                    downKey = null;
                }
            }
        }

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
