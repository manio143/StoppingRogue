using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;

namespace StoppingRogue.Tasks
{
    [DataContract]
    [DefaultEntityComponentProcessor(typeof(TaskProcessor))]
    public class TaskComponent : EntityComponent
    {
        public bool Completed { get; set; }
        public TaskType Type { get; set; }
    }

    public enum TaskType
    {
        FillBoxSlot,
        FillPipeSlot,
        SwitchLightOn,
        DestroyMainrfame,
    }
}
