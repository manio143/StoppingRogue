using Stride.Core.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace StoppingRogue.Levels
{
    public static class LevelReader
    {
        public static Level Read(string fileName)
        {
            return Read(File.ReadAllLines(fileName));
        }
        public static Level Read(string[] levelContents)
        {
            VerifyMagic(levelContents[0]);
            var userActions = GetUserActions(levelContents[1]);
            GetSize(levelContents[2], out var width, out var height);
            var tileMap = ReadTiles(width, height, levelContents[3..(4 + height)]);
            var switchLogic = ReadSwitchLogic(levelContents[3 + height]);
            var pattern = ReadPattern(levelContents[(4 + height)..]);

            var lvl = new Level
            {
                Width = width,
                Height = height,
                Tiles = tileMap,
                ActionPattern = pattern,
                UserActions = userActions,
                SwitchMapping = switchLogic,
            };

            return lvl;
        }

        private static Dictionary<Int2, (bool, Int2)> ReadSwitchLogic(string v)
        {
            var dict = new Dictionary<Int2, (bool, Int2)>();
            foreach(var (position,positive,door) in v.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(s => {
                s = s.Trim();
                var positive_ = s.Contains('+');
                var split = positive_ ? s.Split('+') : s.Split('-');
                return (ReadInt2(split[0].Trim()), positive_, ReadInt2(split[1].Trim()));
                }))
            {
                dict.Add(position, (positive, door));
            }
            return dict;
        }

        private static Int2 ReadInt2(string v)
        {
            var split = v.Split(',');
            var x = Int32.Parse(split[0].Substring(1).Trim());
            var y = Int32.Parse(split[1].Substring(0, split[1].Length - 1).Trim());
            return new Int2(x, y);
        }

        private static ActionType[] GetUserActions(string v)
        {
            return v.Split(",").Select(s => s.Trim())
                .Select(s => (ActionType)Enum.Parse(typeof(ActionType), s))
                .Concat(new ActionType[] { 0 })
                .ToArray();
        }

        private static Action[] ReadPattern(string[] v)
        {
            InitializeReadActionMap();
            return String.Concat(v.Select(s => s.Trim())).Select(c => readActionMap[c]).ToArray();
        }

        private static Dictionary<Char, Action> readActionMap;
        private static void InitializeReadActionMap()
        {
            if (readActionMap != null)
                return;

            readActionMap = new Dictionary<char, Action>();
            var enumFields = typeof(Action).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in enumFields)
            {
                var attr = field.GetCustomAttribute<ActionCharAttribute>();
                if (attr != null)
                {
                    readActionMap.Add(attr.Character, (Action)field.GetValue(null));
                }
            }
        }

        private static TileType[,] ReadTiles(int width, int height, string[] v)
        {
            InitializeReadTileMap();
            var tiles = new TileType[width, height];
            try
            {
                for (int line = 0; line < height; line++)
                {
                    for (int col = 0; col < width; col++)
                    {
                        tiles[col, line] = readTileMap[v[line][col]];
                    }
                }
                return tiles;
            } catch(IndexOutOfRangeException)
            {
                throw new InvalidDataException("TileMap doesn't correspond to width/height parameters.");
            }
        }

        private static Dictionary<Char, TileType> readTileMap;
        private static void InitializeReadTileMap()
        {
            if (readTileMap != null)
                return;

            readTileMap = new Dictionary<char, TileType>();
            var enumFields = typeof(TileType).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in enumFields)
            {
                var attr = field.GetCustomAttribute<TileCharAttribute>();
                if(attr != null)
                {
                    readTileMap.Add(attr.Character, (TileType)field.GetValue(null));
                }
            }
        }

        private static void GetSize(string v, out int width, out int height)
        {
            var split = v.Split(",").Select(s => s.Trim()).Select(s => Int32.Parse(s));
            width = split.First();
            height = split.Last();
        }

        private static void VerifyMagic(string v)
        {
            if (v.Trim() != "SRLD1337")
                throw new InvalidDataException("Invalid Magic string for level.");
        }
    }
}
