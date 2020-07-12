using StoppingRogue.Destructable;
using Stride.Core;
using Stride.Engine;
using Stride.Rendering.Sprites;
using System;

namespace StoppingRogue.Tasks
{
    [DataContract]
    public class MainframeComponent : EntityComponent
    {
        public TaskComponent taskComponent;

        public void Destroy()
        {
            if (taskComponent.Type != TaskType.DestroyMainrfame)
                throw new InvalidOperationException();

            taskComponent.Completed = true;

            //TODO replace my sprite and remove destructable component
            (Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet).CurrentFrame = 20;
            Entity.Remove<DestructableComponent>();
        }
    }
}
