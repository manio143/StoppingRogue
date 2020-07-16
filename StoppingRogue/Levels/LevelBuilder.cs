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
    /// <summary>
    /// This class is responsible for creating a <see cref="Scene"/> from a <see cref="Level"/> description.
    /// </summary>
    public class LevelBuilder
    {
        private SpriteSheet environmentSheet;
        private SpriteSheet robotSheet;
        private SpriteSheet itemSheet;

        /// <summary>
        /// Initializes new builder.
        /// </summary>
        /// <param name="environmentSheet">Environment tiles</param>
        /// <param name="robotSheet">Robot sprites</param>
        /// <param name="itemSheet">Item sprites</param>
        public LevelBuilder(SpriteSheet environmentSheet, SpriteSheet robotSheet, SpriteSheet itemSheet, ActionController actionController)
        {
            this.environmentSheet = environmentSheet;
            this.robotSheet = robotSheet;
            this.itemSheet = itemSheet;
        }

        /// <summary>
        /// Generates entities in a new scene.
        /// </summary>
        /// <param name="level">Level description</param>
        /// <param name="robot">Robot entity</param>
        /// <returns>A scene to be added to the root scene.</returns>
        public Scene Build(Level level, out Entity robot)
        {
            var scene = new Scene();

            // Create collections to later link switches with doors
            var switchSetups = new List<(Entity, bool, Int2)>();
            var doors = new Dictionary<Int2, Entity>();

            // out params have to be initialized
            robot = null;

            for (int line = 0; line < level.Tiles.GetLength(1); line++)
                for (int col = 0; col < level.Tiles.GetLength(0); col++)
                {
                    var tile = level.Tiles[col, line];

                    if (tile == TileType.None)
                        continue; // don't create entities for none tiles

                    if (tile == TileType.PressurePlateWithBox
                        || tile == TileType.PressurePlateWithMetalBox)
                    {
                        // The tile that describes two entities
                        var plate = InitializeEntity(line, col, TileType.PressurePlate);
                        var boxTile = tile == TileType.PressurePlateWithBox
                            ? TileType.WoodBox : TileType.MetalBox;
                        var box = InitializeEntity(line, col, boxTile);

                        scene.Entities.Add(plate);
                        scene.Entities.Add(box);

                        TryAddSwitchOrDoor(level, switchSetups, doors, line, col, plate);
                    }
                    else
                    {
                        // If entity can move away (or get destroyed),
                        // put floor underneath it
                        if (IsMovable(tile))
                        {
                            var floor = InitializeEntity(line, col, TileType.Floor);
                            scene.Entities.Add(floor);
                        }

                        Entity entity = InitializeEntity(line, col, tile);

                        scene.Entities.Add(entity);

                        TryAddSwitchOrDoor(level, switchSetups, doors, line, col, entity);

                        if (tile == TileType.Robot)
                            robot = entity;
                    }
                }

            ConnectSwitchesToDoors(switchSetups, doors);

            return scene;
        }

        private static void ConnectSwitchesToDoors(List<(Entity, bool, Int2)> switchSetups, Dictionary<Int2, Entity> doors)
        {
            foreach (var (swe, pos, dp) in switchSetups)
            {
                var swit = swe.Get<SwitchComponent>() ?? throw new InvalidDataException();

                var door = doors[dp].Get<DoorComponent>() ?? throw new InvalidDataException();

                swit.Doors.Add((pos, door));
            }
        }

        /// <summary>
        /// Checks if the entity is on the switch mapping and populates <paramref name="switchSetups"/> and <paramref name="doors"/> accordingly.
        /// </summary>
        private static void TryAddSwitchOrDoor(Level level, List<(Entity, bool, Int2)> switchSetups, Dictionary<Int2, Entity> doors, int line, int col, Entity entity)
        {
            var vec = new Int2(col, line);
            if (level.SwitchMapping.ContainsKey(vec))
            {
                var dops = level.SwitchMapping[vec];
                switchSetups.AddRange(dops.Select(dp => (entity, dp.Item1, dp.Item2)));
            }
            else if (level.SwitchMapping.Any(kvp => kvp.Value.Any(dp => dp.Item2 == vec)))
            {
                doors.Add(vec, entity);
            }
        }

        private Entity InitializeEntity(int line, int col, TileType tile)
        {
            var entity = new Entity();
            SetPosition(entity, col, line, tile);
            entity.Name = $"({col}, {line}) {tile}";
            AddComponents(entity, tile);
            return entity;
        }

        /// <summary>
        /// Can it be move away from its current position?
        /// </summary>
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
                case TileType.Door:
                case TileType.OpenedDoor:
                case TileType.Robot:
                    return true;
                default:
                    return false;
            }
        }

        private void SetPosition(Entity entity, int col, int line, TileType tile)
        {
            var transform = entity.GetOrCreate<TransformComponent>();
            transform.Position = new Vector3(col, -line, ZPos(tile));

            // Each tile is 48px wide. At pixel to unit scale 1/100.
            // This way each tile is one unit wide in game.
            transform.Scale = (Vector3)new Vector2(1 / 0.48f);
        }

        /// <summary>
        /// Adds components to the <paramref name="entity"/> based on tile type.
        /// </summary>
        /// <remarks>This should be abstracted into somethign prettier.</remarks>
        private void AddComponents(Entity entity, TileType tile)
        {
            AddSprite(entity, tile);

            if (tile == TileType.Robot)
            {
                var rc = entity.GetOrCreate<RobotController>();
                var rl = entity.GetOrCreate<RobotLight>();
                var rls = entity.GetOrCreate<RobotLaser>();
                entity.GetOrCreate<RobotHolder>();
                
                rl.robotSpriteSheet = robotSheet;
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
                rb.CanCollideWith = CollisionFilterGroupFlags.DefaultFilter
                    | CollisionFilterGroupFlags.SensorTrigger
                    // Hole
                    | CollisionFilterGroupFlags.CustomFilter3;
            }
            if(tile == TileType.HoleInFloor)
            {
                var rb = entity.GetOrCreate<RigidbodyComponent>();
                rb.ColliderShapes.Add(new BoxColliderShapeDesc()
                {
                    Is2D = true,
                    Size = new Vector3(0.45f, 0.45f, 0),
                });
                rb.RigidBodyType = RigidBodyTypes.Kinematic;
                rb.CollisionGroup = CollisionFilterGroups.CustomFilter3;
                rb.CanCollideWith = CollisionFilterGroupFlags.DefaultFilter;
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
                // CustomFilter1 reacts with RobotLight
                rb.CollisionGroup = CollisionFilterGroups.CustomFilter1;
                rb.CanCollideWith = CollisionFilterGroupFlags.CustomFilter1;

                var ls = switchEntity.GetOrCreate<LightSwitch>();
                var task = entity.GetOrCreate<TaskComponent>();

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
                // CustomFilter2 reacts with raycast in RobotHolder
                rb.CollisionGroup = CollisionFilterGroups.CustomFilter2;
                rb.CanCollideWith = CollisionFilterGroupFlags.CustomFilter2;
                entity.AddChild(holdableEntity);

                var item = entity.GetOrCreate<ItemComponent>();
                item.ItemType = GetItemType(tile);
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
                        var col = (int)entity.Transform.Position.X;
                        var line = -(int)entity.Transform.Position.Y;
                        var cutPipe = InitializeEntity(line, col, TileType.CutPipe);

                        entity.Scene.Entities.Add(cutPipe);
                        entity.Scene = null; // remove long pipe from scene
                    };
                }
                else
                {
                    expl.PostExplosion += () =>
                    {
                        entity.Scene = null;
                    };
                }
            }
            if (tile == TileType.PressurePlate || tile == TileType.StepOnSwitch)
            {
                var switchComp = entity.GetOrCreate<SwitchComponent>();
                if (tile == TileType.PressurePlate)
                    entity.GetOrCreate<PressurePlate>();
                else
                    entity.GetOrCreate<StepOnSwitch>();

                var rb = entity.GetOrCreate<RigidbodyComponent>();
                rb.ColliderShapes.Add(new BoxColliderShapeDesc()
                {
                    Is2D = true,
                    Size = new Vector3(0.45f, 0.45f, 0),
                });
                rb.RigidBodyType = RigidBodyTypes.Kinematic;
                // Sensor trigger allows RobotController to step on it
                rb.CollisionGroup = CollisionFilterGroups.SensorTrigger;
                rb.CanCollideWith = CollisionFilterGroupFlags.DefaultFilter;
            }
            if (tile == TileType.SlotForBox || tile == TileType.SlotForPipe)
            {
                var task = entity.GetOrCreate<TaskComponent>();
                task.Type = tile == TileType.SlotForBox ? TaskType.FillBoxSlot : TaskType.FillPipeSlot;

                var slot = entity.GetOrCreate<SlotComponent>();
                slot.taskComponent = task;
                slot.ItemType = tile == TileType.SlotForBox ? Item.MetalBox : Item.CutPipe;
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

        /// <summary>
        /// Some entities should always be displayed above others.
        /// </summary>
        public float ZPos(TileType tile)
        {
            switch (tile)
            {
                case TileType.WallUpper:
                case TileType.OpenedDoor:
                    return 0.003f;
                case TileType.Robot:
                    return 0.002f;
                case TileType.WoodBox:
                case TileType.MetalBox:
                case TileType.CutPipe:
                    return 0.001f;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// A tile with a collider blocks robot from entering it.
        /// </summary>
        private bool HasCollider(TileType tile)
        {
            switch (tile)
            {
                case TileType.WallLower:
                case TileType.WallLowerFancy:
                case TileType.WallUpper:
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
                case TileType.Robot:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Items can be picked up.
        /// </summary>
        private bool IsItem(TileType tile)
        {
            switch (tile)
            {
                case TileType.WoodBox:
                case TileType.MetalBox:
                case TileType.CutPipe:
                    return true;
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
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Destructable entities can be destroyed with the laser.
        /// </summary>
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
                default:
                    return false;
            }
        }

        /// <summary>
        /// Tile frame in its sprite sheet.
        /// </summary>
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

        /// <summary>
        /// Tiles sprite sheet.
        /// </summary>
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
