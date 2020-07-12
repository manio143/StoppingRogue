using StoppingRogue.Robot;
using Stride.Engine;
using Stride.Graphics;
using Stride.Core.Mathematics;
using Stride.Physics;
using System;
using System.Threading.Tasks;
using Stride.Rendering.Sprites;

namespace StoppingRogue.Switches
{
    public class DoorComponent : AsyncScript
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
        }

        public override async Task Execute()
        {
            var physics = Entity.Get<RigidbodyComponent>();
            while(true)
            {
                var collision = await physics.NewCollision();

                if (collision.ColliderA.CollisionGroup != CollisionFilterGroups.SensorTrigger
                    && collision.ColliderB.CollisionGroup != CollisionFilterGroups.SensorTrigger)
                    continue;

                var other = collision.ColliderA == physics ? collision.ColliderB.Entity : collision.ColliderA.Entity;

                var robotCtrl = other.Get<RobotController>();
                if (robotCtrl == null)
                    continue;

                var offset = robotCtrl.direction.Y;
                if (offset > 0) offset *= 2;
                else offset -= 0.5f;
                var targetY = (float)Math.Round(other.Transform.Position.Y + offset);
                while (robotCtrl.IsMoving)
                    await Script.NextFrame();

                other.Transform.Position.Y = targetY;
            }
        }
    }
}
