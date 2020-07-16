using Stride.Audio;
using Stride.Core;
using Stride.Engine;
using System;

namespace StoppingRogue.Destructable
{
    /// <summary>
    /// Describes an entity that can be destroyed with a laser.
    /// </summary>
    [DataContract]
    public class DestructableComponent : EntityComponent
    {
        /// <summary>
        /// Called when the entity is hit with a laser.
        /// </summary>
        public event Action OnDestruct;

        public SoundInstance breakSound;

        /// <summary>
        /// Invokes <see cref="OnDestruct"/> event.
        /// </summary>
        public void Destruct()
        {
            breakSound?.Play();
            OnDestruct?.Invoke();
        }
    }
}
