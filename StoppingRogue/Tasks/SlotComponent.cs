using StoppingRogue.Items;
using Stride.Core;
using Stride.Engine;
using Stride.Rendering.Sprites;
using System;

namespace StoppingRogue.Tasks
{
    /// <summary>
    /// A slot for an item.
    /// </summary>
    [DataContract]
    public class SlotComponent : EntityComponent
    {
        /// <summary>
        /// Associated task.
        /// </summary>
        public TaskComponent taskComponent;

        /// <summary>
        /// Type of accepted item.
        /// </summary>
        public Item ItemType { get; set; }

        /// <summary>
        /// Fill slot with an item to complete the task.
        /// </summary>
        /// <param name="item"></param>
        public void Fill(Item item)
        {
            VerifyConsistency();
            if (item != ItemType)
                throw new InvalidOperationException();
            
            taskComponent.Completed = true;
            (Entity.Get<SpriteComponent>().SpriteProvider as SpriteFromSheet).CurrentFrame = ItemType == Item.CutPipe ? 28 : 27;
        }

        private void VerifyConsistency()
        {
            switch (ItemType)
            {
                case Item.MetalBox:
                    if (taskComponent.Type != TaskType.FillBoxSlot)
                        throw new InvalidOperationException();
                    break;
                case Item.CutPipe:
                    if(taskComponent.Type != TaskType.FillPipeSlot)
                        throw new InvalidOperationException();
                    break;
                case Item.WoodBox:
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
