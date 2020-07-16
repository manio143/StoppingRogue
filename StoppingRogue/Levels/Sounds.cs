using Stride.Audio;
using Stride.Core;

namespace StoppingRogue.Levels
{
    [DataContract]
    public struct Sounds
    {
        public Sound DoorSound { get; set; }
        public Sound GrabSound { get; set; }
        public Sound ReleaseSound { get; set; }
        public Sound SwitchSound { get; set; }
        public Sound LaserSound { get; set; }
        public Sound MistakeSound { get; set; }
        public Sound BreakSound { get; set; }
        public Sound LightSound { get; set; }
        public Sound SlotSound { get; set; }
    }
}
