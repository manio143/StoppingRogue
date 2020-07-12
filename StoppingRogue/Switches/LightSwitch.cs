using StoppingRogue.Tasks;
using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.Rendering.Sprites;
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
                if (active) return;
                active = value;
                if (active)
                {
                    if (taskComponent.Type != TaskType.SwitchLightOn)
                        throw new InvalidOperationException();
                    taskComponent.Completed = true;

                    (Entity.GetParent().Get<SpriteComponent>().SpriteProvider as SpriteFromSheet).CurrentFrame = 26;
                }
            }
        }

        public TaskComponent taskComponent;
        private bool active;
    }
}
