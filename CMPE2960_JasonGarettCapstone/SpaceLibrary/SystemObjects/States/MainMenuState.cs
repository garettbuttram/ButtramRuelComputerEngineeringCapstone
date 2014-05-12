using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

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
    public class MainMenuState : State
    {
        public enum MainMenuControls
        {
            Start = 101,
            Load,
            Help,
            Quit,
            LoadTable,
            LoadGame,
            PlayerNameEntered
        };

        public delegate void delVoidGalaxyPlayer(Galaxy gal, Player gamePlayer);

        //Called when the gamestate is to be pushed
        public event delVoidGalaxyPlayer OnGameStart;
        //Called when the game is to be closed
        public event delVoidVoid OnCloseApplication;

        //Background picture
        private GUIImage m_BackGroundImage;

        //GUI elements used for loading/creating new files
        private GUIListBox m_LoadTable;
        private GUIButton m_LoadSelected;
        private GUIEditBox m_PlayerName;
        private GUIButton m_NameEntered;

        public MainMenuState(IrrlichtDevice dev)
            : base(dev)
        {
            int halfScreenWidth = (int)(StateManager.m_ScreenDimensions.Width / 2);
            int halfScreenHeight = (int)(StateManager.m_ScreenDimensions.Height / 2);

            Texture image = m_dev.VideoDriver.GetTexture("StarWars.jpg");
            m_BackGroundImage = m_env.AddImage(new Recti(0, 0, StateManager.m_ScreenDimensions.Width, StateManager.m_ScreenDimensions.Height));
            m_BackGroundImage.Image = image;
            m_ListOfGUIElements.Add(m_BackGroundImage);

            int buttonWidth = 100;
            int buttonHeight = 75;
            int startx;
            int starty;

            Recti buttRect = new Recti(startx = halfScreenWidth - buttonWidth - 25,
                                       starty = 150 + halfScreenHeight,
                                       startx + buttonWidth,
                                       starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)MainMenuControls.Start, "Start Game", "Starts a new game"));

            buttRect = new Recti(startx = halfScreenWidth + 25,
                                 starty = 150 + halfScreenHeight,
                                 startx + buttonWidth,
                                 starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)MainMenuControls.Load, "Load Game", "Load a saved game and continue playing"));

            buttRect = new Recti(startx = halfScreenWidth - buttonWidth - 25, starty = 150 + halfScreenHeight + buttonHeight + 50, startx + buttonWidth, starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)MainMenuControls.Help, "Help and Options", "Learn to play and change game options"));

            buttRect = new Recti(startx = halfScreenWidth + 25, starty = 150 + halfScreenHeight + buttonHeight + 50, startx + buttonWidth, starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)MainMenuControls.Quit, "Quit", "Exit to Windows"));

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
                default:
                    break;
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
            if(evnt.GUI.Type == GUIEventType.ButtonClicked)
            {
                switch ((MainMenuControls)evnt.GUI.Caller.ID)
                {
                    case MainMenuControls.Start:
                        RemoveLoadElements();
                        BuildPlayerEntry();
                        return true;
                    case MainMenuControls.Load:
                        RemoveStartElements();
                        BuildLoadElements();
                        return true;
                    case MainMenuControls.Help:
                        HelpState hs = new HelpState(m_dev);
                        hs.OnExit += new delVoidVoid(StateFinished);
                        NewStateCreated(hs);
                        return true;
                    case MainMenuControls.Quit:
                        if (OnCloseApplication != null)
                            OnCloseApplication();
                        return true;
                    case MainMenuControls.LoadGame:
                        LoadSelected();
                        return true;
                    case MainMenuControls.PlayerNameEntered:
                        if(m_PlayerName.Text != "" && OnGameStart != null)
                            OnGameStart(new Galaxy(), new Player(new Controller(), m_dev, m_PlayerName.Text));
                        return true;
                }
            }
            return false;
        }

        private void BuildPlayerEntry()
        {
            int halfScreenWidth = (int)(StateManager.m_ScreenDimensions.Width / 2);
            int halfScreenHeight = (int)(StateManager.m_ScreenDimensions.Height / 2);

            m_PlayerName = m_env.AddEditBox("", new Recti(halfScreenWidth - 200,
                                                       halfScreenHeight - 50,
                                                       halfScreenWidth,
                                                       halfScreenHeight), true, null, (int)MainMenuControls.PlayerNameEntered);

            Recti buttRect = new Recti(halfScreenWidth - 125, 400, halfScreenWidth - 50, 475);
            m_NameEntered = m_env.AddButton(buttRect, null, (int)MainMenuControls.PlayerNameEntered, "Entered Name", "Begins the game if you have entered your name");

            m_ListOfGUIElements.Add(m_PlayerName);
            m_ListOfGUIElements.Add(m_NameEntered);

            m_ListOfShownGUIElements.Add(m_PlayerName);
            m_ListOfShownGUIElements.Add(m_NameEntered);

        }

        /// <summary>
        /// Builds the table of saves the player can load
        /// </summary>
        private void BuildLoadElements()
        {
            if (Directory.Exists(m_dev.FileSystem.WorkingDirectory + @"\saves"))
            {
                int halfScreenWidth = (int)(StateManager.m_ScreenDimensions.Width / 2);
                int halfScreenHeight = (int)(StateManager.m_ScreenDimensions.Height / 2);
                m_LoadTable = m_env.AddListBox(new Recti(halfScreenWidth - 200,
                                                       halfScreenHeight - 200,
                                                       halfScreenWidth,
                                                       halfScreenHeight), null, (int)MainMenuControls.LoadTable);
                m_LoadTable.SetDrawBackground(true);
                m_ListOfGUIElements.Add(m_LoadTable);
                Recti buttRect = new Recti(400, 400, 500, 475);
                m_LoadSelected = m_env.AddButton(buttRect, null, (int)MainMenuControls.LoadGame, "Load Selected", "Load the selected game");
                m_ListOfGUIElements.Add(m_LoadSelected);
                m_ListOfGUIElements.Add(m_LoadTable);

                m_ListOfShownGUIElements.Add(m_LoadSelected);
                m_ListOfShownGUIElements.Add(m_LoadTable);

                IEnumerable<string> files = Directory.EnumerateFiles(m_dev.FileSystem.WorkingDirectory + @"\saves", "*.xml");

                foreach (string s in files)
                {
                    FileInfo fi = new FileInfo(s);
                    m_LoadTable.AddItem(fi.Name);
                }
            }
        }

        /// <summary>
        /// Loads the selected game save
        /// </summary>
        private void LoadSelected()
        {
            if (m_LoadTable.SelectedItem == null)
                return;

            string file = m_dev.FileSystem.WorkingDirectory + @"\saves\" + m_LoadTable.SelectedItem;
            object save = null;

            RemoveLoadElements();

            try
            {
                FileStream fs = new FileStream(file, FileMode.Open);
                XmlSerializer xmlSer = new XmlSerializer(typeof(Save));

                save = xmlSer.Deserialize(fs);

                fs.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }

            if (save is Save)
            {
                Save gameSave = save as Save;
                //Need to build the contsructors for taking in a save
                if (OnGameStart != null)
                    OnGameStart(new Galaxy(gameSave.m_GalaxySave), new Player(gameSave.m_PlayerSave, new Controller(), m_dev));
            }
        }

        /// <summary>
        /// Removes the loading stuff if they have been created
        /// </summary>
        public override void PauseState()
        {
            RemoveLoadElements();
            RemoveStartElements();

            base.PauseState();
        }

        /// <summary>
        /// Removes elements required for loading
        /// </summary>
        private void RemoveLoadElements()
        {
            if (m_LoadTable != null)
            {
                m_LoadTable.Remove();
                m_ListOfShownGUIElements.Remove(m_LoadTable);
                m_LoadTable = null;
            }

            if (m_LoadSelected != null)
            {
                m_LoadSelected.Remove();
                m_ListOfShownGUIElements.Remove(m_LoadSelected);
                m_LoadSelected = null;
            }
        }

        /// <summary>
        /// Remove elements required for starting the game
        /// </summary>
        private void RemoveStartElements()
        {
            if (m_NameEntered != null)
            {
                m_NameEntered.Remove();
                m_ListOfShownGUIElements.Remove(m_NameEntered);
                m_NameEntered = null;
            }

            if (m_PlayerName != null)
            {
                m_PlayerName.Remove();
                m_ListOfShownGUIElements.Remove(m_PlayerName);
                m_PlayerName = null;
            }
        }
    }
}
