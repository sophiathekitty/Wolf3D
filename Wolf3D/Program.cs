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
    partial class Program : MyGridProgram
    {
        //======================================================================
        RayCaster rayCaster;
        public Program()
        {
            Echo("Booting Wolf3D");
            GridInfo.Init("Wolf3D",this);
            Echo("GridInfo Init");
            GridBlocks.InitBlocks(GridTerminalSystem);
            Echo("GridBlocks Init");
            GridDB.Init();
            Echo("GridDB Init");
            RayTexture.LoadTextures();
            Echo("RayTexture Loaded");
            RaySprite.LoadSprites();
            Echo("Sprites Loaded");
            GameSoundPlayer.Init();
            rayCaster = new RayCaster(GridBlocks.GetTextSurface("Main Display"));
            Echo("RayCaster Init");
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            Echo("Wolf3D Ready");
        }

        public void Save()
        {
            GridInfo.Save();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            rayCaster.Main(argument);
        }
        //======================================================================
    }
}
