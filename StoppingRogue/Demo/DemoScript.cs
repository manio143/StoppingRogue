using StoppingRogue.Levels;
using StoppingRogue.Turns;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace StoppingRogue.Demo
{
    public class DemoScript : StartupScript
    {
        public SpriteSheet Environment { get; set; }
        public SpriteSheet Robot { get; set; }
        public SpriteSheet Items { get; set; }

        private Level level;
        public override void Start()
        {
            level = LevelReader.Read("Resources/TileTest.txt");
            var actionController = Entity.GetOrCreate<ActionController>();
            var builder = new LevelBuilder(Environment, Robot, Items, actionController);
            var scene = builder.Build(level);

            this.Entity.Scene.Children.Add(scene);
        }
    }
}
