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
    public class PlayerStatusState : State, IStateDrawable
    {
        private enum PlayerStatusControls
        {
            Close
        }

        private Player m_GamePlayer;

        private GUIListBox m_InvListBox;

        private GUIImage m_BackImage;

        private Dictionary<InventoryItem.ItemType, int> m_Inv = new Dictionary<InventoryItem.ItemType, int>();

        public PlayerStatusState(IrrlichtDevice dev, Player player)
            : base(dev)
        {
            m_GamePlayer = player;

            Texture image = Loader.GetTexture("MenuImage.png");
            m_BackImage = Loader.GetImage((StateManager.m_ScreenDimensions.Width - 773) / 2, (StateManager.m_ScreenDimensions.Height - 690) / 2, StateManager.m_ScreenDimensions.Width, StateManager.m_ScreenDimensions.Height);
            m_BackImage.Image = image;

            m_ListOfGUIElements.Add(m_BackImage);
            m_ListOfGUIElements.Add(m_InvListBox = m_env.AddListBox(new Recti(200,200,400,500)));

            int halfScreenWidth = (int)(StateManager.m_ScreenDimensions.Width / 2);
            int halfScreenHeight = (int)(StateManager.m_ScreenDimensions.Height / 2);

            int buttonWidth = 50;
            int buttonHeight = 40;
            int startx = StateManager.m_ScreenDimensions.Width - buttonWidth - 25;
            int starty = StateManager.m_ScreenDimensions.Height - buttonHeight - 25;

            Recti buttRect = new Recti(startx, starty, startx + buttonWidth, starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)PlayerStatusControls.Close, "Close", "Return to the game"));
            m_ListOfShownGUIElements.AddRange(m_ListOfGUIElements);

            foreach (InventoryItem item in m_GamePlayer.m_PlayerInventory)
            {
                if (!m_Inv.ContainsKey(item.m_ItemType))
                    m_Inv.Add(item.m_ItemType, 0);
                m_Inv[item.m_ItemType]++;
            }
            foreach (KeyValuePair<InventoryItem.ItemType, int> item in m_Inv)
            {
                m_InvListBox.AddItem(item.Value.ToString().PadLeft(4) + " -|- " + item.Key.ToString().Replace('_',' '));
            }
        }

        public void Draw()
        {
            //Draw text here
            //m_DrawFront.Draw("work this time???", 50, 50, new IrrlichtLime.Video.Color(255, 255, 255));
        }

        public override bool HandleEvent(Event evnt)
        {
            switch (evnt.Type)
            {
                case EventType.Key:
                    if (evnt.Key.PressedDown)
                    {
                        if (evnt.Key.Key == KeyCode.KeyI)
                        {
                            StateFinished();
                            return true;
                        }
                    }
                    break;
                case EventType.GUI:
                    if (evnt.GUI.Type == GUIEventType.ButtonClicked)
                    {
                        switch ((PlayerStatusControls)evnt.GUI.Caller.ID)
                        {
                            case PlayerStatusControls.Close:
                                StateFinished();
                                return true;
                        }
                     }
                    break;
            }
            return false;
        }
    }
}
