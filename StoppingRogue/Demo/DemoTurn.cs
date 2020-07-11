using StoppingRogue.Turns;
using Stride.Core.Mathematics;
using Stride.Engine;
using System.Threading.Tasks;

namespace StoppingRogue.Demo
{
    public class DemoTurn : StartupScript
    {
        int turn;
        public override void Start()
        {
            Script.AddTask(Increment);
            Script.AddTask(Print);
        }

        private async Task Increment()
        {
            while(true)
            {
                await TurnSystem.NextTurn();
                turn = TurnSystem.TurnNumber;
            }
        }

        private async Task Print()
        {
            while(true)
            {
                await Script.NextFrame();
                DebugText.Print($"Current turn: {turn}", new Int2(5, 5));
            }
        }
    }
}
