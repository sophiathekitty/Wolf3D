using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        //----------------------------------------------------------------------
        // TileMap
        //----------------------------------------------------------------------
        public class TileMap
        {
            string[] map;
            public string Map { get { return string.Join("\n", map); } set { map = value.Split('\n'); } }
            public Vector2 Size { get { if (map.Length > 0) return new Vector2(map[0].Length, map.Length); else return Vector2.Zero;  } }
            public Vector2 TileSize = new Vector2(16);
            public TileMap(string map)
            {
                Map = map;
            }
            public char GetTile(Vector2 position)
            {
                if (position.X < 0 || position.Y < 0 || position.X >= Size.X || position.Y >= Size.Y) return ' ';
                return map[(int)position.Y][(int)position.X];
            }
            public char GetTileAtWorldPosition(Vector2 position)
            {
                char tile = GetTile(position / TileSize);
                if (tile != ' ') tile = GetTile(Vector2.Max(Vector2.Zero, position / TileSize));
                return tile;
            }
            public void DrawGrid(RasterSprite renderSurface)
            {
                for (int x = 0; x < Size.X; x++)
                {
                    renderSurface.drawLineRGB(new Vector2(x * TileSize.X, 0), new Vector2(x * TileSize.X, Size.Y * TileSize.Y), Color.Purple);
                }
                for (int y = 0; y < Size.Y; y++)
                {
                    renderSurface.drawLineRGB(new Vector2(0, y * TileSize.Y), new Vector2(Size.X * TileSize.X, y * TileSize.Y), Color.Blue);
                }
            }
            public void FillTile(RasterSprite renderSurface, Vector2 position, Color color)
            {
                renderSurface.fillRectRGB(position * TileSize+Vector2.One, position * TileSize + TileSize-Vector2.One, color);
            }
        }
        //----------------------------------------------------------------------
    }
}
