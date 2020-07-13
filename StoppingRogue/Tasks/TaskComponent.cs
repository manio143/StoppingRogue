using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;

namespace StoppingRogue.Tasks
{
    /// <summary>
    /// A Task that can be completed.
    /// </summary>
    [DataContract]
    [DefaultEntityComponentProcessor(typeof(TaskProcessor))]
    public class TaskComponent : EntityComponent
    {
        /// <summary>
        /// Has the task been completed?
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// What kind of task is this?
        /// </summary>
        public TaskType Type { get; set; }
    }

    /// <summary>
    /// Possible tasks.
    /// </summary>
    public enum TaskType
    {
        FillBoxSlot,
        FillPipeSlot,
        SwitchLightOn,
        DestroyMainrfame,
    }
}
