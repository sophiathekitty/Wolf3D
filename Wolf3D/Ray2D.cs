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
        // Ray2D
        //----------------------------------------------------------------------
        public class Ray2D
        {
            public static float[] depth = new float[64];
            Vector2 position;
            Vector2 horizontal = Vector2.Zero;
            Vector2 vertical = Vector2.Zero;
            TileMap tileMap;
            Color rayColor = Color.Purple;
            char hTile = ' ';
            char vTile = ' ';
            public Vector2 Position { get { return position; } set { position = value; } }
            public float Rotation; // radians
            // angle in degrees
            public float Angle { get { return Rotation * 180.0f / (float)Math.PI; } set { Rotation = value * (float)Math.PI / 180.0f; } }
            public Ray2D(Vector2 position, float angle, TileMap tileMap)
            {
                this.position = position;
                this.Angle = angle;
                this.tileMap = tileMap;
                CastRay();
                GridInfo.Echo("Ray2D: " + position.ToString() + " " + vertical.ToString());
            }
            public void CastRay()
            {
                int dof = 0; // depth of field
                int dofMax = 32;
                //hTile = ' ';
                //vTile = ' ';
                Vector2 step = Vector2.Zero;
                // find the first vertical intesection with the tilemap grid lines (use: TileSize)                
                float tan = (float)Math.Tan(Rotation);
                if(Math.Cos(Rotation) > 0.001)
                {
                    vertical.X = (float)Math.Floor(position.X / tileMap.TileSize.X) * tileMap.TileSize.X + tileMap.TileSize.X + 0.1f;
                    vertical.Y = position.Y + (vertical.X - position.X) * tan;
                    step.X = tileMap.TileSize.X;
                    step.Y = step.X * tan; // look left
                    rayColor = Color.Red;
                }
                else if(Math.Cos(Rotation) < -0.001)
                {
                    vertical.X = (float)Math.Floor(position.X / tileMap.TileSize.X) * tileMap.TileSize.X - 0.1f;
                    vertical.Y = position.Y + (vertical.X - position.X) * tan;
                    step.X = -tileMap.TileSize.X;
                    step.Y = step.X * tan; // look right
                    rayColor = Color.Purple;
                }
                else
                {
                    vertical = Position; // no vertical intersection
                    dof = dofMax;
                }
                while(dof < dofMax && vertical.X > 0 && vertical.Y > 0 && vertical.X < tileMap.Size.X * tileMap.TileSize.X && vertical.Y < tileMap.Size.Y * tileMap.TileSize.Y)
                {
                    char tile = tileMap.GetTileAtWorldPosition(vertical);
                    //vTile = tile;
                    if (RayTexture.TEXTURES.ContainsKey(tile))
                    {
                        vTile = tile;
                        break;
                    }
                    vertical += step;
                    dof++;
                }
                // find the first horizontal intesection with the tilemap grid lines (use: TileSize)
                dof = 0;
                tan = 1.0f / tan;
                if(Math.Sin(Rotation) > 0.001)
                {
                    horizontal.Y = (float)Math.Floor(position.Y / tileMap.TileSize.Y) * tileMap.TileSize.Y + tileMap.TileSize.Y + 0.1f;
                    horizontal.X = position.X + (horizontal.Y - position.Y) * tan;
                    step.Y = tileMap.TileSize.Y;
                    step.X = step.Y * tan; // look up
                    rayColor = Color.Blue;
                }
                else if(Math.Sin(Rotation) < -0.001)
                {
                    horizontal.Y = (float)Math.Floor(position.Y / tileMap.TileSize.Y) * tileMap.TileSize.Y - 0.1f;
                    horizontal.X = position.X + (horizontal.Y - position.Y) * tan;
                    step.Y = -tileMap.TileSize.Y;
                    step.X = step.Y * tan; // look down
                    rayColor = Color.Pink;
                }
                else
                {
                    horizontal = Position; // no horizontal intersection
                    dof = dofMax;
                }
                while(dof < dofMax && horizontal.X > 0 && horizontal.Y > 0 && horizontal.X < tileMap.Size.X * tileMap.TileSize.X && horizontal.Y < tileMap.Size.Y * tileMap.TileSize.Y)
                {
                    char tile = tileMap.GetTileAtWorldPosition(horizontal);
                    //hTile = tile;
                    if (RayTexture.TEXTURES.ContainsKey(tile))
                    {
                        hTile = tile;
                        break;
                    }
                    horizontal += step;
                    dof++;
                }
            }
            public void Draw(RasterSprite renderSurface)
            {
                if(Vector2.Distance(horizontal,position) < Vector2.Distance(vertical,position))
                {
                    if(RayTexture.TEXTURES.ContainsKey(hTile)) rayColor = RayTexture.TEXTURES[hTile].horizontal;
                    else rayColor = Color.Red;
                    renderSurface.drawLineRGB(position,horizontal, rayColor);
                }
                else
                {
                    if(RayTexture.TEXTURES.ContainsKey(vTile)) rayColor = RayTexture.TEXTURES[vTile].vertical;
                    else rayColor = Color.White;
                    renderSurface.drawLineRGB(position,vertical, rayColor);
                }
            }
            public void DrawVirticalLine(RasterSprite renderSurface, int x, float angle)
            {
                float distance = 0;
                if (Vector2.Distance(horizontal, position) < Vector2.Distance(vertical, position))
                {
                    distance = Vector2.Distance(horizontal, position);
                    if (RayTexture.TEXTURES.ContainsKey(hTile)) rayColor = RayTexture.TEXTURES[hTile].horizontal;
                    else rayColor = Color.Red;
                }
                else
                {
                    distance = Vector2.Distance(vertical, position);
                    if (RayTexture.TEXTURES.ContainsKey(vTile)) rayColor = RayTexture.TEXTURES[vTile].vertical;
                    else rayColor = Color.White;
                }
                distance *= (float)Math.Cos((angle - Angle % 360) * Math.PI / 180.0f);
                int height = (int)((renderSurface.Size.Y * tileMap.TileSize.Y) / distance);
                if (height > renderSurface.Size.Y) height = (int)renderSurface.Size.Y;
                if (height < 0) height = 0;
                int y = (int)(renderSurface.Size.Y / 2.0f - height / 2.0f);
                renderSurface.drawLineRGB(new Vector2(x, y), new Vector2(x, y + height), rayColor);
            }
            public void DrawVirticalTexture(RasterSprite renderSurface, int x, float angle)
            {
                float distance = 0;
                if (Vector2.Distance(horizontal, position) < Vector2.Distance(vertical, position))
                {
                    distance = Vector2.Distance(horizontal, position);
                }
                else
                {
                    distance = Vector2.Distance(vertical, position);
                }
                distance *= (float)Math.Cos((angle - Angle % 360) * Math.PI / 180.0f);
                int height = (int)((renderSurface.Size.Y * tileMap.TileSize.Y) / distance);
                if (height > renderSurface.Size.Y*2.5f) height = (int)(renderSurface.Size.Y*2.5f);
                if (height < 0) height = 0;
                int y = (int)(renderSurface.Size.Y / 2.0f - height / 2.0f);
                if (Vector2.Distance(horizontal, position) < Vector2.Distance(vertical, position))
                {
                    // figure out which column to draw (what subpixel of the grid did horizontal hit?)
                    int subX = (int)(horizontal.X % tileMap.TileSize.X);
                    if(horizontal.Y > position.Y) subX = (int)(tileMap.TileSize.X - subX - 1);
                    string column = RayTexture.TEXTURES[hTile].GetHorizontalData(subX, height);
                    if(y < 0)
                    {
                        string[] lines = column.Split('\n');
                        int size = (int)Math.Min(renderSurface.Size.Y-2, lines.Length + y);
                        //GridInfo.Echo("hTrimming " +x +", " + y + " lines "+lines.Length + " size " + size);
                        string[] lines2 = lines.Skip(-y).ToArray();
                        //GridInfo.Echo("hTrimming2 " + x + ", " + y + " lines " + lines2.Length + " size " + size);
                        column = string.Join("\n", lines2);
                        y = 0;
                    }
                    if(!renderSurface.drawPixels(x, y, column))
                    {
                        string[] lines = column.Split('\n');
                        //GridInfo.Echo("hDrawPixels failed "+x+","+y+" "+column.Length);
                    }
                } 
                else
                {
                    // figure out which column to draw (what subpixel of the grid did vertical hit?)
                    int subY = (int)(vertical.Y % tileMap.TileSize.Y);
                    if (vertical.X < position.X) subY = (int)(tileMap.TileSize.Y - subY - 1);
                    string column = RayTexture.TEXTURES[vTile].GetVerticalData(subY, height);
                    if(y < 0)
                    {
                        string[] lines = column.Split('\n');
                        int size = (int)Math.Min(renderSurface.Size.Y-2, lines.Length+y);
                        //GridInfo.Echo("vTrimming "+ x + ", " + y + " lines "+lines.Length + " size " + size);
                        string[] lines2 = lines.Skip(-y).ToArray();
                        column = string.Join("\n", lines2);
                        y = 0;
                    }
                    if(!renderSurface.drawPixels(x, y, column))
                    {
                        string[] lines = column.Split('\n');
                        //GridInfo.Echo("vDrawPixels failed "+x+","+y+" " + lines[0].Length +", " +lines.Length);
                    }
                }
                depth[x] = distance;
            }
        }
    }
}
