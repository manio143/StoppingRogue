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
        public static EventKey<Levels.Action> ActionEvent = new EventKey<Levels.Action>(eventName: "Action");

        private EventReceiver<Levels.Action> ActionReceiver = new EventReceiver<Levels.Action>(ActionEvent);

        public RobotController Robot;

        public override async Task Execute()
        {
            this.Priority = 50;
            while(true)
            {
                await TurnSystem.NextTurn();
                await Script.NextFrame();
                await Script.NextFrame();

                var action = await ActionReceiver.ReceiveAsync();
                if (Robot != null)
                    await Robot.ExecuteAction(action);
            }
        }
    }
}
