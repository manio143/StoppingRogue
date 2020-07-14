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
        private bool pressed = false;
        public override async Task Execute()
        {
            var physics = Entity.Get<RigidbodyComponent>();
            var switchComp = Entity.Get<SwitchComponent>();

            while (true)
            {
                if(physics.Collisions.Count > 0)
                {
                    if(!pressed)
                    {
                        pressed = true;
                        switchComp.Switch();
                        // TODO switch sound
                    }
                }
                else
                {
                    if (pressed)
                    {
                        pressed = false;
                        switchComp.Switch();
                        // TODO switch sound
                    }
                }
                await Script.NextFrame();
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
