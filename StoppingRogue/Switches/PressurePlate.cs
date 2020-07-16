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

            pressed = physics.Collisions.Count > 0;

            while (true)
            {
                await Script.NextFrame();
                if(physics.Collisions.Count > 0)
                {
                    if(!pressed)
                    {
                        pressed = true;
                        switchComp.Switch();
                    }
                }
                else
                {
                    if (pressed)
                    {
                        pressed = false;
                        switchComp.Switch();
                    }
                }
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
        }
    }
}
