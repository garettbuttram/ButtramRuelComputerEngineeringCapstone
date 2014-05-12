using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

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
using IrrKlang;

namespace SpaceLibrary
{
    public class StateManager
    {
        public delegate void delVoidString(string s);

        //Called when the game is to be closed
        public event State.delVoidVoid OnCloseApplication;
        //screen dimensions
        static public Dimension2Di m_ScreenDimensions { get; private set; }

        static public Graphics m_gr { get; private set; }
        
        //All currently held states
        private Stack<State> m_States = new Stack<State>();

        //The player of the game
        public Player m_Player { get; private set; }

        //The galaxy the player plays in
        public Galaxy m_Galaxy { get; private set; }

        public IrrlichtDevice m_dev { get; private set; }

        //Get the currently active state
        public State CurrentState
        {
            get
            {
                return m_States.Peek();
            }
        }

        public ISound m_Music = null;

        public StateManager(Graphics gr, IrrlichtDevice dev, Dimension2Di sd)
        {
            m_ScreenDimensions = sd;
            m_gr = gr;
            m_dev = dev;

            MainMenuState mms = new MainMenuState(m_dev);

            mms.OnStateFinished += new State.delVoidVoid(PopState);
            mms.OnNewStateCreated += new State.delVoidState(AddState);
            mms.OnNewStateStack += new State.delVoidState(ClearStack);

            mms.OnGameStart += new MainMenuState.delVoidGalaxyPlayer(StartGame);
            mms.OnCloseApplication += new State.delVoidVoid(EndProgram);

            m_States.Push(mms);

            State.m_DrawFront = m_dev.GUIEnvironment.GetFont("fonthaettenschweiler.bmp");
            m_dev.GUIEnvironment.Skin.SetFont(State.m_DrawFront);
            //AddState(new GameState(m_env, m_Galaxy.GetCurrentLevel(), m_Player));

            m_Music = Loader.m_se.Play2D("Sound/M_Ethereal.wav");
        }

        /// <summary>
        /// Updates all active states
        /// </summary>
        public void Update()
        {
            foreach (State s in m_States.Where((checkActive) => checkActive.isActive && checkActive is IStateUpdateable).ToList())
                (s as IStateUpdateable).Update();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Draw()
        {
            if (m_States.Peek() is IStateDrawable)
                (m_States.Peek() as IStateDrawable).Draw();
            if (CurrentState is GameState || CurrentState is SpaceStationState)
                m_Player.RenderStats();
        }
        /// <summary>
        /// Adds a state to the stack pausing the previous state
        /// </summary>
        /// <param name="newState"></param>
        public void AddState(State newState)
        {
            newState.OnStateFinished += new State.delVoidVoid(PopState);
            newState.OnNewStateCreated += new State.delVoidState(AddState);
            newState.OnNewStateStack += new State.delVoidState(ClearStack);

            m_States.Peek().PauseState();
            newState.BeginState();
            m_States.Push(newState);
            //m_States.Peek().BeginState();
        }

        /// <summary>
        /// Removes the current state from the stack resuming the previous one
        /// </summary>
        private void PopState()
        {
            if (m_States.Count > 1)
            {
                m_States.Peek().EndState();
                m_States.Pop();
                m_States.Peek().ResumeState();
            }
        }
        /// <summary>
        /// Clears the current state stack and starts over with a new state
        /// </summary>
        public void ClearStack(State newInitialState)
        {
            while (m_States.Count > 1)
                PopState();

            AddState(newInitialState);
        }

        /// <summary>
        /// Get the player of the game
        /// </summary>
        /// <returns>The Player of the game</returns>
        public Player GetPlayer()
        {
            return m_Player;
        }

        /// <summary>
        /// Draw the regions of all active game states
        /// </summary>
        /// <param name="gr">The graphics context of the regions</param>
        public void GDIDraw(Graphics gr)
        {
            foreach (State s in m_States.Where((checkActive) => checkActive.isActive && checkActive is IGDIDrawable))
                (s as IGDIDrawable).GDIDraw(gr);
        }

        /// <summary>
        /// Handles events thrown by the irrlicht device
        /// </summary>
        /// <param name="evnt">The thrown event</param>
        /// <returns>Whether or not the event has been handled</returns>
        public bool HandleEvent(Event evnt)
        {
            if (m_Player != null)
            {
                if (evnt.Type == EventType.Key)
                    if (evnt.Key.PressedDown)
                        m_Player.m_PlayerControls.PressKey(evnt.Key.Key);
                    else
                        m_Player.m_PlayerControls.ReleaseKey(evnt.Key.Key);
            }

            if(CurrentState != null)
                return CurrentState.HandleEvent(evnt);

            return false;
        }

        /// <summary>
        /// Begins the game based on the given 
        /// </summary>
        private void StartGame(Galaxy gal, Player gamePlayer)
        {
            m_Galaxy = gal;
            m_Player = gamePlayer;
            CreateAndAddGameState();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateAndAddGameState()
        {
            m_Player.m_PlayerShip.MoveToRandom();
            GameState newGame = new GameState(m_dev, m_Galaxy.GetCurrentLevel(), m_Player, m_Galaxy);
            newGame.OnSaveGame += new GameState.delVoidVoid(SaveGameState);
            newGame.OnNewSystemWanted += new delVoidString(CreateAndStartNewSystem);
            ClearStack(newGame);
        }

        /// <summary>
        /// Ends the program
        /// </summary>
        private void EndProgram()
        {
            if (OnCloseApplication != null)
                OnCloseApplication();
        }

        /// <summary>
        /// Saves the game
        /// </summary>
        private void SaveGameState()
        {
            if (!Directory.Exists(m_dev.FileSystem.WorkingDirectory + @"\saves"))
                Directory.CreateDirectory(m_dev.FileSystem.WorkingDirectory + @"\saves");

            string filename = m_dev.FileSystem.WorkingDirectory + @"\saves\" + m_Player.m_PlayerName + ".xml";

            try
            {
                XmlSerializer xmlSer = new XmlSerializer(typeof(Save));
                TextWriter write = new StreamWriter(filename);
                Save gameSave = new Save(m_Galaxy, m_Player);

                xmlSer.Serialize(write, gameSave);

                write.Close();
            }
            catch(Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Creates a new system and builds a new game state around it
        /// </summary>
        private void CreateAndStartNewSystem(string systemChoice)
        {
            if (systemChoice == "New system")
                m_Galaxy.CreateNewSystemAndSetAsCurrent();
            else
                m_Galaxy.SetCurrentSystemByName(systemChoice);
            CreateAndAddGameState();
        }

        public void EndGame()
        {
            m_States.Clear();
            m_States = null;
            m_Galaxy = null;
            m_Player = null;
            m_dev = null;
            m_gr = null;
            m_ScreenDimensions = null;
        }
    }
}
