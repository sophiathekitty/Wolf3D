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
        // handles playing sounds
        //----------------------------------------------------------------------
        public class GameSoundPlayer
        {
            static List<IMySoundBlock> audioChannels = new List<IMySoundBlock>();
            static List<int> channelInUse = new List<int>();
            static List<string> availableSounds = new List<string>();
            public static void Init()
            {
                audioChannels = GridBlocks.soundBlocks;
                foreach (var channel in audioChannels)
                {
                    channel.Stop();
                    channelInUse.Add(0);
                }
                if (audioChannels.Count > 0)
                {
                    audioChannels[0].GetSounds(availableSounds);
                }
            }
            public static void Play(string sound)
            {
                if (audioChannels.Count == 0 || !availableSounds.Contains(sound)) return;
                int channel = 0;
                int max = channelInUse[0];
                for (int i = 0; i < channelInUse.Count; i++)
                {
                    if (channelInUse[i] < max)
                    {
                        channel = i;
                        break;
                    }
                }
                audioChannels[channel].Stop();
                channelInUse[channel] = 5;
                audioChannels[channel].SelectedSound = sound;
                audioChannels[channel].Play();
            }
            public static void Update()
            {
                for (int i = 0; i < channelInUse.Count; i++)
                {
                    if (channelInUse[i] > 0)
                    {
                        channelInUse[i]--;
                    }
                }
            }
        }
        //----------------------------------------------------------------------
    }
}
