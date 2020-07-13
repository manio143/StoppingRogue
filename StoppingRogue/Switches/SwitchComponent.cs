using Stride.Core;
using Stride.Engine;
using System;
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
        /// If not positive, it reports the negated state.
        /// </summary>
        public bool Positive { get; set; } = true;

        /// <summary>
        /// On switch state change.
        /// </summary>
        public event Action<bool> OnSwitch;

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
            if (Positive)
                OnSwitch?.Invoke(State);
            else
                OnSwitch?.Invoke(!State);
        }
    }
}
