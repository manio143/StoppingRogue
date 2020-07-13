using Stride.Core;
using Stride.Engine;

namespace StoppingRogue.Items
{
    /// <summary>
    /// Describes items that can be held.
    /// </summary>
    public enum Item
    {
        WoodBox,
        MetalBox,
        CutPipe,
    }

    /// <summary>
    /// Describes an entity that can be picked up.
    /// </summary>
    [DataContract]
    public class ItemComponent : EntityComponent
    {
        public Item ItemType { get; set; }
    }
}
