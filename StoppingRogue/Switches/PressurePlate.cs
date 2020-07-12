using Stride.Engine;
using Stride.Physics;
using System.Threading.Tasks;

namespace StoppingRogue.Switches
{
    public class PressurePlate : AsyncScript
    {
        public override async Task Execute()
        {
            var physics = Entity.Get<RigidbodyComponent>();
            var switchComp = Entity.Get<SwitchComponent>();

            if(physics.Collisions.Count > 0)
            {
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
