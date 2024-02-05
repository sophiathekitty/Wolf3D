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
        // Weapon
        //----------------------------------------------------------------------
        // WIC v1.2.5.0 - Dither mode: (None) - 125x48 px
        public class Weapon : RasterSprite
        {
            public static List<Weapon> LoadWeapons(Screen screen)
            {
                List<Weapon> weapons = new List<Weapon>();
                RasterSprite source = new RasterSprite(Vector2.Zero, 0.070f, new Vector2(125,48), GridDB.GetData("Main",0));
                weapons.Add(new Weapon(screen,10,10,false,new Vector2(125,24),source.getPixels(0,0,125,24),SCALE));
                weapons.Add(new Weapon(screen,5,60,true,new Vector2(125,24),source.getPixels(0,24,125,24),SCALE));
                return weapons;
            }
            static float SCALE = 0.2f;
            public static int Ammo = 10;
            public static int MaxAmmo = 99;
            List<string> cells = new List<string>();
            bool attacking = false;
            bool needsAmmo = false;
            int index = 0;
            int delay = 0;
            int delayMax = 5;
            int attackRadius = 10;
            float attackRange = 5;

            public Weapon(Screen screen,int radius, float range, bool ammo, Vector2 size, string data, float scale = 0.1f, int cells = 5) : base(Vector2.Zero, scale, size, data)
            {
                Vector2 CellSize = new Vector2(size.X/cells, size.Y);
                needsAmmo = ammo;
                for(int i = 0; i < cells; i++)
                {
                    this.cells.Add(getPixels(i*(int)CellSize.X,0,(int)CellSize.X,(int)CellSize.Y).Replace(IGNORE.ToString(),INVISIBLE));

                }
                Data = this.cells[0];
                Size = CellSize;
                Vector2 ScreenSize = PixelToScreen(CellSize);
                Position = new Vector2(screen.Size.X/2 - ScreenSize.X/2, screen.Size.Y - ScreenSize.Y);
                attackRadius = radius;
                attackRange = range;
                screen.AddSprite(this);
            }
            public void Attack()
            {
                if(!Visible) return;
                if (!attacking) 
                {
                    if(needsAmmo && Ammo <= 0) return;
                    if(needsAmmo) Ammo = Math.Max(0,Ammo-1);
                    //see if we killed any enemies
                    foreach(Enemy enemy in TileMap.enemies)
                    {
                        if (enemy.Dead || !enemy.IsVisible || Vector2.DistanceSquared(enemy.Position, RayCaster.player.Position) > attackRange * attackRange) continue;
                        if (enemy.LastX > 32 - attackRadius && enemy.FirstX < 32 + attackRadius)
                        {
                            enemy.Dead = true;
                            break;
                        }
                    }
                }
                attacking = true;
            }
            public override MySprite ToMySprite(RectangleF _viewport)
            {
                if(attacking)
                {
                    //RayCaster.DebugDisplay.Data += "Attacking\n";
                    if (delay++ > delayMax)
                    {
                        delay = 0;
                        if(index++ < cells.Count-1)
                        {
                            Data = cells[index];
                        }
                        else
                        {
                            attacking = false;
                            index = 0;
                            Data = cells[0];
                        }
                    }
                }
                return base.ToMySprite(_viewport);
            }
        }
        //----------------------------------------------------------------------
    }
}
