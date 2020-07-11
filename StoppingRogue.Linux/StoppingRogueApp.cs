using Stride.Engine;

namespace StoppingRogue
{
    class StoppingRogueApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
