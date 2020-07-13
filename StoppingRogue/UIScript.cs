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
using System.Linq;
using System.Text;

namespace StoppingRogue
{
    public class UIScript : SyncScript
    {
        private EventReceiver tasksCompleted = new EventReceiver(TaskProcessor.AllTasksCompleted);

        public UIPage GamePage;
        public UIPage GameResult;
        public UIPage LevelSelection;
        private UIComponent UI;
        public SpriteSheet UISheet;

        [DataMemberIgnore]
        public InputController inputController;
        [DataMemberIgnore]
        public RobotBrain robotBrain;

        public LevelSelection Loader;
        private List<TaskComponent> tasks = new List<TaskComponent>();

        enum UIState { InGame, GameResult, LevelSelection }
        private UIState state = UIState.LevelSelection;

        enum Result { None, Success, Failure }

        private int completedLevels = 0;
        private int currentLevel;

        public int NumberOfLevels { get; set; }

        public override void Start()
        {
            UI = Entity.Get<UIComponent>();
            CreateLevelSelection();
            
            var continueBtn = (GameResult.RootElement as Panel).Children[3] as Button;
            continueBtn.Click += (sender, args) =>
            {
                foreach (var entity in Entity.Scene.Children[0].Entities)
                    entity.Scene = null;
                Entity.Scene.Children.RemoveAt(0);
                CreateLevelSelection();
                UI.Page = LevelSelection;
                state = UIState.LevelSelection;
            };

            UI.Page = LevelSelection;
        }

        public override void Update()
        {
            if (state == UIState.InGame)
            {
                var timeLeft = TurnSystem.RemainingTime;
                Result result = Result.None;
                if (tasksCompleted.TryReceive())
                {
                    result = Result.Success;
                }
                else if (timeLeft.Ticks < 0)
                {
                    result = Result.Failure;
                }

                if (result != Result.None)
                {
                    var cyclesLeft = 10 - robotBrain.Cycles;
                    var starsNum = (int)Math.Max(1, Math.Floor(cyclesLeft / 2.0));
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
                            {
                                Sheet = UISheet,
                                CurrentFrame = 10
                            };
                        stars[i].Visibility = i < starsNum ? Visibility.Visible : Visibility.Hidden;
                    }

                    if (result == Result.Success)
                        completedLevels = Math.Max(completedLevels, currentLevel + 1);

                    UI.Page = GameResult;

                    TurnSystem.Disable(Entity.Scene);
                    state = UIState.GameResult;
                }
                else
                {
                    var nextUserAction = inputController.NextAction.GetActionType();
                    var nextRobotAction = robotBrain.NextAction.GetActionType();

                    var grid = GamePage.RootElement as Grid;
                    var actions = ((grid.Children[0] as Panel).Children[0] as Panel).Children.Cast<Border>().ToArray();
                    var timer = grid.Children[1] as TextBlock;
                    var taskEntries = ((grid.Children[2] as Panel).Children.Skip(1).Select(c => ((c as Panel).Children[0] as ToggleButton, (c as Panel).Children[1] as TextBlock))).ToArray();

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
                    }

                    UpdateActions(actions[0], ActionType.Movement, nextUserAction, nextRobotAction, 0);
                    UpdateActions(actions[1], ActionType.Hold, nextUserAction, nextRobotAction, 2);
                    UpdateActions(actions[2], ActionType.Laser, nextUserAction, nextRobotAction, 4);
                    UpdateActions(actions[3], ActionType.Light, nextUserAction, nextRobotAction, 6);
                }
            }
        }

        private void UpdateActions(Border action, ActionType act, ActionType nextUserAction, ActionType nextRobotAction, int activeFrame)
        {
            var userBorder = action;
            var robotBorder = userBorder.Content as Border;
            var image = robotBorder.Content as ImageElement;
            userBorder.BorderColor = nextUserAction == act
                ? new Color(0x14, 0xe8, 0, 0xff) : new Color(0, 0, 0, 0);
            robotBorder.BorderColor = nextRobotAction == act
                ? new Color(0xef, 0, 0, 0xff) : new Color(0, 0, 0, 0);
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

        public void CreateLevelSelection()
        {
            var root = (LevelSelection.RootElement as Panel).Children.Last() as Panel;
            for (int i = 0; i < NumberOfLevels; i++)
            {
                var button = root.Children[i] as Button;
                var text = button.Content as TextBlock;
                text.Text = (i + 1).ToString();
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                button.IsEnabled = i <= completedLevels;
                (button.NotPressedImage as SpriteFromSheet).CurrentFrame =
                    i <= completedLevels ? 11 : 12;
                (button.MouseOverImage as SpriteFromSheet).CurrentFrame =
                    i <= completedLevels ? 11 : 12;
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
