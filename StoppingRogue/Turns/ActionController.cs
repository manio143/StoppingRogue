using StoppingRogue.Robot;
using Stride.Engine;
using Stride.Engine.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StoppingRogue.Turns
{
    public class ActionController : AsyncScript
    {
        private static (bool user, Levels.Action action)? _action;
        private static object BroadcastLock = new object();
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

        public RobotController Robot;

        public override async Task Execute()
        {
            this.Priority = 50;
            while(true)
            {
                if (await TurnSystem.NextTurn())
                    continue;
                await Script.NextFrame();
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
