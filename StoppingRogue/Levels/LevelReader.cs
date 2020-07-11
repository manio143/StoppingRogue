﻿using System;
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
            GetSize(levelContents[1], out var width, out var height);
            var tileMap = ReadTiles(width, height, levelContents[2..(3 + height)]);
            var pattern = ReadPattern(levelContents[(2 + height)..]);

            var lvl = new Level
            {
                Width = width,
                Height = height,
                Tiles = tileMap,
                ActionPattern = pattern
            };

            return lvl;
        }

        private static ActionType[] ReadPattern(string[] v)
        {
            InitializeReadActionMap();
            return String.Concat(v.Select(s => s.Trim())).Select(c => readActionMap[c]).ToArray();
        }

        private static Dictionary<Char, ActionType> readActionMap;
        private static void InitializeReadActionMap()
        {
            if (readActionMap != null)
                return;

            readActionMap = new Dictionary<char, ActionType>();
            var enumFields = typeof(ActionType).GetFields(BindingFlags.Public | BindingFlags.Static);
            foreach (var field in enumFields)
            {
                var attr = field.GetCustomAttribute<ActionCharAttribute>();
                if (attr != null)
                {
                    readActionMap.Add(attr.Character, (ActionType)field.GetValue(null));
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
            if (v != "SRLD1337")
                throw new InvalidDataException("Invalid Magic string for level.");
        }
    }
}