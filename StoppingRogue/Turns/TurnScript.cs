using Stride.Engine;
using System;

namespace StoppingRogue.Turns
{
    /// <summary>
    /// Updates the <see cref="TurnSystem"/> and manages turns.
    /// </summary>
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
            if (diff > TimeSpan.FromSeconds(1))
                diff = TimeSpan.FromSeconds(1);
            if (diff > TurnSystem.TurnLength)
            {
                lastTurn = currentTime;
                TurnSystem.TurnNumber++;

                // wake up all that wait for a new turn
                while(TurnSystem.Channel.Balance < 0)
                    TurnSystem.Channel.Send(false);

                TurnSystem.RemainingTime -= diff;
            }
        }
    }
}
