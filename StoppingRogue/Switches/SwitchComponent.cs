using Stride.Core;
using Stride.Engine;
using System;
using System.Diagnostics;

namespace StoppingRogue.Switches
{
    [DataContract]
    public class SwitchComponent : EntityComponent
    {
        public bool Positive { get; set; } = true;
        public event Action<bool> OnSwitch;
        public bool State { get; set; }

        public void Switch()
        {
            Debug.WriteLine($"Switch toggled on '{Entity.Name}'");
            if (Positive)
                OnSwitch?.Invoke(State);
            else
                OnSwitch?.Invoke(!State);
        }
    }
}
