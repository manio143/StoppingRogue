using StoppingRogue.Robot;
using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Physics;
using System;
using System.Threading.Tasks;
using Stride.Rendering.Sprites;

namespace StoppingRogue.Switches
{
    public class DoorComponent : ScriptComponent
    {
        public void Open()
        {
            var rb = Entity.Get<RigidbodyComponent>();
            rb.ColliderShapes.Clear();
            rb.ColliderShapes.Add(new BoxColliderShapeDesc()
            {
                Is2D = true,
                Size = new Vector3(0.45f, 0.70f, 0),
                LocalOffset = new Vector3(0, 0.35f, 0),
            });
            rb.CollisionGroup = CollisionFilterGroups.SensorTrigger;
            rb.CanCollideWith = CollisionFilterGroupFlags.DefaultFilter;

            var sprite = Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
            sprite.CurrentFrame = 13;

            //TODO door sound

        }
        public void Close()
        {
            var rb = Entity.Get<RigidbodyComponent>();
            rb.ColliderShapes.Clear();
            rb.ColliderShapes.Add(new BoxColliderShapeDesc()
            {
                Is2D = true,
                Size = new Vector3(0.45f, 0.45f, 0),
            });
            rb.CollisionGroup = CollisionFilterGroups.DefaultFilter;
            rb.CanCollideWith = CollisionFilterGroupFlags.DefaultFilter | CollisionFilterGroupFlags.SensorTrigger;

            var sprite = Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
            sprite.CurrentFrame = 12;
            
            //TODO door sound
        }
    }
}
