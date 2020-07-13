using Stride.Engine;
using Stride.Physics;
using Stride.Core.Mathematics;
using System.Threading.Tasks;
using StoppingRogue.Switches;
using StoppingRogue.Destructable;
using System.Diagnostics;
using StoppingRogue.Tasks;
using StoppingRogue.Items;

namespace StoppingRogue.Robot
{
    /// <summary>
    /// Allows grabbing items.
    /// </summary>
    public class RobotHolder : StartupScript
    {
        private PhysicsComponent physics;
        private Entity grabbedEntity;

        /// <summary>
        /// Is robot holding something?
        /// </summary>
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
                // Shoot a raycast one tile forward
                // CustomFilter2 reacts only with entities that have a parent
                // with an ItemComponent
                Entity.Transform.GetWorldTransformation(out var globalPos, out _, out _);
                var hit = physics.Simulation.Raycast(globalPos, globalPos + (Vector3)direction, CollisionFilterGroups.CustomFilter2);
                grabbedEntity = hit.Collider?.Entity.GetParent();

                if (grabbedEntity == null)
                    return;

                // remove the object from the scene
                Entity.Scene.Entities.Remove(grabbedEntity);
                // clone it to avoid physics system disaster
                var clone = grabbedEntity.Clone();

                // make it smaller, move to look as if held
                clone.Transform.Scale = grabbedEntity.Transform.Scale * ScaleFactor;
                clone.Transform.Position = new Vector3(0.2f, -0.2f, 0);
                // disable collisions
                var rigidBody = clone.Get<RigidbodyComponent>();
                rigidBody.Enabled = false;

                // attach to the robot
                Entity.AddChild(clone);

                grabbedEntity = clone;
            }
            else
            {
                // check if there's free space in front
                Entity.Transform.GetWorldTransformation(out var globalPos, out _, out _);
                var hit = physics.Simulation.Raycast(globalPos, globalPos + (Vector3)direction, CollisionFilterGroups.DefaultFilter);

                var slot = hit.Collider?.Entity.Get<SlotComponent>();
                // Don't allow releasing the item on something with a default colider
                // unless it's an item slot, which check for next
                if (hit.Collider?.Entity != null
                    && hit.Collider.Entity.Get<PressurePlate>() == null
                    && slot == null)
                    return;

                if(slot != null)
                {
                    var item = grabbedEntity.Get<ItemComponent>()?.ItemType;
                    if (item == null)
                        return; // this shouldn't happen but better safe than sorry

                    if (slot.taskComponent.Completed == false 
                        && slot.ItemType == item.Value)
                    {
                        slot.Fill(item.Value);
                        Entity.RemoveChild(grabbedEntity);
                        grabbedEntity = null;
                    }
                    return;
                }

                // it wasn't a slot, so let's place the item on the ground
                Entity.RemoveChild(grabbedEntity);
                // clone it to avoid physics system disaster
                grabbedEntity = grabbedEntity.Clone();

                // cloning cannot preserve events, so recreate destructable on wood box
                var destr = grabbedEntity.Get<DestructableComponent>();
                if (destr != null)
                {
                    var capture = grabbedEntity;
                    var expl = grabbedEntity.GetOrCreate<ExplosionComponent>();
                    destr.OnDestruct += expl.Explode;
                    destr.OnDestruct += () => Debug.WriteLine($"Entity '{capture.Name}' destroyed");
                    expl.PostExplosion += () => capture.Scene = null;
                }

                // roll back modifications
                grabbedEntity.Transform.Scale = grabbedEntity.Transform.Scale / ScaleFactor;
                grabbedEntity.Transform.Position = Entity.Transform.Position + (Vector3)direction;
                grabbedEntity.Get<RigidbodyComponent>().Enabled = true;

                // attach to the scene
                Entity.Scene.Entities.Add(grabbedEntity);

                grabbedEntity = null;
            }
        }
    }
}
