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
            List<RaySprite> itemSprites = new List<RaySprite>();
            List<RaySprite> blocking = new List<RaySprite>();
            List<RaySprite> renderSprites = new List<RaySprite>();
            public string name;
            public string Map { get { return string.Join("\n", map); } set { map = value.Split('\n'); } }
            public Vector2 Size { get { if (map.Length > 0) return new Vector2(map[0].Length, map.Length); else return Vector2.Zero;  } }
            public Vector2 TileSize = new Vector2(16);
            public Vector2 Start = Vector2.One;
            public float StartAngle = 0;
            public TileMap(string map)
            {
                LoadMap(map);
            }
            public void LoadMap(string map)
            {
                string[] parts = map.Split('|');
                Map = parts[0];
                if (parts.Length > 1) // player info
                {
                    // load player position and angle
                    string[] p = parts[1].Split(',');
                    if (p.Length == 4)
                    {
                        name = p[0];
                        int x = 0, y = 0;
                        if (int.TryParse(p[1], out x) && int.TryParse(p[2], out y))
                        {
                            Start = new Vector2(x, y) * TileSize - TileSize / 2;
                            int angle = 0;
                            if (int.TryParse(p[3], out angle))
                            {
                                StartAngle = angle * 90;
                            }
                        }
                    }
                }
                if(parts.Length > 2) // items
                {
                    GridInfo.Echo("Loading item sprites");
                    string[] sprites = parts[2].Split('\n');
                    foreach(string sprite in sprites)
                    {
                        string[] s = sprite.Split(',');
                        if(s.Length == 3)
                        {
                            int id = 0, x = 0, y = 0;
                            if (int.TryParse(s[0],out id) && int.TryParse(s[1],out x) && int.TryParse(s[2],out y))
                            {
                                RaySprite raySprite = new RaySprite(new Vector2(x,y) * TileSize - TileSize / 2, id);
                                itemSprites.Add(raySprite);
                                renderSprites.Add(raySprite);
                            }
                        }
                    }
                }
                if(parts.Length > 3) // blocking
                {
                    GridInfo.Echo("Loading blocking sprites");
                    string[] sprites = parts[3].Split('\n');
                    foreach (string sprite in sprites)
                    {
                        string[] s = sprite.Split(',');
                        if (s.Length == 3)
                        {
                            int id = 0, x = 0, y = 0;
                            if (int.TryParse(s[0], out id) && int.TryParse(s[1], out x) && int.TryParse(s[2], out y))
                            {
                                RaySprite raySprite = new RaySprite(new Vector2(x, y) * TileSize - TileSize / 2, id);
                                blocking.Add(raySprite);
                                renderSprites.Add(raySprite);
                            }
                        }
                    }
                }
            }
            public char GetTile(Vector2 position)
            {
                if (position.X < 0 || position.Y < 0 || position.X >= Size.X || position.Y >= Size.Y) return ' ';
                return map[(int)position.Y][(int)position.X];
            }
            public char GetTileAtWorldPosition(Vector2 position)
            {
                char tile = GetTile(position / TileSize);
                if (tile == ' ') tile = GetTile(Vector2.Max(Vector2.Zero, position / TileSize));
                return tile;
            }
            public bool SetTile(Vector2 position,char tile, char originalTile)
            {
                if (GetTile(position) != originalTile) return false;
                if (position.X < 0 || position.Y < 0 || position.X >= Size.X || position.Y >= Size.Y) return false;
                map[(int)position.Y] = map[(int)position.Y].Substring(0, (int)position.X) + tile + map[(int)position.Y].Substring((int)position.X + 1);
                return true;
            }
            public bool SetTileAtWorldPosition(Vector2 position, char tile, char originaTile)
            {
                if(!SetTile(position / TileSize, tile,originaTile)) return SetTile(Vector2.Max(Vector2.Zero, position / TileSize), tile,originaTile);
                return true;
            }
            public bool OpenDoorAtWorldPosition(Vector2 position)
            {
                char door = RayTexture.Door;
                char tile = GetTileAtWorldPosition(position);
                if (tile == door) return SetTileAtWorldPosition(position, door.ToString().ToUpper()[0], door);
                return OpenLockedDoorAtWorldPosition(position);
            }
            public bool OpenLockedDoorAtWorldPosition(Vector2 position)
            {
                char door = RayTexture.LockedDoor;
                char tile = GetTileAtWorldPosition(position);
                if (tile == door && Player.Keys > 0)
                {
                    Player.Keys--;
                    return SetTileAtWorldPosition(position, door.ToString().ToUpper()[0], door);
                }
                return false;
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
            public void DrawSprites(RasterSprite renderSurface,Player player)
            {
                //GridInfo.Echo("Drawing " + itemSprites.Count + " sprites");
                // sort sprites by distance to player (furthest first)
                renderSprites.Sort((a, b) => (int)(b.Position - player.Position).LengthSquared() - (int)(a.Position - player.Position).LengthSquared());
                int i = 0;
                foreach(RaySprite sprite in renderSprites)
                {
                    //if(i++ > 10) break;
                    sprite.Draw(renderSurface,player);
                }
            }
            public bool IsBlocking(Vector2 position)
            {
                foreach(RaySprite sprite in blocking)
                {
                    if (Vector2.DistanceSquared(position,sprite.Position) < 16) return true;
                }
                return false;
            }
            public void PickUpItem(Vector2 position)
            {
                for(int i = 0; i < itemSprites.Count; i++)
                {
                    if (Vector2.DistanceSquared(position, itemSprites[i].Position) < 16)
                    {
                        bool remove = true;
                        if (itemSprites[i].id == 0)
                        {
                            if (Player.Health < Player.MaxHealth)
                            {
                                Player.Health += 25;
                                if (Player.Health > Player.MaxHealth) Player.Health = Player.MaxHealth;
                            }
                            else remove = false;
                        }
                        else if (itemSprites[i].id == 1) Player.Keys++;
                        else if (itemSprites[i].id == 4) Player.Gold += 15;
                        if (remove)
                        {
                            renderSprites.Remove(itemSprites[i]);
                            itemSprites.RemoveAt(i);
                        }
                        return;
                    }
                }
            }
        }
        //----------------------------------------------------------------------
    }
}
