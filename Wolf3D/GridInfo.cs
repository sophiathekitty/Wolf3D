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
        //---------------------------------------------------------------//
        // Grid Info                                                     //
        //---------------------------------------------------------------//
        // holds some basic info about the grid and other useful stuff   //
        // to have globally can also report changes to variables and     //
        // send them to other grids.                                     //
        //---------------------------------------------------------------//
        // add to Program():                                             //
        // GridInfo.Init("Program Name",GridTerminalSystem,IGC,Me,Echo); //
        // if(Storage != "") GridInfo.Load(Storage);                     //
        //                                                               //
        // add to Save():                                                //
        // Storage = GridInfo.Save();                                    //
        //                                                               //
        // add to Main():                                                //
        // GridInfo.CheckMessages();                                     //
        //                                                               //
        // usage:                                                        //
        // GridInfo.SetVar("varname","value");                           //
        // GridInfo.GetVarAs<T>("varname","optionalDefault");            //
        //                                                               //
        // change listener:                                              //
        // GridInfo.AddVarChangedHandler("varname",MyHandler);           //
        //                                                               //
        // change broadcasting:                                          //
        // GridInfo.AddChangeBroadcaster("program","varname");           //
        // GridInfo.AddChangeUnicaster(igcAddress,"varname");            //
        // GridInfo.AddBroadcastListener("key");                         //
        // GridInfo.AddVarBroadcastListener("program");                  //
        //---------------------------------------------------------------//
        public class GridInfo
        {
            public static long RunCount = 0; // to store how many times the script has run since compiling
            public static string ProgramName = "Program"; // the name of the program
            public static IMyGridTerminalSystem GridTerminalSystem; // so it can be globally available
            public static IMyIntergridCommunicationSystem IGC; // so it can be globally available
            public static IMyProgrammableBlock Me; // so it can be globally available... lol
            public static Action<string> EchoAction; // EchoAction?.Invoke("hello");
            private static IMyBroadcastListener broadcastListener; // so it can be globally available
            private static List<IMyBroadcastListener> listeners = new List<IMyBroadcastListener>(); // so it can be globally available
            private static List<IMyBroadcastListener> varListeners = new List<IMyBroadcastListener>(); // so it can be globally available
            private static string bound_vars = ""; // a list of vars that have been bound to the grid
            private static Dictionary<string, string> broadcast_vars = new Dictionary<string, string>(); // a list of vars that have been bound to the grid
            private static Dictionary<string, long> unicast_vars = new Dictionary<string, long>(); // a list of vars that have been bound to the grid
            public static bool handleUnicastMessages = false;
            private static Program program;
            public static void Echo(string message)
            {
                EchoAction?.Invoke(message);
            }
            public static Dictionary<string, string> GridVars = new Dictionary<string, string>();
            //-------------------------------------------//
            // setup GridInfo                            //
            //-------------------------------------------//
            public static void Init(string name, IMyGridTerminalSystem gts, IMyIntergridCommunicationSystem igc, IMyProgrammableBlock me, Action<string> echo)
            {
                ProgramName = name;
                GridTerminalSystem = gts;
                IGC = igc;
                Me = me;
                EchoAction = echo;
                broadcastListener = IGC.RegisterBroadcastListener(ProgramName);
                if (name != "") Me.CustomName = "Program: " + ProgramName + " @" + IGC.Me.ToString();
            }
            public static void Init(string name, Program program)
            {
                GridInfo.program = program;
                Init(name, program.GridTerminalSystem, program.IGC, program.Me, program.Echo);
                Load(program.Storage);
            }
            public static void Init(string name, Program program, string storage)
            {
                GridInfo.program = program;
                Init(name, program.GridTerminalSystem, program.IGC, program.Me, program.Echo);
                Load(storage);
            }
            public static void Init(Program program)
            {
                GridInfo.program = program;
                Init("", program.GridTerminalSystem, program.IGC, program.Me, program.Echo);
                Load(program.Storage);
            }
            //-------------------------------------------//
            // handle broadcast messages                 //
            //-------------------------------------------//
            public static List<MyIGCMessage> CheckMessages()
            {
                List<MyIGCMessage> messages = new List<MyIGCMessage>();
                while (broadcastListener.HasPendingMessage)
                {
                    MyIGCMessage message = broadcastListener.AcceptMessage();
                    Echo("GridInfo-" + message.Tag + ": " + message.As<string>());
                    /*
                    string[] data = message.As<string>().Split('║');
                    if (data.Length == 2)
                    {
                        SetVar(data[0], data[1]);
                    }
                    else messages.Add(message);
                    */
                    if (!checkForVar(message)) messages.Add(message);
                }
                while (IGC.UnicastListener.HasPendingMessage)
                {
                    messages.Add(IGC.UnicastListener.AcceptMessage());
                }
                foreach (IMyBroadcastListener listener in listeners)
                {
                    while (listener.HasPendingMessage)
                    {
                        messages.Add(listener.AcceptMessage());
                    }
                }
                foreach (IMyBroadcastListener listener in varListeners)
                {
                    while (listener.HasPendingMessage)
                    {
                        MyIGCMessage message = listener.AcceptMessage();
                        Echo(message.Tag + ": " + message.As<string>());
                        checkForVar(message);
                    }
                }
                return messages;
            }
            static bool checkForVar(MyIGCMessage message)
            {
                string[] data = message.As<string>().Split('║');
                if (data.Length == 2)
                {
                    GridInfo.Echo("GridInfo-VarFound! " + data[0] + ": " + data[1]);
                    SetVar(data[0], data[1], false);
                    return true;
                }
                return false;
            }
            public static IMyBroadcastListener AddBroadcastListener(string name)
            {
                IMyBroadcastListener listener = IGC.RegisterBroadcastListener(name);
                listeners.Add(listener);
                return listener;
            }
            public static void AddVarBroadcastListener(string name)
            {
                IMyBroadcastListener listener = IGC.RegisterBroadcastListener(name);
                varListeners.Add(listener);
            }
            //-------------------------------------------//
            // Get a var as a specific type of variable  //
            //                                           //
            // key - the id of the variable to get       //
            // defaultValue - the value to return if     //
            //                the variable doesn't exist //
            //-------------------------------------------//    
            public static T GetVarAs<T>(string key, T defaultValue = default(T))
            {
                if (!GridVars.ContainsKey(key)) return defaultValue; //(T)Convert.ChangeType(null,typeof(T));
                return (T)Convert.ChangeType(GridVars[key], typeof(T));
            }
            public static Vector3D GetVarAsVector3D(string key, Vector3D defaultValue = default(Vector3D))
            {
                if (!GridVars.ContainsKey(key)) return defaultValue;
                string[] data = GridVars[key].Split(',');
                if (data.Length == 3)
                {
                    double x = double.Parse(data[0]);
                    double y = double.Parse(data[1]);
                    double z = double.Parse(data[2]);
                    return new Vector3D(x, y, z);
                }
                return defaultValue;
            }
            //-------------------------------------------//
            // set a grid info var                       //
            //                                           //
            // key - the id of the variable to set       //
            // value - the value (converted to a string) //
            //-------------------------------------------//
            public static void SetVar(string key, string value, bool send = true)
            {
                if (GridVars.ContainsKey(key)) GridVars[key] = value;
                else GridVars.Add(key, value);
                if (bound_vars.Contains(key + "║")) OnVarChanged(key, value);
                if (send && broadcast_vars.ContainsKey(key)) IGC.SendBroadcastMessage(broadcast_vars[key], key + "║" + value);
                if (send && unicast_vars.ContainsKey(key)) IGC.SendUnicastMessage(unicast_vars[key], key, value);
            }
            public static void SetVar(string key, Vector3D value)
            {
                SetVar(key, value.X.ToString() + "," + value.Y.ToString() + "," + value.Z.ToString());
            }
            //------------------------------------------------------------//
            // converts the grid info vars to a string to save in Storage //
            //------------------------------------------------------------//
            public static string Save()
            {
                StringBuilder storage = new StringBuilder();
                foreach (KeyValuePair<string, string> var in GridVars)
                {
                    storage.Append(var.Key + "║" + var.Value + "\n");
                }
                if (program != null) program.Storage = storage.ToString();
                return storage.ToString();
            }
            //----------------------------------------------//
            // parse the Storage string into grid info vars //
            //----------------------------------------------//
            public static void Load(string storage)
            {
                string[] lines = storage.Split('\n');
                foreach (string line in lines)
                {
                    string[] var = line.Split('║');
                    if (var.Length == 2)
                    {
                        GridVars.Add(var[0], var[1]);
                    }
                }
            }
            //----------------------------------//
            // event for when a var is changed  //
            //----------------------------------//
            public static event Action<string, string> VarChanged;
            private static void OnVarChanged(string key, string value)
            {
                VarChanged?.Invoke(key, value);
            }
            public static void AddChangeListener(string key, Action<string, string> handler)
            {
                bound_vars += key + "║";
                VarChanged += handler;
            }
            public static void AddChangeListener(string key)
            {
                bound_vars += key + "║";
            }
            // send changes to a prog by its name
            public static void AddChangeBroadcaster(string progName, string key)
            {
                broadcast_vars.Add(key, progName);
            }
            // send changes to a prog by its igc address
            public static void AddChangeUnicaster(string key, long id)
            {
                unicast_vars.Add(key, id);
                handleUnicastMessages = true;
            }
            //----------------------------------//
            // the world position for the block //
            //----------------------------------//
            public static Vector3D BlockWorldPosition(IMyFunctionalBlock block, Vector3D offset = new Vector3D())
            {
                return Vector3D.Transform(offset, block.WorldMatrix);
            }
            //----------------------------------//
            // remapped game time               //
            //----------------------------------//
            public static DateTime GameTime
            {
                get
                {
                    // get the current time since midnight in seconds
                    double time = DateTime.Now.TimeOfDay.TotalSeconds;
                    // scale the time so that 2 hours real time is 1 day in game so 1am real time is 12pm in game
                    // if time = 3600 then time = 43200
                    time = time * 12;
                    double hour = (time / 3600) % 24;
                    double minute = (time / 60) % 60;
                    double second = time % 60;
                    return new DateTime(1, 1, 1, (int)hour, (int)minute, (int)second);
                }
            }
            public static string GameTimeString { get { return GridInfo.GameTime.ToString("h:mm tt"); } }
            //----------------------------------//
            // main loop                        //
            //----------------------------------//
            public static Action<string, UpdateType> MainLoop;
            public static Action<string> Command;
            public static Dictionary<string, Action<MyIGCMessage>> MessageHandlers = new Dictionary<string, Action<MyIGCMessage>>();
            public static Action<List<MyIGCMessage>> MessagesHandler;
            public static void Main(string argument, UpdateType updateSource)
            {
                RunCount++;
                if (updateSource == UpdateType.IGC)
                {
                    List<MyIGCMessage> messages = CheckMessages();
                    for (int i = 0; i < messages.Count; i++)
                    {
                        if (MessageHandlers.ContainsKey(messages[i].Tag))
                        {
                            MessageHandlers[messages[i].Tag](messages[i]);
                            messages.RemoveAt(i);
                            i--;
                        }
                    }
                    if (MessagesHandler != null) MessagesHandler(messages);
                }
                else if (updateSource == UpdateType.Terminal || updateSource == UpdateType.Trigger)
                {
                    Command?.Invoke(argument);
                }
                else MainLoop?.Invoke(argument, updateSource);
            }
            public static void AddMainLoop(Action<string, UpdateType> handler)
            {
                MainLoop += handler;
            }
            public static void AddMessageHandler(string tag, Action<MyIGCMessage> handler)
            {
                MessageHandlers.Add(tag, handler);
            }
            public static void AddMessagesHandler(Action<List<MyIGCMessage>> handler)
            {
                MessagesHandler += handler;
            }
            public static void AddCommandHandler(Action<string> handler)
            {
                Command += handler;
            }
        }
        //---------------------------------------------------------------//
        // GridInfo End                                                  //
        //---------------------------------------------------------------//
    }
}
