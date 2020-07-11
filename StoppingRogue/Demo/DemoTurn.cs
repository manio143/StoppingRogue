using StoppingRogue.Turns;
using Stride.Core.Mathematics;
using Stride.Engine;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StoppingRogue.Demo
{
    public class DemoTurn : StartupScript
    {
        int turn;
        public override void Start()
        {
            Script.AddTask(Increment);
        }

        private async Task Increment()
        {
            while (true)
            {
                await TurnSystem.NextTurn();
                turn = TurnSystem.TurnNumber;
                Debug.WriteLine($"Current turn: {turn}");
            }
        }
    }
}
