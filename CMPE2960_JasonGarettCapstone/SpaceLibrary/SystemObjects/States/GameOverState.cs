using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IrrlichtLime;
using IrrlichtLime.GUI;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace SpaceLibrary
{
    class GameOverState : State
    {
        public GameOverState(IrrlichtDevice dev)
            :base(dev)
        {
            int buttonWidth = 100;
            int buttonHeight = 40;
            int startx = StateManager.m_ScreenDimensions.Width - buttonWidth - 25;
            int starty = StateManager.m_ScreenDimensions.Height - buttonHeight - 25;

            Recti buttRect = new Recti(startx, starty, startx + buttonWidth, starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, 301, "Main Menu", "Go back to the main menu"));
            m_ListOfShownGUIElements.AddRange(m_ListOfGUIElements);
        }

        public override bool HandleEvent(Event evnt)
        {
            if(evnt.Type == EventType.GUI)
                if(evnt.GUI.Type == GUIEventType.ButtonClicked)
                    if (evnt.GUI.Caller.ID == 301)
                    {
                        StateFinished();
                        StateFinished();
                        return true;
                    }
            return false;
        }
    }
}
