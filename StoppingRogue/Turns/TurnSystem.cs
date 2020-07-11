using Stride.Core.MicroThreading;
using Stride.Engine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StoppingRogue.Turns
{
    public static class TurnSystem
    {
        public static readonly TimeSpan TurnLength = TimeSpan.FromSeconds(1.0 / 2.0);
        public static int TurnNumber { get; internal set; }
        public static TimeSpan RemainingTime { get; internal set; }

        internal static Channel<bool> Channel { get; } =
        new Channel<bool> { Preference = ChannelPreference.PreferSender };
        public static async Task NextTurn()
        {
            await Channel.Receive();
        }

        public static void Enable(Scene rootScene)
        {
            var e = rootScene.Entities.First(e => e.Get<TurnScript>() != null);
            var ts = e.Get<TurnScript>();
            ts.Enabled = true;
        }

        public static void Disable(Scene rootScene)
        {
            var e = rootScene.Entities.First(e => e.Get<TurnScript>() != null);
            var ts = e.Get<TurnScript>();
            ts.Enabled = false;
        }
    }
}
