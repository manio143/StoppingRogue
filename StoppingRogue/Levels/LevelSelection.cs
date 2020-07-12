using StoppingRogue.Input;
using StoppingRogue.Robot;
using StoppingRogue.Turns;
using Stride.Core.IO;
using Stride.Engine;
using Stride.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StoppingRogue.Levels
{
    public class LevelSelection : ScriptComponent
    {
        public SpriteSheet Environment { get; set; }
        public SpriteSheet Robot { get; set; }
        public SpriteSheet Items { get; set; }
        public CameraComponent Camera { get; set; }
        public UIScript UIScript { get; set; }

        private Level level;
        public void LoadLevel(int levelNumber)
        {
            string lvl;
            using (var stream = Content.FileProvider.OpenStream($"LVL{levelNumber}", VirtualFileMode.Open, VirtualFileAccess.Read))
            using (var reader = new StreamReader(stream))
                lvl = reader.ReadToEnd();
            level = LevelReader.Read(lvl.Split('\n'));

            var actionController = Entity.GetOrCreate<ActionController>();
            var builder = new LevelBuilder(Environment, Robot, Items, actionController);

            var scene = builder.Build(level, out var robot);
            this.Entity.Scene.Children.Add(scene);

            var robotBrain = Entity.GetOrCreate<RobotBrain>();
            robotBrain.actions = level.ActionPattern;
            robotBrain.userActions = level.UserActions;
            robotBrain.Reset();

            var inputController = Entity.GetOrCreate<InputController>();
            inputController.userActions = level.UserActions;

            Camera.Entity.Get<RobotCameraFollower>().Robot = robot;

            TurnSystem.Reset();
            TurnSystem.Enable(Entity.Scene);
            TurnSystem.RemainingTime = level.ActionPattern.Length * TurnSystem.TurnLength * 10;

            UIScript.inputController = inputController;
            UIScript.robotBrain = robotBrain;
        }
    }
}
