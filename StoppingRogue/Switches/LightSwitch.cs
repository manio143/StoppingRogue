using Stride.Core;
using Stride.Engine;
using Stride.Engine.Design;
using System.ComponentModel;

namespace StoppingRogue.Switches
{
    [DefaultEntityComponentProcessor(typeof(LightSwitchProcessor))]
    [DataContract]
    public class LightSwitch : EntityComponent
    {
        [DefaultValue(false)]
        public bool Active { get; set; }
    }
}
