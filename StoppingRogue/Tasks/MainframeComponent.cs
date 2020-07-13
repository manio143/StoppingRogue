using StoppingRogue.Destructable;
using Stride.Core;
using Stride.Engine;
using Stride.Rendering.Sprites;
using System;

namespace StoppingRogue.Tasks
{
    /// <summary>
    /// Provides <see cref="Destroy"/> for a <see cref="DestructableComponent"/>.
    /// </summary>
    [DataContract]
    public class MainframeComponent : EntityComponent
    {
        /// <summary>
        /// Associated task.
        /// </summary>
        public TaskComponent taskComponent;

        /// <summary>
        /// Completes the task and changes the sprite.
        /// </summary>
        public void Destroy()
        {
            if (taskComponent.Type != TaskType.DestroyMainrfame)
                throw new InvalidOperationException();

            taskComponent.Completed = true;

            (Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet).CurrentFrame = 20;
            Entity.Remove<DestructableComponent>();
        }
    }
}
