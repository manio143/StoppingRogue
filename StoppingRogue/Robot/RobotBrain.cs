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

        public int Cycles { get; private set; }
        public override async Task Execute()
        {
            this.Priority = 30;
            while (true)
            {
                NextAction = GetAction();
                if (await TurnSystem.NextTurn())
                    continue;
                await Script.NextFrame();

                var action = NextAction;
                if (action != Levels.Action.Nop)
                    ActionController.Broadcast(action, user: false);
                IncrementIndex();
            }
        }

        private static Random random = new Random();

        public Levels.Action NextAction { get; private set; }

        private Levels.Action GetAction()
        {
            var probability = GetProbability(Cycles);
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

        public void Reset()
        {
            Cycles = 0;
            index = 0;
        }

        private double GetProbability(int cycle)
        {
            cycle = Math.Min(10, cycle);
            return cycle * 0.04;
        }

        private void IncrementIndex()
        {
            index++;
            if (index >= actions.Length)
            {
                index = 0;
                Cycles++;
            }
        }
    }
}
