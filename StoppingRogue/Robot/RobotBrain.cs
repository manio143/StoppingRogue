using StoppingRogue.Levels;
using StoppingRogue.Turns;
using Stride.Engine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StoppingRogue.Robot
{
    public class RobotBrain : AsyncScript
    {
        public ActionType[] userActions;
        public Levels.Action[] actions;
        private int index;
        private int cycle;
        public override async Task Execute()
        {
            this.Priority = 30;
            while (true)
            {
                await TurnSystem.NextTurn();
                await Script.NextFrame();

                var action = GetAction();
                if (action != Levels.Action.Nop)
                    ActionController.Broadcast(action, user: false);
                IncrementIndex();
            }
        }

        private static Random random = new Random();
        private Levels.Action GetAction()
        {
            var probability = GetProbability(cycle);
            if (random.NextDouble() < probability)
            {
                var values = Enum.GetValues(typeof(Levels.Action));
                return (Levels.Action)random.Next(0, values.Length);
            }
            else
            {
                return actions[index];
            }
        }

        private double GetProbability(int cycle)
        {
            cycle = Math.Min(10, cycle);
            return cycle * 0.05;
        }

        private void IncrementIndex()
        {
            index++;
            if (index >= actions.Length)
            {
                index = 0;
                cycle++;
            }
        }
    }
}
