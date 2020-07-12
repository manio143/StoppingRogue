using Stride.Engine;
using Stride.Physics;
using Stride.Core.Mathematics;
using System.Threading.Tasks;
using StoppingRogue.Switches;
using StoppingRogue.Destructable;
using System.Diagnostics;

namespace StoppingRogue.Robot
{
    public class RobotHolder : StartupScript
    {
        private PhysicsComponent physics;
        private Entity grabbedEntity;

        public bool IsHolding => grabbedEntity != null;

        public override void Start()
        {
            physics = Entity.Get<RigidbodyComponent>();
        }

        private const float ScaleFactor = 0.3f;

        public void GrabRelease(Vector2 direction)
        {
            if(grabbedEntity == null)
            {
                Entity.Transform.GetWorldTransformation(out var globalPos, out _, out _);
                var hit = physics.Simulation.Raycast(globalPos, globalPos + (Vector3)direction, CollisionFilterGroups.CustomFilter2);
                grabbedEntity = hit.Collider?.Entity.GetParent();

                if (grabbedEntity == null)
                    return;

                Entity.Scene.Entities.Remove(grabbedEntity);
                var clone = grabbedEntity.Clone();

                clone.Transform.Scale = grabbedEntity.Transform.Scale * ScaleFactor;
                clone.Transform.Position = new Vector3(0.2f, -0.2f, 0);
                var rigidBody = clone.Get<RigidbodyComponent>();
                rigidBody.Enabled = false;

                Entity.AddChild(clone);

                grabbedEntity = clone;
            }
            else
            {
                Entity.Transform.GetWorldTransformation(out var globalPos, out _, out _);
                var hit = physics.Simulation.Raycast(globalPos, globalPos + (Vector3)direction, CollisionFilterGroups.DefaultFilter);
                // Don't allow releasing the item on something else
                if (hit.Collider?.Entity != null && hit.Collider.Entity.Get<PressurePlate>() == null)
                    return;

                Entity.RemoveChild(grabbedEntity);
                grabbedEntity = grabbedEntity.Clone();

                var destr = grabbedEntity.Get<DestructableComponent>();
                if (destr != null)
                {
                    var capture = grabbedEntity;
                    var expl = grabbedEntity.GetOrCreate<ExplosionComponent>();
                    destr.OnDestruct += expl.Explode;
                    destr.OnDestruct += () => Debug.WriteLine($"Entity '{capture.Name}' destroyed");
                    expl.PostExplosion += () => capture.Scene = null;
                }

                Entity.Scene.Entities.Add(grabbedEntity);
                grabbedEntity.Transform.Scale = grabbedEntity.Transform.Scale / ScaleFactor;
                grabbedEntity.Transform.Position = Entity.Transform.Position + (Vector3)direction;
                grabbedEntity.Get<RigidbodyComponent>().Enabled = true;

                grabbedEntity = null;
            }
        }
    }
}
