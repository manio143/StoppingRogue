using Stride.Engine;
using System;

namespace StoppingRogue.Destructable
{
    /// <summary>
    /// Present alongside the <see cref="DestructableComponent"/>.
    /// Meant to be called from <see cref="DestructableComponent.OnDestruct"/> and display a particle explosion.
    /// </summary>
    public class ExplosionComponent : ScriptComponent
    {
        /// <summary>
        /// What to do after destruction (e.g. remove entity).
        /// </summary>
        public event Action PostExplosion;

        /// <summary>
        /// Emits an explosion and invokes <see cref="PostExplosion"/>.
        /// </summary>
        public void Explode()
        {
            // TODO: emit particles
            PostExplosion?.Invoke();
        }
    }
}
