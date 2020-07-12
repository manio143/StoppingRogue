using Stride.Core;
using Stride.Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace StoppingRogue.Items
{
    public enum Item
    {
        WoodBox,
        MetalBox,
        CutPipe,
    }
    [DataContract]
    public class ItemComponent : EntityComponent
    {
        public Item ItemType { get; set; }
    }
}
