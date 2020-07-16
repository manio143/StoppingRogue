using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Physics;
using Stride.Rendering.Sprites;
using System.Diagnostics;

namespace StoppingRogue.Switches
{
    /// <summary>
    /// Controls a door entity.
    /// </summary>
    public class DoorComponent : ScriptComponent
    {
        /// <summary>
        /// Open door - set sensor collision and opened sprite.
        /// </summary>
        public void Open()
        {
            Debug.WriteLine($"Open {Entity.Name}");

            var rb = Entity.Get<RigidbodyComponent>();
            rb.Enabled = false;

            var sprite = Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
            sprite.CurrentFrame = 13;

            //TODO door sound

        }

        /// <summary>
        /// Close door - set default collision and closed sprite.
        /// </summary>
        public void Close()
        {
            Debug.WriteLine($"Close {Entity.Name}");

            var rb = Entity.Get<RigidbodyComponent>();
            rb.Enabled = true;

            var sprite = Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
            sprite.CurrentFrame = 12;
            
            //TODO door sound
        }
    }
}
