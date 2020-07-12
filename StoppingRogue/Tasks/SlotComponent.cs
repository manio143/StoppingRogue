using StoppingRogue.Items;
using Stride.Core;
using Stride.Engine;
using Stride.Rendering.Sprites;
using System;
using System.Collections.Generic;
using System.Text;

namespace StoppingRogue.Tasks
{
    [DataContract]
    public class SlotComponent : EntityComponent
    {
        public TaskComponent taskComponent;
        public Item ItemType { get; set; }

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
