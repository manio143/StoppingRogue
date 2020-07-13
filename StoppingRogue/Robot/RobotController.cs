using Stride.Engine;
using Stride.Core.Mathematics;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using Stride.Physics;
using System.Linq;
using Stride.Rendering.Sprites;

namespace StoppingRogue.Robot
{
    /// <summary>
    /// Controls robot's actions.
    /// </summary>
    public class RobotController : StartupScript
    {
        public Vector2 direction = new Vector2(0, -1); //down

        /// <summary>
        /// Execute an action on a robot. Called from <see cref="Turns.ActionController"/>.
        /// </summary>
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
                    if (robotHolder.IsHolding || robotLight.EnabledState)
                        return;
                    robotLaser.ShootLaser(direction);
                    return;
                case Levels.Action.SwitchLight:
                    robotLight.Switch();
                    return;
                case Levels.Action.GrabRelease:
                    robotHolder.GrabRelease(direction);
                    return;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// How long should you move for. This has to be shorter than one turn (<see cref="Turns.TurnSystem.TurnLength"/>)!
        /// </summary>
        private const double MovementDurationInSeconds = 0.3;
        /// <summary>
        /// Move in the direction (x,y).
        /// </summary>
        private async Task Move(float y, float x)
        {
            // Note: this should be a Manhattan distance
            direction = new Vector2(x, y);

            // Update things that depend on direction
            robotLight.UpdateTransform(direction);
            UpdateSprite(direction);

            var offset = new Vector3(x, y, 0);
            var current = Entity.Transform.Position;
            var target = Entity.Transform.Position + offset;

            var currentTime = Game.UpdateTime.Total;
            var targetTime = currentTime + TimeSpan.FromSeconds(MovementDurationInSeconds);

            var advancement = 0.0; // for smoothness

            while(targetTime - currentTime > TimeSpan.FromSeconds(0.05))
            {
                await Script.NextFrame();

                var diff = Game.UpdateTime.Elapsed;
                currentTime += diff;

                // advance by the fraction
                var fraction = Math.Abs(diff.TotalSeconds / MovementDurationInSeconds);
                advancement = Math.Min(1, advancement + fraction);

                // Debug.WriteLine("F: {0}, A: {1}", fraction, advancement);

                // Move
                Entity.Transform.Position = Interpolate(current, target, advancement);

                // If you colided with anything
                if(physics.Collisions.Count > 0)
                {
                    // Ignore doors and switches
                    if (physics.Collisions.All(c => c.ColliderA.CollisionGroup == CollisionFilterGroups.SensorTrigger || c.ColliderB.CollisionGroup == CollisionFilterGroups.SensorTrigger))
                        continue;

                    // Roll back
                    Entity.Transform.Position = current;
                    return;
                }
            }
            Entity.Transform.Position = target;
        }

        /// <summary>
        /// Smooth interpolation between two vectors.
        /// </summary>
        private Vector3 Interpolate(Vector3 current, Vector3 target, double advancement)
        {
            var part = MathUtil.SmoothStep((float)advancement);
            return current + (part * (target - current));
        }

        public RobotController()
        {
            this.Priority = 10; // has to run after other components
        }

        private RigidbodyComponent physics;
        private RobotLight robotLight;
        private RobotHolder robotHolder;
        private RobotLaser robotLaser;
        private SpriteFromSheet spriteProvider;
        public override void Start()
        {
            physics = Entity.Get<RigidbodyComponent>();
            robotLight = Entity.Get<RobotLight>();
            robotHolder = Entity.Get<RobotHolder>();
            robotLaser = Entity.Get<RobotLaser>();
            spriteProvider = Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
            
            robotLight.UpdateTransform(direction);
            UpdateSprite(direction);
        }

        public void UpdateSprite(Vector2 direction)
        {
            if(direction.X == 0)
            {
                if(direction.Y > 0) //up
                    spriteProvider.CurrentFrame = 2;
                else //down
                    spriteProvider.CurrentFrame = 0;
            }
            else
            {
                if (direction.X > 0) //right
                    spriteProvider.CurrentFrame = 4;
                else //left
                    spriteProvider.CurrentFrame = 3;
            }
        }
    }
}
