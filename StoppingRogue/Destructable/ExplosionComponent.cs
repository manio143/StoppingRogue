using StoppingRogue.Levels;
using Stride.Engine;

namespace StoppingRogue.Destructable
{
    public class ExplosionComponent : ScriptComponent
    {
        public event System.Action PostExplosion;
        public void Explode()
        {
            // TODO: emit particles
            PostExplosion?.Invoke();
        }
    }
}
