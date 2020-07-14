using StoppingRogue.Input;
using StoppingRogue.Robot;
using StoppingRogue.Turns;
using Stride.Core.IO;
using Stride.Engine;
using Stride.Graphics;

namespace StoppingRogue.Levels
{
    /// <summary>
    /// Manages the instantiation of levels.
    /// </summary>
    public class LevelSelection : ScriptComponent
    {
        public SpriteSheet Environment { get; set; }
        public SpriteSheet Robot { get; set; }
        public SpriteSheet Items { get; set; }
        public CameraComponent Camera { get; set; }
        public UIScript UIScript { get; set; }

        private Level level;

        /// <summary>
        /// Load level asset, build scene, setup dependencies.
        /// </summary>
        public void LoadLevel(int levelNumber)
        {
            // Read RawAsset by its URI
            using (var stream = Content.OpenAsStream($"LVL{levelNumber}", StreamFlags.None))
                level = LevelReader.Read(stream);

            var actionController = Entity.GetOrCreate<ActionController>();
            var builder = new LevelBuilder(Environment, Robot, Items, actionController);

            var scene = builder.Build(level, out var robot);
            this.Entity.Scene.Children.Add(scene);

            actionController.Robot = robot.Get<RobotController>();

            var robotBrain = Entity.GetOrCreate<RobotBrain>();
            robotBrain.actions = level.ActionPattern;
            robotBrain.Reset();

            var inputController = Entity.GetOrCreate<InputController>();
            inputController.userActions = level.UserActions;

            Camera.Entity.Get<RobotCameraFollower>().Robot = robot;

            TurnSystem.Reset();
            TurnSystem.Enable(Entity.Scene);
            TurnSystem.RemainingTime = level.ActionPattern.Length * TurnSystem.TurnLength * 5;

            UIScript.inputController = inputController;
            UIScript.robotBrain = robotBrain;
        }
    }
}
