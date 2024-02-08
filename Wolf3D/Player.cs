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
using System.Threading;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.GameServices;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        //----------------------------------------------------------------------
        // Player
        //----------------------------------------------------------------------
        public class Player
        {
            GameInput input;
            TileMap tileMap;
            public static int Keys = 0;
            public static float Health = 50;
            public static float MaxHealth = 100;
            public static int Gold = 0;
            public static bool Dead { get { return Health <= 0; } }
            public Vector2 Position;
            public Vector2 InteractPoint { get { return Position + new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * radius; } }
            public float Rotation; // radians
            // angle in degrees
            public float Angle { get { return Rotation * 180.0f / (float)Math.PI; } set { Rotation = value * (float)Math.PI / 180.0f; } }
            public float FOV = 1.0f;
            public float MoveSpeed = 1.0f;
            public float RotationSpeed = 0.1f;
            float radius = 6.66f;
            int invincibility = 0;
            int invincibilityMax = 100;
            public Player(GameInput input, TileMap tileMap)
            {
                this.input = input;
                this.tileMap = tileMap;
            }
            public void Update()
            {
                if(!input.PlayerPresent) return;
                Angle = (Angle + input.Mouse.X * RotationSpeed) % 360;
                Vector2 move = Vector2.Zero;
                // move forward
                if(input.W) move += new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
                // move backward
                else if(input.S) move -= new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation));
                // strafe right
                if(input.D) move += new Vector2((float)Math.Cos(Rotation + Math.PI / 2.0f), (float)Math.Sin(Rotation + Math.PI / 2.0f));
                // strafe left
                else if(input.A) move += new Vector2((float)Math.Cos(Rotation - Math.PI / 2.0f), (float)Math.Sin(Rotation - Math.PI / 2.0f));
                if(move.LengthSquared() > 0.001f)
                {
                    move.Normalize();
                    move *= MoveSpeed;
                    Vector2 newPosition = Position + move;
                    if(!RayTexture.TEXTURES.ContainsKey(tileMap.GetTileAtWorldPosition(newPosition)) && !tileMap.IsBlocking(newPosition))
                    {
                        Position = newPosition;
                    }
                    else
                    {
                        Position -= move;
                    }
                }
                // check for items
                tileMap.PickUpItem(Position);
                invincibility = Math.Max(0,invincibility-1);
                if(invincibility == 0) RayCaster.HealthDisplay.Color = Color.White;
            }
            public void Damage(float amount)
            {
                if(invincibility > 0) return;
                Health = Math.Max(0,Health-amount);
                invincibility = invincibilityMax;
                RayCaster.HealthDisplay.Color = Color.Red;
                GameSoundPlayer.Play("WolfPain");
            }
            public void Draw(RasterSprite renderSurface)
            {
                renderSurface.drawCircleRGB((int)Position.X,(int)Position.Y,(int)radius, 255,255,0);
                renderSurface.drawLineRGB(Position, Position + new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation))*5, Color.Yellow);
            }
        }
        //----------------------------------------------------------------------
    }
}
