using Stride.Engine;
using Stride.Core.Mathematics;
using Stride.Physics;
using Stride.Rendering.Sprites;
using System.Diagnostics;
using SharpDX.XInput;
using System.Transactions;
using Stride.Audio;

namespace StoppingRogue.Switches
{
    /// <summary>
    /// Controls a door entity.
    /// </summary>
    public class DoorComponent : ScriptComponent
    {
        public SoundInstance doorSound;

        /// <summary>
        /// True if door is opened.
        /// </summary>
        public bool OpenedState { get; internal set; }

        public void OpenClose()
        {
            if (OpenedState)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Open door - set sensor collision and opened sprite.
        /// </summary>
        public void Open()
        {
            Debug.WriteLine($"Open {Entity.Name}");
            OpenedState = true;

            var rb = Entity.Get<RigidbodyComponent>();
            rb.Enabled = false;

            var sprite = Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
            sprite.CurrentFrame = 13;

            doorSound?.Play();

        }

        /// <summary>
        /// Close door - set default collision and closed sprite.
        /// </summary>
        public void Close()
        {
            Debug.WriteLine($"Close {Entity.Name}");
            OpenedState = false;

            var rb = Entity.Get<RigidbodyComponent>();
            rb.Enabled = true;

            var sprite = Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet;
            sprite.CurrentFrame = 12;
            
            doorSound?.Play();
        }
    }
}
