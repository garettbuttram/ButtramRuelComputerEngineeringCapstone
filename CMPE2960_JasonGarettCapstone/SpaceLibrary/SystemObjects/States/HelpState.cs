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
    class HelpState : State, IStateDrawable
    {
        //Background picture
        private GUIImage m_BackGroundImage;

        private GUICheckBox m_AudioToggle;

        public event delVoidVoid OnExit;

        public HelpState(IrrlichtDevice dev)
            : base(dev)
        {
            Texture image = m_dev.VideoDriver.GetTexture("HelpImage.png");
            m_BackGroundImage = m_env.AddImage(new Recti(0, 0, StateManager.m_ScreenDimensions.Width, StateManager.m_ScreenDimensions.Height));
            m_BackGroundImage.Image = image;
            m_ListOfGUIElements.Add(m_BackGroundImage);

            int halfScreenWidth = (int)(StateManager.m_ScreenDimensions.Width / 2);
            int halfScreenHeight = (int)(StateManager.m_ScreenDimensions.Height / 2);

            int buttonWidth = 50;
            int buttonHeight = 40;
            int startx = StateManager.m_ScreenDimensions.Width - buttonWidth - 25;
            int starty = StateManager.m_ScreenDimensions.Height - buttonHeight - 25;

            Recti buttRect = new Recti(startx, starty, startx + buttonWidth, starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, 301, "Back", "Go back to the main menu"));

            buttRect = new Recti(startx + 20, starty - 35, startx + 45, starty-10);
            m_AudioToggle = m_env.AddCheckBox(true, buttRect);
            m_ListOfGUIElements.Add(m_AudioToggle);

            m_ListOfShownGUIElements.AddRange(m_ListOfGUIElements);
        }
        public override void EndState()
        {
            m_ListOfGUIElements.ForEach((gui) => gui.Remove());
        }
        public void Draw()
        {
            m_DrawFront.Draw("Mute", StateManager.m_ScreenDimensions.Width - 85, StateManager.m_ScreenDimensions.Height - 91, new IrrlichtLime.Video.Color(255, 255, 255));
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
            if (evnt.GUI.Type == GUIEventType.ButtonClicked && evnt.GUI.Caller.ID == 301)
            {
                        StateFinished();
                        if (OnExit != null)
                            OnExit();
                        return true;
            }
            else if (evnt.GUI.Type == GUIEventType.CheckBoxChanged)
            {
                if (m_AudioToggle.Checked)
                    Loader.EnableAudio();
                else
                    Loader.MuteAudio();

                return true;
            }
            return false;
        }
    }
}
