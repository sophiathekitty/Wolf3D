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
        // RayCaster
        //----------------------------------------------------------------------
        public class RayCaster : Screen
        {
            public static ScreenSprite DebugDisplay;
            public static ScreenSprite KeysDisplay;
            public static ScreenSprite GoldDisplay;
            public static ScreenSprite HealthDisplay;
            public static ScreenSprite AmmoDisplay;
            public static ScreenSprite ButtonPrompt;
            public static ScreenSprite GameOverDisplay;
            //RasterSprite renderSurfaceMap;
            RasterSprite renderSurface;
            public static TileMap tileMap;
            public static Player player;
            int weaponIndex = 1;
            List<Weapon> weapons;
            GameInput input;
            //string renderCacheMap;
            string renderCache;
            //bool loading = false;
            //Vector2 loadTilePos = Vector2.Zero;

            //bool drawMap = false;
            bool renderTextures = true;

            int respawnDelay = 0;
            int respawnDelayMax = 500;

            Ray2D ray;
            //----------------------------------------------------------------------
            // Constructor
            //----------------------------------------------------------------------
            public RayCaster(IMyTextSurface drawingSurface) : base(drawingSurface)
            {
                BackgroundColor = Color.Black;
                //loading = drawMap;
                //renderSurfaceMap = new RasterSprite(Vector2.Zero, 0.070f*2f, new Vector2(128),"");
                //if (drawMap) renderSurfaceMap.fillRGB(Color.Black);
                //if (drawMap) AddSprite(renderSurfaceMap);
                renderSurface = new RasterSprite(Vector2.Zero, 0.072f * 4f, new Vector2(64),"");
                //renderSurface = new RasterSprite(Vector2.Zero, 0.072f * 3f, new Vector2(72,56),"");
                //renderSurface.fillRGB(Color.Black);
                renderSurface.fillHalfRGB(50,50,50);
                renderSurface.fillHalfRGB(100,100,100);
                renderCache = renderSurface.Data;
                AddSprite(renderSurface);
                //tileMap = new TileMap(GridDB.GetData("Map",0));
                tileMap = new TileMap();
                //if (drawMap) tileMap.DrawGrid(renderSurfaceMap);
                //renderCacheMap = renderSurfaceMap.Data;
                //renderSurface.Visible = !drawMap;
                //renderSurfaceMap.Visible = drawMap;
                input = new GameInput(GridBlocks.GetPlayer());
                player = new Player(input,tileMap);
                player.Position = tileMap.Start; //= new Vector2(60);
                player.Angle = tileMap.StartAngle; //= 360-45;
                ray = new Ray2D(player.Position, player.Angle, tileMap);
                weapons = Weapon.LoadWeapons(this);
                weapons[0].Visible = false;
                DebugDisplay = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopLeft,Vector2.Zero,1f,Vector2.Zero,Color.White,"Monospace","",TextAlignment.LEFT,SpriteType.TEXT);
                AddSprite(DebugDisplay);
                KeysDisplay = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopRight,Vector2.Zero,1f,Vector2.Zero,Color.White,"Monospace","",TextAlignment.RIGHT,SpriteType.TEXT);
                AddSprite(KeysDisplay);
                GoldDisplay = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopRight,new Vector2(0,30),1f,Vector2.Zero,Color.White,"Monospace","",TextAlignment.RIGHT,SpriteType.TEXT);
                AddSprite(GoldDisplay);
                HealthDisplay = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.BottomLeft,new Vector2(0,-30),1f,Vector2.Zero,Color.White,"Monospace","",TextAlignment.LEFT,SpriteType.TEXT);
                AddSprite(HealthDisplay);
                AmmoDisplay = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.BottomRight,new Vector2(0,-30),1f,Vector2.Zero,Color.White,"Monospace","",TextAlignment.RIGHT,SpriteType.TEXT);
                AddSprite(AmmoDisplay);
                ButtonPrompt = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.Center,Vector2.Zero,1f,Vector2.Zero,Color.White,"Monospace","",TextAlignment.CENTER,SpriteType.TEXT);
                AddSprite(ButtonPrompt);
                GameOverDisplay = new ScreenSprite(ScreenSprite.ScreenSpriteAnchor.TopCenter,new Vector2(0,30),1.75f,Vector2.Zero,Color.White,"Monospace","The\nSkeleton Club\nJust Got\nA New Member",TextAlignment.CENTER,SpriteType.TEXT);
                AddSprite(GameOverDisplay);
                GameOverDisplay.Visible = false;
            }
            //----------------------------------------------------------------------
            // Main
            //----------------------------------------------------------------------
            public override void Main(string argument)
            {
                //DebugDisplay.Data = "";
                Update();
                Draw();
            }
            //----------------------------------------------------------------------
            // Update
            //----------------------------------------------------------------------
            public override void Update()
            {
                // game logic goes here
                if (!Player.Dead)
                {
                    // player input
                    player.Update();
                    if (input.EPressed)
                    {
                        if (!tileMap.OpenDoorAtWorldPosition(player.InteractPoint))
                        {
                            int exit = tileMap.OpenGoalDoorAtWorldPosition(player.InteractPoint);
                            if(exit > 0)
                            {
                                int level = TileMap.level + exit;
                                if(level < 6)
                                {
                                    tileMap.LoadMap(level);
                                    player.Position = tileMap.Start;
                                    player.Angle = tileMap.StartAngle;
                                }
                            }
                        }
                    }
                    if (input.QPressed)
                    {
                        weapons[weaponIndex].Visible = false;
                        weaponIndex = (weaponIndex + 1) % weapons.Count;
                        weapons[weaponIndex].Visible = true;
                    }
                    if (input.SpacePressed)
                    {
                        weapons[weaponIndex].Attack();
                        if (weaponIndex > Weapon.Ammo)
                        {
                            weapons[weaponIndex].Visible = false;
                            weaponIndex = 0;
                            weapons[weaponIndex].Visible = true;
                        }
                    }
                }
                tileMap.UpdateSprites();
            }
            //----------------------------------------------------------------------
            // Draw
            //----------------------------------------------------------------------
            public override void Draw()
            {
                // drawing code goes here
                //renderSurfaceMap.Data = renderCacheMap;
                /*
                if (loading && drawMap)
                {
                    char tile = tileMap.GetTile(loadTilePos);
                    if (RayTexture.TEXTURES.ContainsKey(tile))
                    {
                        GridInfo.Echo("Loading tile " + loadTilePos + "(" + tileMap.Size + ")");
                        tileMap.FillTile(renderSurfaceMap, loadTilePos, RayTexture.TEXTURES[tile].Color);
                    }
                    loadTilePos.X++;
                    if (loadTilePos.X >= tileMap.Size.X)
                    {
                        loadTilePos.X = 0;
                        loadTilePos.Y++;
                    }
                    if (loadTilePos.Y >= tileMap.Size.Y)
                    {
                        loading = false;
                        //renderSurfaceMap.Visible = false;
                        ///renderSurface.Visible = true;
                    }
                    renderCacheMap = renderSurfaceMap.Data;
                }
                else 
                */
                if(input.PlayerPresent && !Player.Dead)
                {
                    renderSurface.Data = renderCache;
                    renderSurface.Color = Color.White;
                    //if(renderSurfaceMap.Visible) player.Draw(renderSurfaceMap);
                    ray.Position = player.Position;
                    ray.Angle = player.Angle - 30 % 360;
                    int j = 0;
                    float step = 60.0f / renderSurface.Size.X;
                    for (int i = 0; i < renderSurface.Size.X; i++)
                    {
                        ray.CastRay();
                        //if (drawMap) ray.Draw(renderSurfaceMap);
                        //else
                        if (renderTextures) ray.DrawVirticalTexture(renderSurface, i, player.Angle);
                        else ray.DrawVirticalLine(renderSurface, i, player.Angle);
                        ray.Angle += step;
                    }
                    if(/*(!drawMap) && */renderTextures) tileMap.DrawSprites(renderSurface, player);
                    //ray.CastRay();
                    //ray.Draw(renderSurface);
                    HealthDisplay.Data = "HP: " + (int)Player.Health + "%";
                    GoldDisplay.Data = "Gold: " + Player.Gold;
                    KeysDisplay.Data = "Keys: " + Player.Keys;
                    AmmoDisplay.Data = "Ammo: " + Weapon.Ammo;
                    ButtonPrompt.Data = tileMap.DoorPromptatWorldPosition(player.InteractPoint);
                }
                else if (Player.Dead)
                {
                    if (!GameOverDisplay.Visible)
                    {
                        respawnDelay = respawnDelayMax;
                    }
                    GameOverDisplay.Visible = true;
                    renderSurface.Color = Color.DarkRed;
                    if(respawnDelay-- <= 0)
                    {
                        GameOverDisplay.Visible = false;
                        tileMap.ReloadMap();
                        player.Position = tileMap.Start;
                        player.Angle = tileMap.StartAngle;
                    }
                    HealthDisplay.Data = "";
                    GoldDisplay.Data = "";
                    KeysDisplay.Data = "";
                    AmmoDisplay.Data = "";
                }
                else
                {
                    HealthDisplay.Data = "";
                    GoldDisplay.Data = "";
                    KeysDisplay.Data = "";
                    AmmoDisplay.Data = "";
                }
                base.Draw();
            }
        }
        //----------------------------------------------------------------------
    }
}
