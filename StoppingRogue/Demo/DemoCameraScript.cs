using Stride.Engine;
using Stride.Core.Mathematics;

namespace StoppingRogue.Demo
{
    public class DemoCameraScript : SyncScript
    {
        public override void Update()
        {
            if (Input.IsKeyDown(Stride.Input.Keys.Right))
                Entity.Transform.Position += new Vector3(0.1f, 0, 0);
            if (Input.IsKeyDown(Stride.Input.Keys.Left))
                Entity.Transform.Position += new Vector3(-0.1f, 0, 0);
            if (Input.IsKeyDown(Stride.Input.Keys.Up))
                Entity.Transform.Position += new Vector3(0, 0.1f, 0);
            if (Input.IsKeyDown(Stride.Input.Keys.Down))
                Entity.Transform.Position += new Vector3(0, -0.1f, 0);
        }
    }
}
