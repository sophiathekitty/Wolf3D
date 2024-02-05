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
        // RayTexture
        //----------------------------------------------------------------------
        public class RayTexture
        {
            public static Dictionary<char,RayTexture> TEXTURES = new Dictionary<char, RayTexture>();
            public static char Door = 'm';
            public static char LockedDoor = 'n';
            public static char SecretDoor = 'q';
            public static char Goal = 'o';
            public static char SecretGoal = 'p';
            public static void LoadTextures()
            {
                RasterSprite sprite = new RasterSprite(Vector2.Zero, 0.1f, new Vector2(32,240), GridDB.GetData("Textures",0));
                LoadTexture('a', sprite, 0, Color.Blue, Color.DarkBlue);
                LoadTexture('b', sprite, 16, Color.Green, Color.DarkGreen);
                LoadTexture('c', sprite, 32, Color.Yellow, Color.DarkGoldenrod);
                LoadTexture('d', sprite, 48, Color.Red, Color.DarkRed);
                LoadTexture('e', sprite, 64, Color.Purple, Color.DarkMagenta);
                LoadTexture('f', sprite, 80, Color.Cyan, Color.DarkCyan);
                LoadTexture('g', sprite, 96, Color.Orange, Color.DarkOrange);
                LoadTexture('h', sprite, 112, Color.White, Color.Gray);
                LoadTexture('i', sprite, 128, Color.LightBlue, Color.DarkBlue);
                LoadTexture('j', sprite, 144, Color.LightGreen, Color.DarkGreen);
                LoadTexture('k', sprite, 160, Color.LightYellow, Color.DarkGoldenrod);
                LoadTexture('l', sprite, 176, Color.LightPink, Color.DarkRed);
                LoadTexture('q', sprite, 176, Color.LightPink, Color.DarkRed);
                LoadTexture('m', sprite, 192, Color.Pink, Color.DarkMagenta);
                LoadTexture('n', sprite, 208, Color.LightCyan, Color.DarkCyan);
                LoadTexture('o', sprite, 224, Color.Orange, Color.DarkOrange);
                LoadTexture('p', sprite, 224, Color.Orange, Color.DarkOrange);
            }
            static void LoadTexture(char id, RasterSprite sprite, int y, Color hColor, Color vColor)
            {
                string left = sprite.getPixels(0, y, 16, 16);
                string right = sprite.getPixels(16, y, 16, 16);
                //GridInfo.Echo("Loading Texture " + id +" left: "+left.Length +" right: "+right.Length);
                TEXTURES.Add(id, new RayTexture(hColor,vColor, left, right));
            }
            public static string[] ParseTextureData(string data)
            {
                string[] lines = data.Split('\n');
                string[] columns = new string[lines[0].Length];
                for (int y = 0; y < lines.Length; y++)
                {
                    for (int x = 0; x < lines[y].Length; x++)
                    {
                        if (y == 0) columns[x] = "";
                        else columns[x] += "\n";
                        columns[x] += lines[y][x];
                    }
                }
                return columns;
            }
            //----------------------------------------------------------------------
            // Instance Stuff
            //----------------------------------------------------------------------
            public Color horizontal;
            public Color vertical;
            string[] horizontalData;
            string[] verticalData;
            public Color Color { get { return horizontal; } set { horizontal = value; vertical = value;  } }
            public RayTexture(Color horizontal, Color vertical, string horizontalData, string verticalData)
            {
                this.horizontal = horizontal;
                this.vertical = vertical;
                this.horizontalData = ParseTextureData(horizontalData);
                this.verticalData = ParseTextureData(verticalData);
            }
            public RayTexture(Color horizontal, Color vertical, string[] horizontalData, string[] verticalData)
            {
                this.horizontal = horizontal;
                this.vertical = vertical;
                this.horizontalData = horizontalData;
                this.verticalData = verticalData;
            }

            public string GetHorizontalData(int index, float scale)
            {
                // scale virtically by scale
                string[] lines = horizontalData[index % horizontalData.Length].Split('\n');
                string[] scaledLines = new string[(int)(lines.Length * scale)];
                for (int y = 0; y < scaledLines.Length; y++)
                {
                    scaledLines[y] = lines[(int)(y / scale)];
                }
                return string.Join("\n", scaledLines);
            }
            public string GetHorizontalData(int index, int height)
            {
                //return horizontalData[index % horizontalData.Length];
                // scale virtically by scale
                float scale = (float)height / (float)horizontalData.Length;
                string[] lines = horizontalData[index % horizontalData.Length].Split('\n');
                string[] scaledLines = new string[height];
                for (int y = 0; y < scaledLines.Length; y++)
                {
                    scaledLines[y] = lines[(int)(y / scale)];
                }
                return string.Join("\n", scaledLines);
            }
            public string GetVerticalData(int index, float scale)
            {
                // scale virtically by scale
                string[] lines = verticalData[index % verticalData.Length].Split('\n');
                string[] scaledLines = new string[(int)(lines.Length * scale)];
                for (int y = 0; y < scaledLines.Length; y++)
                {
                    scaledLines[y] = lines[(int)(y / scale)];
                }
                return string.Join("\n", scaledLines);
            }
            public string GetVerticalData(int index, int height)
            {
                //return verticalData[index % verticalData.Length];
                // scale virtically by scale
                float scale = (float)height / (float)verticalData.Length;
                string[] lines = verticalData[index % verticalData.Length].Split('\n');
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
