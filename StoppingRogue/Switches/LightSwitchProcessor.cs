using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Games;
using Stride.Physics;

namespace StoppingRogue.Switches
{
    public class LightSwitchProcessor : EntityProcessor<LightSwitch, LightSwitchProcessor.Data>
    {
        protected override Data GenerateComponentData([NotNull] Entity entity, [NotNull] LightSwitch component)
        {
            return new Data
            {
                Switch = component,
                Physics = entity.Get<RigidbodyComponent>(),
            };
        }

        public class Data
        {
            public LightSwitch Switch { get; set; }
            public RigidbodyComponent Physics { get; set; }
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
            foreach(var kvp in ComponentDatas)
            {
                var data = kvp.Value;
                if(data.Physics.Collisions.Count > 0)
                {
                    data.Switch.Active = true;
                    //TODO change graphics of parent
                }
            }
        }
    }
}
