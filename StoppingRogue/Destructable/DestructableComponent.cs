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

        /// <summary>
        /// Invokes <see cref="OnDestruct"/> event.
        /// </summary>
        public void Destruct()
        {
            OnDestruct?.Invoke();
        }
    }
}
