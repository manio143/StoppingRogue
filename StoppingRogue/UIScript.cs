using StoppingRogue.Input;
using StoppingRogue.Levels;
using StoppingRogue.Robot;
using StoppingRogue.Tasks;
using StoppingRogue.Turns;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.Graphics;
using Stride.Rendering.Sprites;
using Stride.UI;
using Stride.UI.Controls;
using Stride.UI.Panels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StoppingRogue
{
    /// <summary>
    /// The UI manager and high level logic invoker.
    /// </summary>
    public class UIScript : SyncScript
    {
        private EventReceiver tasksCompleted = new EventReceiver(TaskProcessor.AllTasksCompleted);

        // UI pages
        public UIPage GamePage;
        public UIPage GameResult;
        public UIPage LevelSelection;

        private UIComponent UI;

        // Sprite sheet for UI elements
        public SpriteSheet UISheet;

        [DataMemberIgnore]
        public InputController inputController;
        [DataMemberIgnore]
        public RobotBrain robotBrain;

        /// <summary>
        /// The level loader component.
        /// </summary>
        public LevelSelection Loader;

        private List<TaskComponent> tasks = new List<TaskComponent>();

        enum UIState { InGame, GameResult, LevelSelection }
        private UIState state = UIState.LevelSelection;

        enum Result { None, Success, Failure }

        private int completedLevels = 0;
        private int currentLevel;

        /// <summary>
        /// How many levels are available?
        /// </summary>
        /// <remarks><see cref="LevelSelection"/> page needs to have that many buttons.</remarks>
        public int NumberOfLevels { get; set; }

        public override void Start()
        {
            UI = Entity.Get<UIComponent>();
            CreateLevelSelection();

            // Setup returning to level selection menu from game result
            var continueBtn = (GameResult.RootElement as Panel).Children[3] as Button;
            continueBtn.Click += (sender, args) =>
            {
                // remove all entities
                foreach (var entity in Entity.Scene.Children[0].Entities)
                    entity.Scene = null;
                Entity.Scene.Children.RemoveAt(0);

                // update available levels
                CreateLevelSelection();

                UI.Page = LevelSelection;
                state = UIState.LevelSelection;
            };

            UI.Page = LevelSelection;
        }

        public override void Update()
        {
            DebugHacks();
            if(Input.IsKeyPressed(Stride.Input.Keys.R))
            {
                foreach (var entity in Entity.Scene.Children[0].Entities)
                    entity.Scene = null;
                Entity.Scene.Children.RemoveAt(0);

                Loader.LoadLevel(currentLevel + 1);
                PopulateTasks();
            }
            if (state == UIState.InGame)
            {
                var timeLeft = TurnSystem.RemainingTime;

                // is the game over
                Result result = tasksCompleted.TryReceive()
                    ? Result.Success
                    : timeLeft.Ticks < 0
                    ? Result.Failure
                    : Result.None;

                if (result != Result.None)
                {
                    // update GameResult page
                    var cyclesLeft = 5 - robotBrain.Cycles;
                    var starsNum = result == Result.Success
                        ? Math.Max(1, cyclesLeft)
                        : 0;
                    var message = result == Result.Failure
                        ? "The robot became sentient and took over the world!"
                        : "You have succesfully completed all tasks";

                    var root = GameResult.RootElement as Panel;
                    var description = root.Children[1] as TextBlock;
                    var stars = (root.Children[2] as Panel).Children.Cast<ImageElement>().ToArray();

                    description.Text = message;
                    for (int i = 0; i < stars.Length; i++)
                    {
                        if (stars[i].Source == null)
                            stars[i].Source = new SpriteFromSheet
                            { Sheet = UISheet, CurrentFrame = 10 };
                        stars[i].Visibility = i < starsNum ? Visibility.Visible : Visibility.Hidden;
                    }

                    // Unlock next level
                    if (result == Result.Success)
                        completedLevels = Math.Max(completedLevels, currentLevel + 1);

                    UI.Page = GameResult;

                    TurnSystem.Disable(Entity.Scene);
                    state = UIState.GameResult;
                }
                else
                {
                    // update Game UI
                    var nextUserAction = inputController.NextAction.GetActionType();
                    var nextRobotAction = robotBrain.NextAction.GetActionType();

                    var grid = GamePage.RootElement as Grid;
                    var actions = ((grid.Children[0] as Panel).Children[0] as Panel).Children.Cast<Border>().ToArray();
                    var timer = grid.Children[2] as TextBlock;
                    var taskEntries = ((grid.Children.Last() as Panel).Children.Skip(1).Select(c => ((c as Panel).Children[0] as ToggleButton, (c as Panel).Children[1] as TextBlock))).ToArray();

                    timer.Text = timeLeft.ToString("mm\\:ss");

                    for (int i = 0; i < taskEntries.Length; i++)
                    {
                        if (i >= tasks.Count)
                        {
                            taskEntries[i].Item1.Parent.Visibility = Visibility.Collapsed;
                            continue;
                        }
                        taskEntries[i].Item1.Parent.Visibility = Visibility.Visible;
                        taskEntries[i].Item1.State = tasks[i].Completed ? ToggleState.Checked : ToggleState.UnChecked;
                        taskEntries[i].Item2.Text = TaskText(tasks[i].Type);
                        taskEntries[i].Item2.WrapText = true;
                        taskEntries[i].Item2.Width = 350;
                    }

                    UpdateActions(actions[0], ActionType.Movement, nextUserAction, nextRobotAction, 0);
                    UpdateActions(actions[1], ActionType.Hold, nextUserAction, nextRobotAction, 2);
                    UpdateActions(actions[2], ActionType.Laser, nextUserAction, nextRobotAction, 4);
                    UpdateActions(actions[3], ActionType.Light, nextUserAction, nextRobotAction, 6);
                }
            }
        }

        [Conditional("DEBUG")]
        private void DebugHacks()
        {
            if (Input.IsKeyPressed(Stride.Input.Keys.F1))
            {
                // quick access to all levels
                completedLevels = NumberOfLevels;
                CreateLevelSelection();
            }
        }

        /// <summary>
        /// Displays a green frame around next user action, and red frame around
        /// next robot action.
        /// </summary>
        private void UpdateActions(Border action, ActionType act, ActionType nextUserAction, ActionType nextRobotAction, int activeFrame)
        {
            var userBorder = action;
            var robotBorder = userBorder.Content as Border;
            var image = robotBorder.Content as ImageElement;
            userBorder.BorderColor = nextUserAction == act
                ? new Color(0x14, 0xe8, 0, 0xff) : new Color(0, 0, 0, 0);
            robotBorder.BorderColor = nextRobotAction == act
                ? new Color(0xef, 0, 0, 0xff) : new Color(0, 0, 0, 0);

            // if action is not available for the user choose a greyed out icon
            (image.Source as SpriteFromSheet).CurrentFrame = inputController.userActions.Contains(act)
                ? activeFrame : activeFrame + 1;
        }

        private string TaskText(TaskType type)
        {
            switch (type)
            {
                case TaskType.FillBoxSlot:
                    return "Fill grey slot with metal box";
                case TaskType.FillPipeSlot:
                    return "Fill blue slot with cut pipe";
                case TaskType.SwitchLightOn:
                    return "Send light to the sensor on the wall";
                case TaskType.DestroyMainrfame:
                    return "Destroy mainframe computer";
                default:
                    throw new InvalidOperationException();
            }
        }

        private static bool firstInit = true;
        /// <summary>
        /// Update <see cref="LevelSelection"/> buttons.
        /// </summary>
        public void CreateLevelSelection()
        {
            var root = (LevelSelection.RootElement as Panel).Children.Last() as Panel;
            while (root.Children.Count < NumberOfLevels)
            {
                var goodButton = root.Children.First() as Button;
                var goodText = goodButton.Content as TextBlock;
                root.Children.Add(new Button
                {
                    Content = new TextBlock
                    {
                        Font = goodText.Font,
                        TextSize = goodText.TextSize,
                        TextAlignment = goodText.TextAlignment,
                        HorizontalAlignment = goodText.HorizontalAlignment,
                        VerticalAlignment = goodText.VerticalAlignment,
                    },
                    NotPressedImage = new SpriteFromSheet { Sheet = UISheet },
                    MouseOverImage = new SpriteFromSheet { Sheet = UISheet },
                    PressedImage = new SpriteFromSheet { Sheet = UISheet, CurrentFrame = 12 },
                    Width = goodButton.Width,
                    Height = goodButton.Height,
                    Margin = goodButton.Margin,
                });
            }
            for (int i = 0; i < NumberOfLevels; i++)
            {
                var button = root.Children[i] as Button;
                var text = button.Content as TextBlock;

                // available level are given orange sprite
                // the rest grey ones
                button.IsEnabled = i <= completedLevels;
                (button.NotPressedImage as SpriteFromSheet).CurrentFrame =
                    i <= completedLevels ? 11 : 12;
                (button.MouseOverImage as SpriteFromSheet).CurrentFrame =
                    i <= completedLevels ? 11 : 12;

                if (firstInit)
                {
                    text.Text = (i + 1).ToString();
                    button.ClickMode = ClickMode.Release;
                    int capture = i;
                    button.Click += (sender, args) =>
                    {
                        if (state == UIState.InGame)
                            return;
                        Loader.LoadLevel(capture + 1);
                        currentLevel = capture;
                        state = UIState.InGame;
                        UI.Page = GamePage;
                        PopulateTasks();
                    };
                }
            }
            firstInit = false;

            // All levels have been passed
            if(completedLevels == NumberOfLevels && !(root.Children.Last() is TextBlock))
            {
                root.Children.Add(new TextBlock
                {
                    Font = Content.Load<SpriteFont>("StrideDefaultFont"),
                    Text = "You did it!",
                    TextSize = 15,
                    TextAlignment = TextAlignment.Center,
                });
            }
        }

        /// <summary>
        /// Find entities with tasks and create a list of those.
        /// </summary>
        private void PopulateTasks()
        {
            tasks.Clear();
            var subscene = Entity.Scene.Children.First();
            foreach (var entity in subscene.Entities)
            {
                var task = entity.Get<TaskComponent>();
                if (task != null)
                {
                    tasks.Add(task);
                }
            }
        }
    }
}
