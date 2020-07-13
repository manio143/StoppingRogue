using Stride.Core.MicroThreading;
using Stride.Engine;
using System;
using System.Linq;

namespace StoppingRogue.Turns
{
    /// <summary>
    /// Provides access to resources one turn at a time.
    /// </summary>
    public static class TurnSystem
    {
        /// <summary>
        /// How long one turn lasts.
        /// </summary>
        public static readonly TimeSpan TurnLength = TimeSpan.FromSeconds(.6);
        
        /// <summary>
        /// Current turn number.
        /// </summary>
        public static int TurnNumber { get; internal set; }
        
        /// <summary>
        /// Time remaining till the end of level.
        /// </summary>
        public static TimeSpan RemainingTime { get; internal set; }

        internal static Channel<bool> Channel { get; private set; }
            = new Channel<bool> { Preference = ChannelPreference.PreferSender };

        /// <summary>
        /// Await the next turn.
        /// </summary>
        /// <returns>True if system was reset, false otherwise.</returns>
        public static ChannelMicroThreadAwaiter<bool> NextTurn()
        {
            return Channel.Receive();
        }

        /// <summary>
        /// Broacast a reset and reset turn number.
        /// </summary>
        public static void Reset()
        {
            while (Channel?.Balance < 0)
                Channel?.Send(true);
            TurnNumber = 0;
        }

        /// <summary>
        /// Find a <see cref="TurnScript"/> and enable it.
        /// </summary>
        public static void Enable(Scene rootScene)
        {
            var e = rootScene.Entities.First(e => e.Get<TurnScript>() != null);
            var ts = e.Get<TurnScript>();
            ts.Enabled = true;
        }

        /// <summary>
        /// Find a <see cref="TurnScript"/> and disable it.
        /// </summary>
        public static void Disable(Scene rootScene)
        {
            var e = rootScene.Entities.First(e => e.Get<TurnScript>() != null);
            var ts = e.Get<TurnScript>();
            ts.Enabled = false;
        }
    }
}
