using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;
using Stride.Rendering.Sprites;
using System;
using System.ComponentModel;

namespace StoppingRogue.Tasks
{
    /// <summary>
    /// Light switch - when activated, completes the associated task.
    /// </summary>
    [DefaultEntityComponentProcessor(typeof(LightSwitchProcessor))]
    [DataContract]
    public class LightSwitch : EntityComponent
    {
        /// <summary>
        /// Switch state.
        /// </summary>
        [DefaultValue(false)]
        public bool Active
        {
            get => active;
            set
            {
                // cannot be turned off after it is on
                if (active) return;

                active = value;
                if (active)
                {
                    // validate and complete task
                    if (taskComponent.Type != TaskType.SwitchLightOn)
                        throw new InvalidOperationException();
                    taskComponent.Completed = true;

                    // change frame to signal activation
                    (Entity.GetParent().Get<SpriteComponent>().SpriteProvider as SpriteFromSheet).CurrentFrame = 26;
                }
            }
        }

        /// <summary>
        /// Associated task.
        /// </summary>
        public TaskComponent taskComponent;
        private bool active;
    }
}
