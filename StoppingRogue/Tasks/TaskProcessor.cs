using Stride.Engine;
using Stride.Engine.Events;
using Stride.Games;
using System.Linq;

namespace StoppingRogue.Tasks
{
    public class TaskProcessor : EntityProcessor<TaskComponent>
    {
        public static EventKey AllTasksCompleted = new EventKey(eventName: "AllTasksCompleted");

        private bool completed;

        public void Reset()
        {
            completed = false;
        }

        public override void Update(GameTime time)
        {
            if (ComponentDatas.Count == 0)
                Reset();
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