using StoppingRogue.Robot;
using Stride.Engine;
using System.Threading.Tasks;

namespace StoppingRogue.Turns
{
    /// <summary>
    /// Receives actions from <see cref="Input.InputController"/> and <see cref="RobotBrain"/>.
    /// </summary>
    public class ActionController : AsyncScript
    {
        private static (bool user, Levels.Action action)? _action;
        private static object BroadcastLock = new object();

        /// <summary>
        /// Broadcast an action. Robot's actions have priority over user actions.
        /// </summary>
        public static void Broadcast(Levels.Action action, bool user)
        {
            lock(BroadcastLock)
            {
                if (_action == null)
                    _action = (user, action);
                else if(_action.Value.user && !user)
                    _action = (user, action);
            }
        }

        /// <summary>
        /// Robot to be controlled.
        /// </summary>
        public RobotController Robot;

        public override async Task Execute()
        {
            while(true)
            {
                if (await TurnSystem.NextTurn())
                    continue;

                // Wait one frame before executing the action to make sure
                // actions have been broadcasted
                await Script.NextFrame();

                Levels.Action action;
                lock (BroadcastLock)
                {
                    action = _action.HasValue ? _action.Value.action : Levels.Action.Nop;
                    _action = null;
                }
                if (Robot != null)
                    await Robot.ExecuteAction(action);
            }
        }
    }
}
