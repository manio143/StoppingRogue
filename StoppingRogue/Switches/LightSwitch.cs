using StoppingRogue.Tasks;
using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;
using System;
using System.ComponentModel;

namespace StoppingRogue.Switches
{
    [DefaultEntityComponentProcessor(typeof(LightSwitchProcessor))]
    [DataContract]
    public class LightSwitch : EntityComponent
    {
        [DefaultValue(false)]
        public bool Active
        {
            get => active;
            set
            {
                active = value;
                if (active)
                {
                    if (taskComponent.Type != TaskType.SwitchLightOn)
                        throw new InvalidOperationException();
                    taskComponent.Completed = true;

                    //TODO: change color
                }
            }
        }

        public TaskComponent taskComponent;
        private bool active;
    }
}
