using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using System;
using System.Threading.Tasks;

namespace StoppingRogue.Robot
{
    public class RobotCameraFollower : AsyncScript
    {
        [DataMemberIgnore]
        public Entity Robot;

        private const float offsetX = 3;
        private const float offsetY = -1;
        public override async Task Execute()
        {
            while(true)
            {
                await Script.NextFrame();
                if (Robot == null)
                    continue;
                await FollowRobot();
                
            }
        }

        public async Task FollowRobot()
        {
            var speed = 3f;
            var currentX = Entity.Transform.Position.X;
            var currentY = Entity.Transform.Position.Y;
            var targetX = Robot.Transform.Position.X + offsetX;
            var targetY = Robot.Transform.Position.Y + offsetY;

            var advancement = 0.1f;
            while( Math.Abs(Entity.Transform.Position.X - targetX) > 0.04
                || Math.Abs(Entity.Transform.Position.Y - targetY) > 0.04)
            {
                var smooth = MathUtil.SmootherStep(advancement);
                Entity.Transform.Position.X = currentX + (targetX - currentX) * smooth;
                Entity.Transform.Position.Y = currentY + (targetY - currentY) * smooth;

                advancement = (float)Math.Min(1, advancement + Game.UpdateTime.Elapsed.TotalSeconds * speed);
                await Script.NextFrame();
                speed = (float)Math.Max(0.5, speed - 0.2);
            }
        }
    }
}
