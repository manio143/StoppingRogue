using StoppingRogue.Destructable;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StoppingRogue.Robot
{
    public class ProjectileComponent : AsyncScript
    {
        public Vector2 direction;
        public const float speed = 0.05f;
        public override async Task Execute()
        {
            Script.AddTask(CheckCollision);
            while(true)
            {
                await Script.NextFrame();
                Entity.Transform.Position = Entity.Transform.Position + speed * (Vector3)direction;
            }
        }

        private async Task CheckCollision()
        {
            var physics = Entity.Get<RigidbodyComponent>();
            var collider = await physics.NewCollision();

            var other = collider.ColliderA == physics ? collider.ColliderB.Entity : collider.ColliderA.Entity;
            var destructable = other.Get<DestructableComponent>();

            Debug.WriteLine($"Projectile collided with '{other.Name}'");

            if(destructable != null)
            {
                destructable.Destruct();
            }

            Entity.Scene = null;
        }
    }
}