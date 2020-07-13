using Stride.Engine;
using Stride.Engine.Events;
using Stride.Games;
using System.Linq;

namespace StoppingRogue.Tasks
{
    /// <summary>
    /// Watches tasks states and broadcasts a signal when all are completed.
    /// </summary>
    public class TaskProcessor : EntityProcessor<TaskComponent>
    {
        public static EventKey AllTasksCompleted =
            new EventKey(eventName: "AllTasksCompleted");

        private bool completed;

        public override void Update(GameTime time)
        {
            if (ComponentDatas.Count == 0)
                completed = false;
            else
            {
                if (ComponentDatas.All(kvp => kvp.Key.Completed))
                {
                    if (!completed)
                        AllTasksCompleted.Broadcast();
                    completed = true;
                }
            }
        }
    }
}