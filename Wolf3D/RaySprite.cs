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
using static IngameScript.Program;

namespace IngameScript
{
    partial class Program
    {
        //----------------------------------------------------------------------
        // RaySprite
        //----------------------------------------------------------------------
        public class RaySprite
        {
            public static List<string[]> SpriteData = new List<string[]>();
            public static void LoadSprites()
            {
                RasterSprite sprite = new RasterSprite(Vector2.Zero, 0.070f, new Vector2(64,144), GridDB.GetData("Textures",0,false));
                for(int y = 0; y < 144; y+=16)
                {
                    for(int x = 0; x < 64; x+=16)
                    {
                        SpriteData.Add(RayTexture.ParseTextureData(sprite.getPixels(x,y,16,16)));
                        //GridInfo.Echo("Sprite-"+ SpriteData.Count+": " + SpriteData[SpriteData.Count - 1][0].Length);
                    }
                }
            }
            public Vector2 Position;
            public int id;
            public bool Visible = true;
            public bool Flipped = true;
            public bool IsVisible;
            public RaySprite(Vector2 position, int id)
            {
                Position = position;
                this.id = id;
            }
            //public int ScreenX {get; private set;}
            public int FirstX = 0;  
            public int LastX = 0;
            public virtual void Draw(RasterSprite surface, Player player)
            {
                IsVisible = false;
                if(Visible)
                {
                    // find the angle between the player and the sprite
                    float radians = (float)Math.Atan2(Position.Y - player.Position.Y, Position.X - player.Position.X); // looking left
                    float angle = radians * (float)(180 / Math.PI) % 360; //(float)(radians - player.Rotation);
                    float playerAngle = player.Angle;
                    if (player.Position.X < Position.X) 
                    {
                        playerAngle = player.Angle % 360;
                        if (playerAngle < 0) playerAngle += 360;
                        if (playerAngle > 180) playerAngle -= 360;
                    }
                    float degrees = (float)(angle - playerAngle);
                    //if (player.Position.Y < Position.Y)
                    //{
                        if (degrees < 0) degrees += 360;
                        if (degrees > 180) degrees -= 360;
                    //}
                    float step = 60.0f / 64.0f; // angle per pixel
                    // -30 to 30 degrees is 0 to 64 pixels for x
                    // convert angle to x coordinate
                    int x = (int)((degrees + 30) / step);
                    //ScreenX = x;
                    //surface.setPixelRGB(x, (int)(surface.Size.Y/2), 255, 255, 0);
                    // find the distance between the player and the sprite
                    float distance = (float)Math.Sqrt(Math.Pow(Position.X - player.Position.X, 2) + Math.Pow(Position.Y - player.Position.Y, 2));
                    // calculate scale
                    int scale = (int)(SpriteData[id].Length * surface.Size.Y / distance);
                    if (scale > surface.Size.Y * 1.2f) scale = (int)(surface.Size.Y * 1.2f);
                    else if (scale <= 0) return;
                    int y = (int)(surface.Size.Y / 2 - scale / 2);
                    if(y < 0) y = 0;
                    x = (int)(x - scale / 2);
                    float percent = (float)scale / (float)SpriteData[id].Length;
                    int c = 0;
                    FirstX = -1;
                    LastX = -1;
                    for (int i = x; i < x + scale; i++)
                    {
                        if (i >= 0 && i < surface.Size.X && Ray2D.depth[i] > distance)
                        {
                            if(Flipped) surface.drawPixelColumnWithIgnore(i, y, ScaleColumn((int)(((scale-1)-c) / percent), scale));
                            else surface.drawPixelColumnWithIgnore(i, y, ScaleColumn((int)(c / percent), scale));
                            IsVisible = true;
                            if(FirstX == -1) FirstX = i;
                            else LastX = i;
                        }
                        c++;
                    }
                }
            }
            public string ScaleColumn(int index, int height)
            {
                // scale virtically by scale
                float scale = (float)height / (float)SpriteData[id].Length;
                string[] lines = SpriteData[id][index % SpriteData[id].Length].Split('\n');
                string[] scaledLines = new string[height];
                for (int y = 0; y < scaledLines.Length; y++)
                {
                    scaledLines[y] = lines[(int)(y / scale)];
                }
                return string.Join("\n", scaledLines);
            }
        }
        //----------------------------------------------------------------------
    }
}
