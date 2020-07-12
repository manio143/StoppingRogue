using Stride.Core;
using Stride.Engine;
using System;

namespace StoppingRogue.Destructable
{
    [DataContract]
    public class DestructableComponent : EntityComponent
    {
        public event Action OnDestruct;

        public void Destruct()
        {
            OnDestruct?.Invoke();
        }
    }
}
