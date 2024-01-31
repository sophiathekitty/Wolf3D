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
        // GridDB
        //----------------------------------------------------------------------
        public class GridDB
        {
            static Dictionary<string,List<IMyTextPanel>> db_blocks = new Dictionary<string, List<IMyTextPanel>>();
            public static void Init()
            {
                foreach(IMyTextPanel block in GridBlocks.textPanels)
                {
                    string name = block.CustomName;
                    if (name.StartsWith("DB:"))
                    {
                        name = name.Substring(3);
                        string[] parts = name.Split('.');
                        name = parts[0];
                        if (!db_blocks.ContainsKey(name)) db_blocks.Add(name, new List<IMyTextPanel>());
                        db_blocks[name].Add(block);
                    } 
                    else if(name =="Main Display")
                    {
                        if (!db_blocks.ContainsKey("Main")) db_blocks.Add("Main", new List<IMyTextPanel>());
                        db_blocks["Main"].Add(block);
                    } 
                    else if(name == "Sign") 
                    {
                        if (!db_blocks.ContainsKey("Sign")) db_blocks.Add("Sign", new List<IMyTextPanel>());
                        db_blocks["Sign"].Add(block);
                    }
                }
            }
            public static string GetData(string name,int index, bool CustomData = true)
            {
                if (db_blocks.ContainsKey(name))
                {
                    if (index < db_blocks[name].Count)
                    {
                        if (CustomData) return db_blocks[name][index].CustomData;
                        else return db_blocks[name][index].GetText();
                    }
                }
                return "";
            }
            public static void SetData(string name, int index, string data, bool CustomData = true)
            {
                if (db_blocks.ContainsKey(name))
                {
                    if (index < db_blocks[name].Count)
                    {
                        if (CustomData) db_blocks[name][index].CustomData = data;
                        else db_blocks[name][index].WriteText(data);
                    }
                }
            }
            public static string GetData(string address) // Name.Index.Data or Name.Index.Text
            {
                string[] parts = address.Split('.');
                if (parts.Length == 3)
                {
                    string name = parts[0];
                    int index = 0;
                    if (int.TryParse(parts[1], out index))
                    {
                        if (parts[2] == "Data") return GetData(name, index, true);
                        else if (parts[2] == "Text") return GetData(name, index, false);
                    }
                } else if(parts.Length == 2)
                {
                    string name = parts[0];
                    int index = 0;
                    if (int.TryParse(parts[1], out index))
                    {
                        return GetData(name, index, true);
                    } 
                    else
                    {
                        if (parts[1] == "Data") return GetData(name, 0, true);
                        else if (parts[1] == "Text") return GetData(name, 0, false);
                    }
                }
                return "";
            }
            public static void SetData(string address, string data) // Name.Index.Data or Name.Index.Text
            {
                string[] parts = address.Split('.');
                if (parts.Length == 3)
                {
                    string name = parts[0];
                    int index = 0;
                    if (int.TryParse(parts[1], out index))
                    {
                        if (parts[2] == "Data") SetData(name, index, data, true);
                        else if (parts[2] == "Text") SetData(name, index, data, false);
                    }
                }
                else if (parts.Length == 2)
                {
                    string name = parts[0];
                    int index = 0;
                    if (int.TryParse(parts[1], out index))
                    {
                        SetData(name, index, data, true);
                    }
                    else
                    {
                        if (parts[1] == "Data") SetData(name, 0, data, true);
                        else if (parts[1] == "Text") SetData(name, 0, data, false);
                    }
                }
            }
        }
        //----------------------------------------------------------------------
    }
}
