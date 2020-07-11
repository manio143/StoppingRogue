using Stride.Core.MicroThreading;
using System;
using System.Threading.Tasks;

namespace StoppingRogue.Turns
{
    public static class TurnSystem
    {
        public static readonly TimeSpan TurnLenght = TimeSpan.FromMilliseconds(1.0 / 3.0);
        public static int TurnNumber { get; internal set; }
        public static TimeSpan RemainingTime { get; internal set; }

        internal static Channel<bool> Channel { get; } =
        new Channel<bool> { Preference = ChannelPreference.PreferSender };
        public static async Task NextTurn()
        {
            await Channel.Receive();
        }
    }
}
