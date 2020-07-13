using Stride.Core.Annotations;
using Stride.Engine;
using Stride.Games;
using Stride.Physics;

namespace StoppingRogue.Tasks
{
    /// <summary>
    /// Manages collisions for <see cref="LightSwitch"/>.
    /// </summary>
    /// <remarks>
    /// The light switch could've been an AsyncScript
    /// and it would probably be shorter.
    /// </remarks>
    public class LightSwitchProcessor : EntityProcessor<LightSwitch, LightSwitchProcessor.Data>
    {
        /// <summary>
        /// Gets associated data for a <see cref="LightSwitch"/>.
        /// </summary>
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

            foreach (var kvp in ComponentDatas)
            {
                var data = kvp.Value;

                // if light was shone on the switch, activate it
                if (data.Physics.Collisions.Count > 0)
                {
                    data.Switch.Active = true;
                }
            }
        }
    }
}
