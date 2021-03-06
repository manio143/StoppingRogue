﻿using Stride.Audio;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Physics;
using Stride.Rendering.Sprites;
using System;

namespace StoppingRogue.Robot
{
    /// <summary>
    /// Allows shooting laser
    /// </summary>
    [DataContract]
    public class RobotLaser : EntityComponent
    {
        /// <summary>
        /// Sprite sheet for the laser sprite.
        /// </summary>
        public SpriteSheet itemSpriteSheet;

        public SoundInstance laserSound;

        /// <summary>
        /// Create a projectile that moves in <paramref name="direction"/>.
        /// </summary>
        public void ShootLaser(Vector2 direction)
        {
            var projectile = new Entity();
            
            var sprite = projectile.GetOrCreate<SpriteComponent>();
            sprite.SpriteProvider = new SpriteFromSheet()
            {
                Sheet = itemSpriteSheet,
                CurrentFrame = 7,
            };
            
            var rigidBody = projectile.GetOrCreate<RigidbodyComponent>();
            rigidBody.RigidBodyType = RigidBodyTypes.Kinematic;
            rigidBody.ColliderShapes.Add(new BoxColliderShapeDesc
            {
                Is2D = true,
                Size = new Vector3(0.2f, 0.2f, 0),
            });
            rigidBody.CollisionGroup = CollisionFilterGroups.DefaultFilter;
            // don't collide with sensors and others
            rigidBody.CanCollideWith = CollisionFilterGroupFlags.DefaultFilter;

            var logic = projectile.GetOrCreate<ProjectileComponent>();
            logic.direction = direction;

            var rotationAngle = (float)(Math.Atan(direction.X / direction.Y));
            projectile.Transform.RotationEulerXYZ = new Vector3(0, 0, rotationAngle);

            Entity.AddChild(projectile);

            laserSound?.Play();
        }
    }
}
