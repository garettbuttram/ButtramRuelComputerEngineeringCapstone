using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;

using IrrlichtLime;
using IrrlichtLime.GUI;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace SpaceLibrary
{
    public class PauseState : State
    {
        private enum PauseMenuControls
        {
            ResumeGame = 201,
            SaveGame,
            SaveAndExit,
            ExitNoSave
        }

        public event delVoidVoid OnExit;
        public event delVoidVoid OnSave;

        public PauseState(IrrlichtDevice dev)
            :base(dev)
        {
            int halfScreenWidth = (int)(StateManager.m_ScreenDimensions.Width / 2);
            int halfScreenHeight = (int)(StateManager.m_ScreenDimensions.Height / 2);

            int buttonWidth = 100;
            int buttonHeight = 75;
            int startx;
            int starty;

            Recti buttRect = new Recti(startx = halfScreenWidth - buttonWidth - 25,
                                      starty = 150 + halfScreenHeight,
                                      startx + buttonWidth,
                                      starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)PauseMenuControls.ResumeGame, "Resume Game", "Resumes the current game"));

            buttRect = new Recti(startx = halfScreenWidth + 25,
                                 starty = 150 + halfScreenHeight,
                                 startx + buttonWidth,
                                 starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)PauseMenuControls.SaveGame, "Save Game", "Save the current game then continue playing"));

            buttRect = new Recti(startx = halfScreenWidth - buttonWidth - 25, starty = 150 + halfScreenHeight + buttonHeight + 50, startx + buttonWidth, starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)PauseMenuControls.SaveAndExit, "Save and Exit", "Save then exit the game"));

            buttRect = new Recti(startx = halfScreenWidth + 25, starty = 150 + halfScreenHeight + buttonHeight + 50, startx + buttonWidth, starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)PauseMenuControls.ExitNoSave, "Exit", "Exit the game"));
            m_ListOfShownGUIElements.AddRange(m_ListOfGUIElements);
        }

        /// <summary>
        /// Handles events when this state is the currently active state
        /// </summary>
        /// <param name="evnt"></param>
        /// <returns></returns>
        public override bool HandleEvent(Event evnt)
        {
            switch (evnt.Type)
            {
                case EventType.GUI:
                    return ProcessGUIEvent(evnt);
                case EventType.Key:
                    return ProcessKeyEvent(evnt);
            }

            return false;
        }

        /// <summary>
        /// Process GUI events
        /// </summary>
        /// <param name="evnt">The event to process</param>
        /// <returns>Whether or not the event was processed</returns>
        private bool ProcessGUIEvent(Event evnt)
        {
            if (evnt.GUI.Type == GUIEventType.ButtonClicked)
            {
                switch ((PauseMenuControls)evnt.GUI.Caller.ID)
                {
                    case PauseMenuControls.ResumeGame:
                        StateFinished();
                        return true;
                    case PauseMenuControls.SaveGame:
                        if (OnSave != null)
                            OnSave();
                        return true;
                    case PauseMenuControls.SaveAndExit:
                        if (OnSave != null)
                            OnSave();
                        StateFinished();
                        if (OnExit != null)
                            OnExit();
                        return true;
                    case PauseMenuControls.ExitNoSave:
                        StateFinished();
                        if (OnExit != null)
                            OnExit();
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Processes Key events
        /// </summary>
        /// <param name="evnt">The event to be processed</param>
        /// <returns>Whether or not the event was processed</returns>
        private bool ProcessKeyEvent(Event evnt)
        {
            switch (evnt.Key.Key)
            {
                case KeyCode.KeyP:
                    if (evnt.Key.PressedDown)
                    {
                        StateFinished();
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
