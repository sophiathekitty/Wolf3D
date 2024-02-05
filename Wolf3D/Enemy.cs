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
        // Enemy
        //----------------------------------------------------------------------
        public class Enemy : RaySprite
        {
            public float Rotation = 0; // radians
            public float Angle { get { return Rotation * (float)(180 / Math.PI) % 360; } set { Rotation = value * (float)(Math.PI / 180); } }
            public float Speed = 0f;
            float MaxSpeed = 1f;
            public bool Dead = false;
            int animationFrame = 0;
            int animationDelay = 0;
            static int animationDelayMax = 10;
            static int animationFrames = 3;
            static float damage = 10;
            Vector2 target = Vector2.Zero;
            int relativeAngleToPlayer 
            { 
                get 
                { 
                    float angle = (float)(Angle - RayCaster.player.Angle + 405) % 360;
                    // remap to 0-3 for the 4 quadrants
                    return (int)(angle / 90);
                } 
            }
            int squaredSeeRange = 16384;//4096;
            bool canSeePlayer
            {
                get
                {
                    if (Dead || relativeAngleToPlayer == 0 || Vector2.DistanceSquared(Position,RayCaster.player.Position) > squaredSeeRange) return false;
                    //GridInfo.Echo("\nCanSeePlayer? " + relativeAngleToPlayer);
                    //RayCaster.DebugDisplay.Data += "CanSeePlayer? " + relativeAngleToPlayer+"\n";
                    // check if there's any tiles between the player and the enemy
                    // find the position half way between the player and the enemy
                    Vector2 diff = Position - RayCaster.player.Position;
                    Vector2 step = diff/4;

                    Vector2 pos = (Position - step);
                    for (int i = 0; i < 3; i++)
                    {
                        char tile = RayCaster.tileMap.GetTileAtWorldPosition(pos);
                        if (RayTexture.TEXTURES.ContainsKey(tile))
                        {
                            //GridInfo.Echo("Can't see player, tile |"+tile+"| at " + Vector2.Floor(pos/RayCaster.tileMap.TileSize));
                            //RayCaster.DebugDisplay.Data += "Can't see player, tile |" + tile + "| at " + Vector2.Floor(pos / RayCaster.tileMap.TileSize) + "\n";
                            return false;
                        }
                        pos -= step;
                    }
                    //GridInfo.Echo("Can see player");
                    return true;
                }
            }
            public float Damage { get { return Math.Max(0,damage-Vector2.Distance(Position,RayCaster.player.Position)); } }
            public Enemy(Vector2 position, float angle) : base(position, 8)
            {
                Angle = angle * 90;
            }
            public void Update()
            {
                if (Dead) return;
                if (canSeePlayer) target = RayCaster.player.Position;
                if (target != Vector2.Zero)
                {
                    // rotate to look at target
                    float target_rotation = (float)Math.Atan2(target.Y - Position.Y, target.X - Position.X);
                    // lerp Rotation (radians) to target_rotation
                    float delta = target_rotation - Rotation;
                    if (delta > Math.PI) delta -= (float)(Math.PI * 2);
                    else if (delta < -Math.PI) delta += (float)(Math.PI * 2);
                    Rotation += delta * 0.01f;
                    if (Vector2.DistanceSquared(Position, target) < 1) { Speed = 0; target = Vector2.Zero; }
                    else
                    {
                        // have speed be relative to difference in angle to target
                        float diff = Math.Abs(delta);
                        //RayCaster.DebugDisplay.Data += "Diff: " + diff + "\n";
                        Speed = MaxSpeed - diff;
                        if(Speed < 0) Speed = 0;
                    }
                    //RayCaster.DebugDisplay.Data += "Speed: " + Speed + "\n";
                }
                // if the distance to target is less than 1, stop moving
                if (Speed > 0)
                {
                    // move forward
                    Vector2 move = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * Speed;
                    Vector2 newPos = Position + move;
                    if (!RayTexture.TEXTURES.ContainsKey(RayCaster.tileMap.GetTileAtWorldPosition(newPos)))
                    {
                        Position = newPos;
                    }
                    else Speed = 0;
                }
                else
                { 
                    // if we're not moving, rotate a random amount
                    if (random.Next(0, 100) < 5) Angle = (Angle + random.Next(-90, 90)) % 360;
                }
                float damage = Damage;
                if (damage > 0)
                {
                    RayCaster.player.Damage(damage);
                }
            }
            static Random random = new Random();
            public override void Draw(RasterSprite surface, Player player)
            {
                // figure out which cell to draw
                if (animationDelay++ > animationDelayMax)
                {
                    animationDelay = 0;
                    if(Dead)
                    {
                        if (animationFrame < animationFrames-1) animationFrame++;
                    }
                    else animationFrame = (animationFrame + 1) % animationFrames;
                }
                int dir = relativeAngleToPlayer;
                // id = 8 is the first cell of the enemy idle looking forward
                // id = 12 is the first cell of the enemy idle looking left (right is also 16 but flipped)
                // id = 16 is the first cell of the enemy idle looking away
                // dir 0 is away, 1 is right, 2 is forward, 3 is left
                if(Dead) id = 32 + animationFrame;
                else
                {
                    int speedOffset = Speed > 0 ? 12 : 0;
                    if (dir == 2) id = 8 + animationFrame + speedOffset;
                    else if (dir == 1) id = 12 + animationFrame + speedOffset;
                    else if (dir == 3) id = 12 + animationFrame + speedOffset;
                    else if (dir == 0) id = 16 + animationFrame + speedOffset;
                }
                Flipped = dir == 3;
                base.Draw(surface, player);
            }
        }
        //----------------------------------------------------------------------
    }
}
