using StoppingRogue.Turns;
using Stride.Engine;
using Stride.Input;
using System.Linq;
using System.Threading.Tasks;

namespace StoppingRogue.Input
{
    public class InputController : AsyncScript
    {
        //TODO limit actions
        private Keys? downKey;
        public override async Task Execute()
        {
            Script.AddTask(ProcessInput);
            while (true)
            {
                await TurnSystem.NextTurn();
                BroadCastAction();
                downKey = null;
            }
        }

        private async Task ProcessInput()
        {
            while(true)
            {
                await Script.NextFrame();
                downKey = Input.HasDownKeys ? Input.DownKeys.First() : downKey;
            }
        }

        private void BroadCastAction()
        {
            switch (downKey)
            {
                case Keys.W:
                    ActionController.ActionEvent.Broadcast(Levels.Action.MoveUp);
                    break;
                case Keys.S:
                    ActionController.ActionEvent.Broadcast(Levels.Action.MoveDown);
                    break;
                case Keys.A:
                    ActionController.ActionEvent.Broadcast(Levels.Action.MoveLeft);
                    break;
                case Keys.D:
                    ActionController.ActionEvent.Broadcast(Levels.Action.MoveRight);
                    break;
                default:
                    ActionController.ActionEvent.Broadcast(Levels.Action.Nop);
                    break;
            }
        }
    }
}
