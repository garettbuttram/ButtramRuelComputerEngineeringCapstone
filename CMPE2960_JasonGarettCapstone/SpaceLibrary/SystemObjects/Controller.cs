using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using IrrlichtLime;
using IrrlichtLime.GUI;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace SpaceLibrary
{
    public class Controller
    {
        //The key used to go forward
        public KeyCode Forward { get; private set; }
        //The key used to go Left
        public KeyCode Left { get; private set; }
        //The key used to go Right
        public KeyCode Right { get; private set; }
        //The key used to go backwards
        public KeyCode Reverse { get; private set; }
        //The key used to go fire a gun
        public KeyCode FirePrimary { get; private set; }
        //The key used to fire a secondary weapon
        public KeyCode FireSecondary { get; private set; }
        //The key used to pause the game
        public KeyCode Pause { get; private set; }
        //The key used to enter space stations
        public KeyCode EnterSS { get; private set; }
        //The key used too change weapons
        public KeyCode ChangeWeapon { get; private set; }

        //Holds whether or not the key is pressed or not
        private Dictionary<KeyCode, bool> KeyStates;
        //Whether or not the keys were pressed during the last update of the controller
        private Dictionary<KeyCode, bool> PreviousKeyStates;

        public Controller()
        {
            KeyStates = new Dictionary<KeyCode, bool>();

            Forward = KeyCode.Up;
            KeyStates.Add(Forward, false);
            Left = KeyCode.Left;
            KeyStates.Add(Left, false);
            Right = KeyCode.Right;
            KeyStates.Add(Right, false);
            Reverse = KeyCode.Down;
            KeyStates.Add(Reverse, false);
            FirePrimary = KeyCode.KeyS;
            KeyStates.Add(FirePrimary, false);
            Pause = KeyCode.KeyP;
            KeyStates.Add(Pause, false);
            EnterSS = KeyCode.KeyC;
            KeyStates.Add(EnterSS, false);
            ChangeWeapon = KeyCode.KeyQ;
            KeyStates.Add(ChangeWeapon, false);
            FireSecondary = KeyCode.KeyD;
            KeyStates.Add(FireSecondary, false);

            PreviousKeyStates = new Dictionary<KeyCode,bool>();
            foreach (KeyCode kc in KeyStates.Keys)
                PreviousKeyStates.Add(kc, false);
        }

        /// <summary>
        /// Process key pressed changing the state of the KeyStates dictionary if nessecary
        /// </summary>
        /// <param name="key">The pressed key</param>
        public void PressKey(KeyCode key)
        {
            if (KeyStates.ContainsKey(key))
                KeyStates[key] = true;
        }

        /// <summary>
        /// Process key released changing the state of the KeyStates dictionary if nessecary
        /// </summary>
        /// <param name="key">The key released</param>
        public void ReleaseKey(KeyCode key)
        {
            if (KeyStates.ContainsKey(key))
                KeyStates[key] = false;
        }

        /// <summary>
        /// Check if a key is pressed
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether or not the key is pressed</returns>
        public bool isPressed(KeyCode key)
        {
            return KeyStates[key];
        }

        /// <summary>
        /// Checks if a key is newly pressed
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>Whether or not the key is newly pressed</returns>
        public bool isNewlyPressed(KeyCode key)
        {
            return KeyStates[key] && !PreviousKeyStates[key];
        }

        /// <summary>
        /// Updates the controller
        /// </summary>
        public void Update()
        {
            foreach (KeyCode kc in KeyStates.Keys)
                PreviousKeyStates[kc] = KeyStates[kc];
        }
    }
}
