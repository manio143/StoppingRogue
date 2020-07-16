using StoppingRogue.Turns;
using Stride.Engine;
using System;
using System.Threading.Tasks;

namespace StoppingRogue.Robot
{
    /// <summary>
    /// Follows the action pattern and some randomness to broadcast actions.
    /// </summary>
    public class RobotBrain : AsyncScript
    {
        /// <summary>
        /// Pattern of actions to execute in a loop.
        /// </summary>
        public Levels.Action[] actions;
        private int index;

        /// <summary>
        /// Number of cycles passed (one cycle = pattern length).
        /// </summary>
        public int Cycles { get; private set; }
        public override async Task Execute()
        {
            this.Priority = 30;
            while (true)
            {
                NextAction = GetAction();

                if (await TurnSystem.NextTurn())
                    continue;

                var action = NextAction;
                // Robot actions are more important than players
                // so if it's Nop, just don't broadcast
                if (action != Levels.Action.Nop)
                    ActionController.Broadcast(action, user: false);
            }
        }

        private static Random random = new Random();

        /// <summary>
        /// Robot selected action to be broadcast on the next turn.
        /// </summary>
        public Levels.Action NextAction { get; private set; }

        /// <summary>
        /// Gets next action (from pattern or random).
        /// </summary>
        /// <returns></returns>
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
                var action = actions[index];
                IncrementIndex(); // only increase index if not random
                return action;
            }
        }

        public void Reset()
        {
            Cycles = 0;
            index = 0;
        }

        /// <summary>
        /// The longer you play the bigger chance of a random action.
        /// </summary>
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
