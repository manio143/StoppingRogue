using StoppingRogue.Turns;
using Stride.Engine;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StoppingRogue.Demo
{
    /// <summary>
    /// Prints to Debug output every turn. Meant to show TurnSystem usage.
    /// </summary>
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
