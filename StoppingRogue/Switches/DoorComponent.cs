using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Physics;
using Stride.Rendering.Sprites;

namespace StoppingRogue.Switches
{
    /// <summary>
    /// Controls a door entity.
    /// </summary>
    public class DoorComponent : ScriptComponent
    {
        /// <summary>
        /// Open door - set sensor collision and opened sprite.
        /// </summary>
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

        /// <summary>
        /// Close door - set default collision and closed sprite.
        /// </summary>
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
