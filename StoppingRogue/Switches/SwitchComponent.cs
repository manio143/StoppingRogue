using Stride.Core;
using Stride.Engine;
using System.Collections.Generic;
using System.Diagnostics;

namespace StoppingRogue.Switches
{
    /// <summary>
    /// A toggleable switch.
    /// </summary>
    [DataContract]
    public class SwitchComponent : EntityComponent
    {
        /// <summary>
        /// List of doors this switch affects.
        /// </summary>
        public List<DoorComponent> Doors { get; } = new List<DoorComponent>();

        /// <summary>
        /// Switch state.
        /// </summary>
        public bool State { get; private set; }

        /// <summary>
        /// Change the state and invoke <see cref="OnSwitch"/> event.
        /// </summary>
        public void Switch()
        {
            Debug.WriteLine($"Switch toggled on '{Entity.Name}'");
            State = !State;
            
            foreach (var door in Doors)
                door.OpenClose();
        }
    }
}
