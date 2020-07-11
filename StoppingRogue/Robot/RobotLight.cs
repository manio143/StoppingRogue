using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Physics;
using Stride.Rendering.Sprites;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StoppingRogue.Robot
{
    public class RobotLight : StartupScript
    {
        public SpriteSheet robotSpriteSheet;
        private Entity lightEntity;
        private SpriteComponent lightSprite;
        private RigidbodyComponent lightPhysics;
        public override void Start()
        {
            lightEntity = new Entity();
            lightSprite = lightEntity.GetOrCreate<SpriteComponent>();
            lightSprite.SpriteProvider = new SpriteFromSheet()
            {
                Sheet = robotSpriteSheet,
                CurrentFrame = 1,
            };
            lightSprite.Enabled = false;

            lightPhysics = lightEntity.GetOrCreate<RigidbodyComponent>();
            lightPhysics.ColliderShape = new BoxColliderShape(true, new Vector3(0.3f, 0.4f, 0));
            lightPhysics.RigidBodyType = RigidBodyTypes.Kinematic;
            lightPhysics.CanCollideWith = CollisionFilterGroupFlags.CustomFilter1;
            lightPhysics.CollisionGroup = CollisionFilterGroups.CustomFilter1;
            lightPhysics.Enabled = false;

            Entity.AddChild(lightEntity);

            Script.AddTask(CheckLightCollision);
            Switch();
        }

        private async Task CheckLightCollision()
        {
            while(true)
            {
                await lightPhysics.NewCollision();
                Debug.WriteLine("Light colided!");
            }
        }

        public void Switch()
        {
            lightSprite.Enabled = !lightSprite.Enabled;
            lightPhysics.Enabled = !lightPhysics.Enabled;
        }

        public void UpdateTransform(Vector2 direction)
        {
            lightEntity.Transform.Position = (Vector3)direction / 2.0f;
            var rotationAngle = (float)(Math.Atan(direction.X / direction.Y) + Math.PI);
            if (direction == new Vector2(0, 1))
                rotationAngle = 0;
            lightEntity.Transform.RotationEulerXYZ = new Vector3(0, 0, rotationAngle);
        }
    }
}
