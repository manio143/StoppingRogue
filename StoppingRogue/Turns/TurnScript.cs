using Stride.Engine;
using System;

namespace StoppingRogue.Turns
{
    public class TurnScript : SyncScript
    {
        private TimeSpan lastTurn;

        public bool Enabled { get; set; }
        public override void Update()
        {
            if (!Enabled)
                return;

            var currentTime = Game.UpdateTime.Total;
            var diff = currentTime - lastTurn;
            if (diff > TurnSystem.TurnLength)
            {
                lastTurn = currentTime;
                TurnSystem.TurnNumber++;
                TurnSystem.Channel.Send(false);
                TurnSystem.RemainingTime -= diff;
            }
        }
    }
}
