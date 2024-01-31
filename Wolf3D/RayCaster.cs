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
            RasterSprite renderSurfaceMap;
            RasterSprite renderSurface;
            TileMap tileMap;
            Player player;
            GameInput input;
            string renderCacheMap;
            string renderCache;
            bool loading = false;
            Vector2 loadTilePos = Vector2.Zero;

            bool drawMap = false;
            bool renderTextures = true;

            Ray2D ray;

            public RayCaster(IMyTextSurface drawingSurface) : base(drawingSurface)
            {
                BackgroundColor = Color.Black;
                loading = drawMap;
                renderSurfaceMap = new RasterSprite(Vector2.Zero, 0.070f*2f, new Vector2(128),"");
                if (drawMap) renderSurfaceMap.fillRGB(Color.Black);
                if (drawMap) AddSprite(renderSurfaceMap);
                renderSurface = new RasterSprite(Vector2.Zero, 0.070f * 4f, new Vector2(64),"");
                renderSurface.fillRGB(Color.Black);
                renderCache = renderSurface.Data;
                AddSprite(renderSurface);
                tileMap = new TileMap(GridDB.GetData("Map",0));
                if (drawMap) tileMap.DrawGrid(renderSurfaceMap);
                renderCacheMap = renderSurfaceMap.Data;
                renderSurface.Visible = !drawMap;
                renderSurfaceMap.Visible = drawMap;
                input = new GameInput(GridBlocks.GetPlayer());
                player = new Player(input,tileMap);
                player.Position = new Vector2(60);
                player.Angle = 360-45;
                ray = new Ray2D(player.Position, player.Angle, tileMap);
            }
            public override void Update()
            {
                // game logic goes here
                player.Update();
            }
            public override void Draw()
            {
                // drawing code goes here
                renderSurfaceMap.Data = renderCacheMap;
                renderSurface.Data = renderCache;
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
                else if(input.PlayerPresent)
                {
                    if(renderSurfaceMap.Visible) player.Draw(renderSurfaceMap);
                    ray.Position = player.Position;
                    ray.Angle = player.Angle - 30 % 360;
                    int j = 0;
                    float step = 60.0f / 64.0f;
                    for (int i = 0; i < 64; i++)
                    {
                        ray.CastRay();
                        if (drawMap) ray.Draw(renderSurfaceMap);
                        else if (renderTextures) ray.DrawVirticalTexture(renderSurface, i, player.Angle);
                        else ray.DrawVirticalLine(renderSurface, i, player.Angle);
                        ray.Angle += step;
                    }
                    //ray.CastRay();
                    //ray.Draw(renderSurface);
                }
                //GridInfo.Echo("Draw? " + GridInfo.RunCount++);
                base.Draw();
            }
        }
        //----------------------------------------------------------------------
    }
}
