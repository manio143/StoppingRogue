using StoppingRogue.Destructable;
using StoppingRogue.Items;
using StoppingRogue.Robot;
using StoppingRogue.Switches;
using StoppingRogue.Tasks;
using StoppingRogue.Turns;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Physics;
using Stride.Rendering.Sprites;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StoppingRogue.Levels
{
    public class LevelBuilder
    {
        private SpriteSheet environmentSheet;
        private SpriteSheet robotSheet;
        private SpriteSheet itemSheet;
        private ActionController actionController;

        public LevelBuilder(SpriteSheet environmentSheet, SpriteSheet robotSheet, SpriteSheet itemSheet, ActionController actionController)
        {
            this.environmentSheet = environmentSheet;
            this.robotSheet = robotSheet;
            this.itemSheet = itemSheet;
            this.actionController = actionController;
        }

        public Scene Build(Level level)
        {
            var scene = new Scene();
            var switchSetups = new List<(Entity, bool, Int2)>();
            var doors = new Dictionary<Int2, Entity>();
            for (int line = 0; line < level.Tiles.GetLength(1); line++)
            {
                for (int col = 0; col < level.Tiles.GetLength(0); col++)
                {
                    var tile = level.Tiles[col, line];
                    if (tile == TileType.PressurePlateWithBox)
                    {
                        var plate = new Entity();
                        SetPosition(plate, col, line);
                        plate.Name = $"({col}, {line}) {TileType.PressurePlate}";
                        AddComponents(plate, TileType.PressurePlate);

                        var box = new Entity();
                        SetPosition(box, col, line);
                        box.Name = $"({col}, {line}) {TileType.WoodBox}";
                        AddComponents(box, TileType.WoodBox);
                        //TODO: box may need Z higher

                        scene.Entities.Add(plate);
                        scene.Entities.Add(box);
                        if (level.SwitchMapping.ContainsKey(new Int2(col, line)))
                        {
                            var (p, d) = level.SwitchMapping[new Int2(col, line)];
                            switchSetups.Add((plate, p, d));
                        }
                    }
                    else
                    {
                        if (IsMovable(tile))
                        {
                            var floor = new Entity();
                            SetPosition(floor, col, line);
                            floor.Name = $"({col}, {line}) {TileType.Floor}";

                            AddComponents(floor, TileType.Floor);

                            scene.Entities.Add(floor);
                        }
                        var entity = new Entity();
                        SetPosition(entity, col, line);
                        entity.Name = $"({col}, {line}) {tile}";

                        AddComponents(entity, tile);

                        scene.Entities.Add(entity);
                        if (level.SwitchMapping.ContainsKey(new Int2(col, line)))
                        {
                            var (p, d) = level.SwitchMapping[new Int2(col, line)];
                            switchSetups.Add((entity, p, d));
                        }
                        else if (level.SwitchMapping.Any(kvp => kvp.Value.Item2 == new Int2(col, line)))
                        {
                            doors.Add(new Int2(col, line), entity);
                        }
                    }
                }
            }

            foreach (var (swe, pos, dp) in switchSetups)
            {
                var swit = swe.Get<SwitchComponent>();
                if (swit == null)
                    throw new InvalidDataException();
                swit.Positive = pos;
                var door = doors[dp].Get<DoorComponent>();
                if (door == null)
                    throw new InvalidDataException();
                swit.OnSwitch += (b) => { 
                    if (b) door.Open(); else door.Close(); 
                };
            }

            return scene;
        }

        private bool IsMovable(TileType tile)
        {
            switch (tile)
            {
                case TileType.WoodCrate:
                case TileType.WoodBox:
                case TileType.MetalBox:
                case TileType.LongPipe:
                case TileType.LongPipeVertical:
                case TileType.CutPipe:
                case TileType.GlassPane:
                case TileType.Robot:
                    return true;
                default:
                    return false;
            }
        }

        private void SetPosition(Entity entity, int col, int line)
        {
            var transform = entity.GetOrCreate<TransformComponent>();
            transform.Position = new Vector3(col, -line, 0);
            transform.Scale = (Vector3)new Vector2(1 / 0.48f);
        }

        private void AddComponents(Entity entity, TileType tile)
        {
            AddSprite(entity, tile);

            if (tile == TileType.Robot)
            {
                var rc = entity.GetOrCreate<RobotController>();
                actionController.Robot = rc;
                var rl = entity.GetOrCreate<RobotLight>();
                rl.robotSpriteSheet = robotSheet;
                entity.GetOrCreate<RobotHolder>();
                var rls = entity.GetOrCreate<RobotLaser>();
                rls.itemSpriteSheet = itemSheet;
            }
            if (HasCollider(tile))
            {
                var rb = entity.GetOrCreate<RigidbodyComponent>();
                rb.ColliderShapes.Add(new BoxColliderShapeDesc()
                {
                    Is2D = true,
                    Size = new Vector3(0.45f, 0.45f, 0),
                });
                rb.RigidBodyType = RigidBodyTypes.Kinematic;
                rb.CollisionGroup = CollisionFilterGroups.DefaultFilter;
                rb.CanCollideWith = CollisionFilterGroupFlags.DefaultFilter | CollisionFilterGroupFlags.SensorTrigger;
            }
            if (tile == TileType.OpenedDoor)
            {
                var rb = entity.GetOrCreate<RigidbodyComponent>();
                rb.ColliderShapes.Add(new BoxColliderShapeDesc()
                {
                    Is2D = true,
                    Size = new Vector3(0.45f, 0.70f, 0),
                    LocalOffset = new Vector3(0, 0.35f, 0),
                });
                rb.RigidBodyType = RigidBodyTypes.Kinematic;
                rb.CollisionGroup = CollisionFilterGroups.SensorTrigger;
                rb.CanCollideWith = CollisionFilterGroupFlags.DefaultFilter;
            }
            if (tile == TileType.Door || tile == TileType.OpenedDoor)
            {
                var door = entity.GetOrCreate<DoorComponent>();
            }
            if (tile == TileType.LightSwitchWall)
            {
                var switchEntity = new Entity();
                var rb = switchEntity.GetOrCreate<RigidbodyComponent>();
                rb.ColliderShapes.Add(new BoxColliderShapeDesc()
                {
                    Is2D = true,
                    Size = new Vector3(0.45f, 0.45f, 0),
                });
                rb.RigidBodyType = RigidBodyTypes.Kinematic;
                rb.CollisionGroup = CollisionFilterGroups.CustomFilter1;
                rb.CanCollideWith = CollisionFilterGroupFlags.CustomFilter1;

                var ls = switchEntity.GetOrCreate<LightSwitch>();
                var task = switchEntity.GetOrCreate<TaskComponent>();
                task.Type = TaskType.SwitchLightOn;
                ls.taskComponent = task;
                entity.AddChild(switchEntity);
            }
            if (IsItem(tile))
            {
                var holdableEntity = new Entity();
                var rb = holdableEntity.GetOrCreate<RigidbodyComponent>();
                rb.ColliderShapes.Add(new BoxColliderShapeDesc()
                {
                    Is2D = true,
                    Size = new Vector3(0.45f, 0.45f, 0),
                });
                rb.RigidBodyType = RigidBodyTypes.Kinematic;
                rb.CollisionGroup = CollisionFilterGroups.CustomFilter2;
                rb.CanCollideWith = CollisionFilterGroupFlags.CustomFilter2;
                var item = holdableEntity.GetOrCreate<ItemComponent>();
                item.ItemType = GetItemType(tile);
                entity.AddChild(holdableEntity);
            }
            if (IsDestructible(tile))
            {
                var destr = entity.GetOrCreate<DestructableComponent>();

                var expl = entity.GetOrCreate<ExplosionComponent>();
                destr.OnDestruct += expl.Explode;
                destr.OnDestruct += () => Debug.WriteLine($"Entity '{entity.Name}' destroyed");

                if (tile == TileType.Mainframe)
                {
                    var mf = entity.GetOrCreate<MainframeComponent>();
                    var task = entity.GetOrCreate<TaskComponent>();
                    task.Type = TaskType.DestroyMainrfame;
                    mf.taskComponent = task;
                    expl.PostExplosion += mf.Destroy;
                }
                else if (tile == TileType.LongPipe || tile == TileType.LongPipeVertical)
                {
                    expl.PostExplosion += () =>
                    {
                        var cutPipe = new Entity();
                        var col = (int)entity.Transform.Position.X;
                        var line = -(int)entity.Transform.Position.Y;
                        SetPosition(cutPipe, col, line);
                        entity.Name = $"({col}, {line}) {TileType.CutPipe}";
                        AddComponents(cutPipe, TileType.CutPipe);

                        entity.Scene.Entities.Add(cutPipe);
                        entity.Scene = null;
                    };
                }
                else
                {
                    expl.PostExplosion += () => entity.Scene = null;
                }
            }
            if (tile == TileType.PressurePlate || tile == TileType.StepOnSwitch)
            {
                var switchComp = entity.GetOrCreate<SwitchComponent>();
                // switchComp.OnSwitch...
                // if inverted pressureplate switchComp.Positive = false
                if (tile == TileType.PressurePlate)
                {
                    entity.GetOrCreate<PressurePlate>();
                }
                else
                {
                    entity.GetOrCreate<StepOnSwitch>();
                }

                var rb = entity.GetOrCreate<RigidbodyComponent>();
                rb.ColliderShapes.Add(new BoxColliderShapeDesc()
                {
                    Is2D = true,
                    Size = new Vector3(0.45f, 0.45f, 0),
                });
                rb.RigidBodyType = RigidBodyTypes.Kinematic;
                rb.CollisionGroup = CollisionFilterGroups.SensorTrigger;
                rb.CanCollideWith = CollisionFilterGroupFlags.DefaultFilter;
            }
        }

        private void AddSprite(Entity entity, TileType tile)
        {
            var spriteComp = entity.GetOrCreate<SpriteComponent>();
            spriteComp.SpriteProvider = GetSpriteForTile(tile);
        }

        private ISpriteProvider GetSpriteForTile(TileType tile)
        {
            return new SpriteFromSheet()
            {
                Sheet = GetSpriteSheet(tile),
                CurrentFrame = GetFrame(tile)
            };
        }

        private bool HasCollider(TileType tile)
        {
            switch (tile)
            {
                case TileType.WallLower:
                case TileType.WallLowerFancy:
                case TileType.RightFacingWall:
                case TileType.LeftFacingWall:
                case TileType.BackFacingWall:
                case TileType.Door:
                case TileType.SlotForBox:
                case TileType.SlotForPipe:
                case TileType.LightSwitchWall:
                case TileType.Mainframe:
                case TileType.Counter:
                case TileType.CounterEdgeLeft:
                case TileType.CounterEdgeRight:
                case TileType.CounterVerticalLeft:
                case TileType.CounterVerticalRight:
                case TileType.WoodCrate:
                case TileType.WoodBox:
                case TileType.MetalBox:
                case TileType.LongPipe:
                case TileType.LongPipeVertical:
                case TileType.CutPipe:
                case TileType.GlassPane:
                case TileType.HoleInFloor:
                case TileType.Robot:
                    return true;
                case TileType.None:
                case TileType.Floor:
                case TileType.WallUpper:
                case TileType.WallEdgeUL:
                case TileType.WallEdgeUR:
                case TileType.WallEdgeLL:
                case TileType.WallEdgeLR:
                case TileType.OpenedDoor:
                case TileType.PressurePlate:
                case TileType.PressurePlateWithBox:
                case TileType.StepOnSwitch:
                default:
                    return false;
            }
        }
        private bool IsItem(TileType tile)
        {
            switch (tile)
            {
                case TileType.WoodBox:
                case TileType.MetalBox:
                case TileType.CutPipe:
                    return true;
                case TileType.WallLower:
                case TileType.WallLowerFancy:
                case TileType.RightFacingWall:
                case TileType.LeftFacingWall:
                case TileType.BackFacingWall:
                case TileType.Door:
                case TileType.SlotForBox:
                case TileType.SlotForPipe:
                case TileType.LightSwitchWall:
                case TileType.Mainframe:
                case TileType.Counter:
                case TileType.CounterEdgeLeft:
                case TileType.CounterEdgeRight:
                case TileType.CounterVerticalLeft:
                case TileType.CounterVerticalRight:
                case TileType.WoodCrate:
                case TileType.LongPipe:
                case TileType.LongPipeVertical:
                case TileType.GlassPane:
                case TileType.HoleInFloor:
                case TileType.Robot:
                case TileType.None:
                case TileType.Floor:
                case TileType.WallUpper:
                case TileType.WallEdgeUL:
                case TileType.WallEdgeUR:
                case TileType.WallEdgeLL:
                case TileType.WallEdgeLR:
                case TileType.OpenedDoor:
                case TileType.PressurePlate:
                case TileType.PressurePlateWithBox:
                case TileType.StepOnSwitch:
                default:
                    return false;
            }
        }
        private Item GetItemType(TileType tile)
        {
            switch (tile)
            {
                case TileType.WoodBox:
                    return Item.WoodBox;
                case TileType.MetalBox:
                    return Item.MetalBox;
                case TileType.CutPipe:
                    return Item.CutPipe;
                case TileType.WallLower:
                case TileType.WallLowerFancy:
                case TileType.RightFacingWall:
                case TileType.LeftFacingWall:
                case TileType.BackFacingWall:
                case TileType.Door:
                case TileType.SlotForBox:
                case TileType.SlotForPipe:
                case TileType.LightSwitchWall:
                case TileType.Mainframe:
                case TileType.Counter:
                case TileType.CounterEdgeLeft:
                case TileType.CounterEdgeRight:
                case TileType.CounterVerticalLeft:
                case TileType.CounterVerticalRight:
                case TileType.WoodCrate:
                case TileType.LongPipe:
                case TileType.LongPipeVertical:
                case TileType.GlassPane:
                case TileType.HoleInFloor:
                case TileType.Robot:
                case TileType.None:
                case TileType.Floor:
                case TileType.WallUpper:
                case TileType.WallEdgeUL:
                case TileType.WallEdgeUR:
                case TileType.WallEdgeLL:
                case TileType.WallEdgeLR:
                case TileType.OpenedDoor:
                case TileType.PressurePlate:
                case TileType.PressurePlateWithBox:
                case TileType.StepOnSwitch:
                default:
                    throw new InvalidOperationException();
            }
        }
        private bool IsDestructible(TileType tile)
        {
            switch (tile)
            {
                case TileType.WoodBox:
                case TileType.Mainframe:
                case TileType.WoodCrate:
                case TileType.GlassPane:
                case TileType.LongPipe:
                case TileType.LongPipeVertical:
                    return true;
                case TileType.CutPipe:
                case TileType.MetalBox:
                case TileType.WallLower:
                case TileType.WallLowerFancy:
                case TileType.RightFacingWall:
                case TileType.LeftFacingWall:
                case TileType.BackFacingWall:
                case TileType.Door:
                case TileType.SlotForBox:
                case TileType.SlotForPipe:
                case TileType.LightSwitchWall:
                case TileType.Counter:
                case TileType.CounterEdgeLeft:
                case TileType.CounterEdgeRight:
                case TileType.CounterVerticalLeft:
                case TileType.CounterVerticalRight:
                case TileType.HoleInFloor:
                case TileType.Robot:
                case TileType.None:
                case TileType.Floor:
                case TileType.WallUpper:
                case TileType.WallEdgeUL:
                case TileType.WallEdgeUR:
                case TileType.WallEdgeLL:
                case TileType.WallEdgeLR:
                case TileType.OpenedDoor:
                case TileType.PressurePlate:
                case TileType.PressurePlateWithBox:
                case TileType.StepOnSwitch:
                default:
                    return false;
            }
        }

        private int GetFrame(TileType tile)
        {
            switch (tile)
            {
                case TileType.None:
                    return 0;
                case TileType.Floor:
                    return 1;
                case TileType.WallLower:
                    return 2;
                case TileType.WallLowerFancy:
                    return 3;
                case TileType.WallUpper:
                    return 4;
                case TileType.RightFacingWall:
                    return 5;
                case TileType.LeftFacingWall:
                    return 6;
                case TileType.WallEdgeUL:
                    return 7;
                case TileType.WallEdgeUR:
                    return 8;
                case TileType.WallEdgeLL:
                    return 9;
                case TileType.WallEdgeLR:
                    return 10;
                case TileType.BackFacingWall:
                    return 11;
                case TileType.Door:
                    return 12;
                case TileType.OpenedDoor:
                    return 13;
                case TileType.PressurePlate:
                    return 14;
                case TileType.PressurePlateWithBox:
                    throw new InvalidEnumArgumentException();
                case TileType.StepOnSwitch:
                    return 15;
                case TileType.SlotForBox:
                    return 16;
                case TileType.SlotForPipe:
                    return 17;
                case TileType.LightSwitchWall:
                    return 18;
                case TileType.Mainframe:
                    return 19;
                case TileType.Counter:
                    return 20;
                case TileType.CounterEdgeLeft:
                    return 21;
                case TileType.CounterEdgeRight:
                    return 22;
                case TileType.CounterVerticalLeft:
                    return 23;
                case TileType.CounterVerticalRight:
                    return 24;
                case TileType.WoodCrate:
                    return 0;
                case TileType.WoodBox:
                    return 1;
                case TileType.MetalBox:
                    return 2;
                case TileType.LongPipe:
                    return 4;
                case TileType.LongPipeVertical:
                    return 5;
                case TileType.CutPipe:
                    return 6;
                case TileType.GlassPane:
                    return 3;
                case TileType.HoleInFloor:
                    return 25;
                case TileType.Robot:
                    return 0;
                default:
                    break;
            }
            throw new NotImplementedException();
        }

        private SpriteSheet GetSpriteSheet(TileType tile)
        {
            switch (tile)
            {
                case TileType.None:
                case TileType.Floor:
                case TileType.WallLower:
                case TileType.WallLowerFancy:
                case TileType.WallUpper:
                case TileType.RightFacingWall:
                case TileType.LeftFacingWall:
                case TileType.WallEdgeUL:
                case TileType.WallEdgeUR:
                case TileType.WallEdgeLL:
                case TileType.WallEdgeLR:
                case TileType.BackFacingWall:
                case TileType.Door:
                case TileType.OpenedDoor:
                case TileType.PressurePlate:
                case TileType.PressurePlateWithBox:
                case TileType.StepOnSwitch:
                case TileType.SlotForBox:
                case TileType.SlotForPipe:
                case TileType.LightSwitchWall:
                case TileType.Mainframe:
                case TileType.Counter:
                case TileType.CounterEdgeLeft:
                case TileType.CounterEdgeRight:
                case TileType.CounterVerticalLeft:
                case TileType.CounterVerticalRight:
                case TileType.HoleInFloor:
                    return environmentSheet;
                case TileType.WoodCrate:
                case TileType.WoodBox:
                case TileType.MetalBox:
                case TileType.LongPipe:
                case TileType.LongPipeVertical:
                case TileType.CutPipe:
                case TileType.GlassPane:
                    return itemSheet;
                case TileType.Robot:
                    return robotSheet;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
