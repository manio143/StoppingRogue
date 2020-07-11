using Stride.Engine;
using Stride.Core.Mathematics;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using Stride.Physics;

namespace StoppingRogue.Robot
{
    public class RobotController : SyncScript
    {
        public async Task ExecuteAction(Levels.Action action)
        {
            Debug.WriteLine("Execute: {0}", action);
            switch (action)
            {
                case Levels.Action.Nop:
                    return;
                case Levels.Action.MoveUp:
                    await Move(1, 0);
                    return;
                case Levels.Action.MoveDown:
                    await Move(-1, 0);
                    return;
                case Levels.Action.MoveRight:
                    await Move(0, 1);
                    return;
                case Levels.Action.MoveLeft:
                    await Move(0, -1);
                    return;
                case Levels.Action.ShootLaser:
                    break; //TODO
                case Levels.Action.SwitchLight:
                    robotLight.Switch();
                    return;
                case Levels.Action.GrabRelease:
                    break; //TODO
                default:
                    break;
            }
        }

        private const double MovementDurationInSeconds = 0.3;
        private async Task Move(float y, float x)
        {
            robotLight.UpdateTransform(new Vector2(x, y));
            var offset = new Vector3(x, y, 0);
            var current = Entity.Transform.Position;
            var target = Entity.Transform.Position + offset;
            var currentTime = Game.UpdateTime.Total;
            var targetTime = currentTime + TimeSpan.FromSeconds(MovementDurationInSeconds);
            var advancement = 0.0;
            while(targetTime - currentTime > TimeSpan.FromSeconds(0.05))
            {
                await Script.NextFrame();
                var diff = Game.UpdateTime.Elapsed;
                currentTime += diff;
                var fraction = Math.Abs(diff.TotalSeconds / MovementDurationInSeconds);
                advancement = Math.Min(1, advancement + fraction);
                //Debug.WriteLine("F: {0}, A: {1}", fraction, advancement);
                Entity.Transform.Position = Interpolate(current, target, advancement);

                // If you colided with anything
                if(physics.Collisions.Count > 0)
                {
                    // TODO: check if the thing is a trigger!
                    Entity.Transform.Position = current;
                    return;
                }
            }
            Entity.Transform.Position = target;
        }

        private Vector3 Interpolate(Vector3 current, Vector3 target, double advancement)
        {
            var part = MathUtil.SmoothStep((float)advancement);
            return current + (part * (target - current));
        }

        public RobotController()
        {
            Priority = 10;
        }

        private RigidbodyComponent physics;
        private RobotLight robotLight;
        public override void Start()
        {
            physics = Entity.Get<RigidbodyComponent>();
            robotLight = Entity.Get<RobotLight>();
            robotLight.UpdateTransform(new Vector2(0, -1));
        }
        public override void Update() { }
    }
}
