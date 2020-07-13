using Stride.Engine;
using Stride.Physics;
using System.Threading.Tasks;

namespace StoppingRogue.Switches
{
    /// <summary>
    /// A switch on the ground.
    /// </summary>
    public class PressurePlate : AsyncScript
    {
        public override async Task Execute()
        {
            var physics = Entity.Get<RigidbodyComponent>();
            var switchComp = Entity.Get<SwitchComponent>();

            if(physics.Collisions.Count > 0)
            {
                // it was initiated with something on it (i.e. Box on PressurePlate)
                await physics.CollisionEnded();
                switchComp.Switch();
                // TODO switch sound
            }

            while (true)
            {
                await physics.NewCollision();
                switchComp.Switch();
                // TODO switch sound

                await physics.CollisionEnded();
                switchComp.Switch();
                // TODO switch sound
            }
        }
    }

    /// <summary>
    /// A switch on the ground, which is activated once, for ever.
    /// </summary>
    public class StepOnSwitch : AsyncScript
    {
        public override async Task Execute()
        {
            var physics = Entity.Get<RigidbodyComponent>();
            var switchComp = Entity.Get<SwitchComponent>();

            await physics.NewCollision();
            switchComp.Switch();
            // TODO switch sound
        }
    }
}
