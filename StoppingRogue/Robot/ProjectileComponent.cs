﻿using StoppingRogue.Destructable;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Physics;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StoppingRogue.Robot
{
    /// <summary>
    /// Describes the behaviour of a laser projectile.
    /// </summary>
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
                // Move
                Entity.Transform.Position = Entity.Transform.Position + speed * (Vector3)direction;
            }
        }

        /// <summary>
        /// One the first collision try to destroy the collider and die.
        /// </summary>
        private async Task CheckCollision()
        {
            var physics = Entity.Get<RigidbodyComponent>();
            var collider = await physics.NewCollision();

            // get collider
            var other = collider.ColliderA == physics ? collider.ColliderB.Entity : collider.ColliderA.Entity;
            var destructable = other.Get<DestructableComponent>();

            Debug.WriteLine($"Projectile collided with '{other.Name}'");

            if(destructable != null)
            {
                Debug.WriteLine($"Projectile destroyed '{other.Name}'");
                destructable.Destruct();
            }

            Entity.Scene = null;
        }
    }
}