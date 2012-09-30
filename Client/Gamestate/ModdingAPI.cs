﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

using LuaInterface;

using FTLOverdrive.Client.Ships;

namespace FTLOverdrive.Client.Gamestate
{
    public class ModdingAPI : IState
    {
        private Lua luastate;

        private Dictionary<string, List<LuaFunction>> dctHooks;

        public void OnActivate()
        {
            // Create the lua state
            luastate = new Lua();

            // Setup hooks
            dctHooks = new Dictionary<string, List<LuaFunction>>();

            // Bind functions
            BindFunction("print", "print");
            BindFunction("NewShip", "NewShip"); // Is there any way to call the constructor directly from lua code?

            luastate.NewTable("hook");
            BindFunction("hook.Add", "hook_Add");

            luastate.NewTable("library");
            BindFunction("library.AddWeapon", "library_AddWeapon");
            BindFunction("library.AddSystem", "library_AddSystem");
            BindFunction("library.AddRace", "library_AddRace");
            BindFunction("library.AddShipGenerator", "library_AddShipGenerator");
            BindFunction("library.GetWeapon", "library_GetWeapon");
            BindFunction("library.GetSystem", "library_GetSystem");
            BindFunction("library.GetRace", "library_GetRace");
            BindFunction("library.GetShip", "library_GetShipGenerator");
            BindFunction("library.CreateAnimation", "library_CreateAnimation");


            // Load lua files
            if (!Directory.Exists("lua")) Directory.CreateDirectory("lua");
            foreach (string name in Directory.GetFiles("lua"))
                luastate.DoFile("lua/" + name);
        }

        public void LoadMods()
        {
            // Validate the directory
            if (!Directory.Exists("mods")) Directory.CreateDirectory("mods");

            // Search for folders
            foreach (var folder in Directory.GetDirectories("mods"))
            {
                string initfile = folder + "/init.lua";
                string modname = Path.GetFileName(folder);
                if (!File.Exists(initfile))
                    Root.Singleton.Log("[" + modname + "] Missing init.lua.");
                else
                    try
                    {
                        luastate.DoFile(initfile);
                    }
                    catch (LuaException ex)
                    {
                        Root.Singleton.Log("[" + modname + "] " + ex.Message);
                    }
            }
        }

        private void BindFunction(string path, string funcname)
        {
            var method = GetType().GetMethod(funcname, BindingFlags.Instance | BindingFlags.NonPublic);
            
            luastate.RegisterFunction(path, this, method);
        }

        public void CallHook(string hookname, params object[] args)
        {
            // Get the hook list
            if (!dctHooks.ContainsKey(hookname)) return;
            var hooks = dctHooks[hookname];

            // Call each hook
            foreach (var func in hooks)
            {
                var results = func.Call(args);
                if ((results != null) && (results.Length > 0)) return;
            }
        }

        public class LuaShipGenerator : Library.ShipGenerator
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }

            public bool Unlocked { get; set; }
            public bool Default { get; set; }

            // whether it's a player ship or NPC ship
            public bool NPC { get; set; }

            public string MiniGraphic { get; set; }

            public LuaFunction Callback { get; set; }

            public Ship Generate(params object[] args)
            {
                if (Callback == null) return null;
                else return (Ship)Callback.Call(args)[0];
            }
        }

        #region Lua Binds

        private void print(object obj)
        {
            Root.Singleton.Log("[Lua] " + obj.ToString());
        }

        private void hook_Add(string name, LuaFunction function)
        {
            // Register in the hook
            if (!dctHooks.ContainsKey(name)) dctHooks.Add(name, new List<LuaFunction>());
            dctHooks[name].Add(function);
        }

        #region Library

        private Library.Weapon library_AddWeapon(string name, string type)
        {
            // Check what type of weapon it is
            if (type == "projectile")
            {
                // Create weapon and return it
                var wep = new Library.ProjectileWeapon();
                Root.Singleton.mgrState.Get<Library>().AddWeapon(name, wep);
                return wep;
            }
            return null;
        }

        private Library.System library_AddSystem(string name)
        {
            // Create system and return it
            var sys = new Library.System();
            Root.Singleton.mgrState.Get<Library>().AddSystem(name, sys);
            return sys;
        }

        private Library.Race library_AddRace(string name)
        {
            // Create race and return it
            var race = new Library.Race();
            Root.Singleton.mgrState.Get<Library>().AddRace(name, race);
            return race;
        }

        private LuaShipGenerator library_AddShipGenerator(string name)
        {
            // Create ship generator and return it
            var gen = new LuaShipGenerator();
            Root.Singleton.mgrState.Get<Library>().AddShipGenerator(name, gen);
            return gen;
        }

        private Library.Weapon library_GetWeapon(string name)
        {
            return Root.Singleton.mgrState.Get<Library>().GetWeapon(name);
        }

        private Library.System library_GetSystem(string name)
        {
            return Root.Singleton.mgrState.Get<Library>().GetSystem(name);
        }

        private Library.Race library_GetRace(string name)
        {
            return Root.Singleton.mgrState.Get<Library>().GetRace(name);
        }

        private Library.ShipGenerator library_GetShipGenerator(string name)
        {
            return Root.Singleton.mgrState.Get<Library>().GetShipGenerator(name);
        }

        private Library.Animation library_CreateAnimation(int tilestart, int tileend, int speed)
        {
            return new Library.Animation() { TileStart = tilestart, TileEnd = tileend, Speed = speed };
        }

        #endregion

        private Ship NewShip()
        {
            return new Ship();
        }

        #endregion

        public void OnDeactivate()
        {
            
        }

        public void Think(float delta)
        {
            
        }
    }
}
