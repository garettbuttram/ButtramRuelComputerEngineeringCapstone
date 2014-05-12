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
    public class GameState : State, IGDIDrawable, IStateUpdateable
    {
        private enum GameStateControls
        {
            LeaveSystem,
            CancelLeaveSystem,
            SystemChoices
        }

        //Whether or not the camera is currently the galaxy cam
        private bool m_SystemCam = false;

        private GUIButton leaveButton;
        private GUIButton cancelLeaveButton;
        private GUIListBox systemChoices;

        //All GameObjects into a single place
        private List<GameObject> m_ListGameObjects;

        //The players camera
        private CameraSceneNode m_PlayerCamera;
        //The system camera
        private CameraSceneNode m_SystemCamera;
        //The pretty space scenery displayed below everything else
        private MeshSceneNode m_Background;

        //The player playing the game
        public Player m_GamePlayer { get; private set; }

        //The galaxy of the game
        private Galaxy m_GameGalaxy;
        //The system the game is currently in
        private SpaceSystem m_GameSystem;

        public event delVoidVoid OnSaveGame;
        public event StateManager.delVoidString OnNewSystemWanted;

        public GameState(IrrlichtDevice dev, SpaceSystem spaceSystem, Player gamePlayer, Galaxy gameGalaxy)
            :base (dev)
        {
            m_ListGameObjects = new List<GameObject>();
            m_GamePlayer = gamePlayer;

            foreach (GameObject go in spaceSystem.GetAllGameObjects())
                AddGameObject(go);

            AddGameObject(gamePlayer.m_PlayerShip);

            m_Background = Loader.LoadMiscMesh(Loader.MiscMesh.Spacefloor);
            m_Background.Position = new Vector3Df(0, -500, 0);

            m_GameGalaxy = gameGalaxy;
            m_GameSystem = spaceSystem;
        }

        /// <summary>
        /// Occurs when the player dies
        /// Ends the game
        /// </summary>
        public void GameOver()
        {
            NewStateCreated(new GameOverState(m_dev));
        }

        /// <summary>
        /// Updates everything in the current GameState
        /// </summary>
        public void Update()
        {
            foreach (IGameObjectUpdateable update in GetAllGameObjects((go) => go is IGameObjectUpdateable).ToList())
                if (isActive)
                    update.Update(this);
                else
                    break;

            if (!isActive)
                return;

            m_PlayerCamera.Target = m_GamePlayer.m_PlayerShip.m_GameObjectMesh.Position;
            m_SystemCamera.Target = m_GamePlayer.m_PlayerShip.m_GameObjectMesh.Position;
            m_GamePlayer.Update(this);

            float playerXPos = m_GamePlayer.m_PlayerShip.m_xPosition;
            float playerZPos = m_GamePlayer.m_PlayerShip.m_zPosition;

            if (Math.Abs(playerXPos) > 1500 || Math.Abs(playerZPos) > 1500)
            {
                isActive = false;
                BuildSystemLeaveChoices();
            }
        }

        /// <summary>
        /// Build the buttons to choose whether or not to leave the system
        /// </summary>
        private void BuildSystemLeaveChoices()
        {
            int halfScreenWidth = (int)(StateManager.m_ScreenDimensions.Width / 2);
            int halfScreenHeight = (int)(StateManager.m_ScreenDimensions.Height / 2);

            int buttonWidth = 100;
            int buttonHeight = 75;
            int startx;
            int starty;

            systemChoices = m_env.AddListBox(new Recti(halfScreenWidth - 200,
                                                   halfScreenHeight - 200,
                                                   halfScreenWidth,
                                                   halfScreenHeight), null, (int)GameStateControls.SystemChoices);
            m_ListOfGUIElements.Add(systemChoices);
            systemChoices.SetDrawBackground(true);
            PopulateSystemChoicesTable();
            systemChoices.SelectedIndex = 0;

            Recti buttRect = new Recti(startx = halfScreenWidth - buttonWidth - 25,
                                      starty = 150 + halfScreenHeight,
                                      startx + buttonWidth,
                                      starty + buttonHeight);
            leaveButton = m_env.AddButton(buttRect, null, (int)GameStateControls.LeaveSystem, "Leave System", "Leave this system for a new system?");
            m_ListOfGUIElements.Add(leaveButton);
            m_ListOfShownGUIElements.Add(leaveButton);

            buttRect = new Recti(startx = halfScreenWidth + 25,
                                 starty = 150 + halfScreenHeight,
                                 startx + buttonWidth,
                                 starty + buttonHeight);
            cancelLeaveButton = m_env.AddButton(buttRect, null, (int)GameStateControls.CancelLeaveSystem, "Stay in System", "Do not leave this system");
            m_ListOfGUIElements.Add(cancelLeaveButton);
            m_ListOfShownGUIElements.Add(cancelLeaveButton);
        }

        private void RemoveSystemLeaveChoices()
        {
            m_ListOfShownGUIElements.Remove(leaveButton);
            m_ListOfShownGUIElements.Remove(cancelLeaveButton);
            m_ListOfShownGUIElements.Remove(systemChoices);

            m_ListOfGUIElements.Remove(leaveButton);
            m_ListOfGUIElements.Remove(cancelLeaveButton);
            m_ListOfGUIElements.Remove(systemChoices);

            leaveButton.Remove();
            cancelLeaveButton.Remove();
            systemChoices.Remove();

            leaveButton = null;
            cancelLeaveButton = null;
            systemChoices = null;
        }

        /// <summary>
        /// Builds the list of systems the player can choose to go to
        /// </summary>
        private void PopulateSystemChoicesTable()
        {
            systemChoices.AddItem("New system");
            foreach (SpaceSystem ss in m_GameGalaxy.m_AllSystems)
            {
                if(ss == m_GameSystem)
                    continue;
                systemChoices.AddItem(ss.m_SystemName); 
            }
        }

        /// <summary>
        /// Draw the regions of everything in the current GameState
        /// </summary>
        /// <param name="gr"></param>
        public void GDIDraw(Graphics gr)
        {
            foreach (IGDIDrawable draw in GetAllGameObjects((go) => go is IGDIDrawable).ToList())
                draw.GDIDraw(gr);
        }

        /// <summary>
        /// Returns all game objects
        /// or only those that match the given filter if set
        /// </summary>
        /// <param name="filterGameObjects">The function to filter game objects</param>
        /// <returns></returns>
        public IEnumerable<GameObject> GetAllGameObjects(Predicate<GameObject> filterGameObjects = null)
        {
            foreach(GameObject go in m_ListGameObjects)
            {
                if(filterGameObjects != null)
                {
                    if(filterGameObjects(go))
                        yield return go;
                }
                else
                {
                    yield return go;
                }
            }
        }

        /// <summary>
        /// Adds a game object to the game state
        /// </summary>
        /// <param name="go">The game object to be added</param>
        private void AddGameObject(GameObject go)
        {
            if (go != null)
            {
                go.BuildGameObjectRegion();
                if (go is MeshGameObject)
                    ((MeshGameObject)go).BuildGameObjectMesh();

                go.ThisGameObjectDied += new GameObject.delVoidGameObject(RemoveGameObject);
                go.ThisGameObjectMadeANewObject += new GameObject.delVoidGameObject(AddGameObject);
                m_ListGameObjects.Add(go);
            }
        }

        /// <summary>
        /// Remove the given game object from the game state
        /// </summary>
        /// <param name="go">The game object to be removed</param>
        private void RemoveGameObject(GameObject go)
        {
            if (go != null)
            {
                if (go is MeshGameObject)
                    ((MeshGameObject)go).RemoveMesh();
                m_ListGameObjects.Remove(go);

                go.ThisGameObjectDied -= new GameObject.delVoidGameObject(RemoveGameObject);
                go.ThisGameObjectMadeANewObject -= new GameObject.delVoidGameObject(AddGameObject);
            }
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
                case EventType.Key:
                    return ProcessKeyEvent(evnt);
                case EventType.GUI:
                    return ProcessGUIEvent(evnt);
            }
            return false;
        }

        /// <summary>
        /// Handles gui events
        /// </summary>
        /// <param name="evnt"></param>
        /// <returns></returns>
        private bool ProcessGUIEvent(Event evnt)
        {
            if (evnt.GUI.Type == GUIEventType.ButtonClicked)
            {
                switch ((GameStateControls)evnt.GUI.Caller.ID)
                {
                    case GameStateControls.LeaveSystem:
                        string systemChoice = systemChoices.SelectedItem;
                        RemoveSystemLeaveChoices();
                        StateFinished();
                        if (OnNewSystemWanted != null)
                            OnNewSystemWanted(systemChoice);
                        return true;
                    case GameStateControls.CancelLeaveSystem:
                        m_GamePlayer.m_PlayerShip.TowardsCenter();
                        m_GamePlayer.m_PlayerShip.BuildGameObjectRegion();
                        m_GamePlayer.m_PlayerShip.RemoveMesh();
                        m_GamePlayer.m_PlayerShip.BuildGameObjectMesh();
                        m_GamePlayer.m_PlayerShip.MoveInward();

                        BuildCameras();

                        RemoveSystemLeaveChoices();
                        isActive = true;
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Processes the Key event sent
        /// </summary>
        /// <param name="evnt">The key event</param>
        /// <returns>Whether or not the event was processed</returns>
        private bool ProcessKeyEvent(Event evnt)
        {
            if (evnt.Key.Key == m_GamePlayer.m_PlayerControls.EnterSS && evnt.Key.PressedDown)
                EnterSpaceStation();

            switch (evnt.Key.Key)
            {
                case KeyCode.KeyP:
                    if (evnt.Key.PressedDown)
                    {
                        PauseState ps = new PauseState(m_dev);
                        ps.OnExit += new delVoidVoid(StateFinished);
                        ps.OnSave += new delVoidVoid(SaveGame);
                        NewStateCreated(ps);
                        return true;
                    }
                    break;
                case KeyCode.KeyI:
                    if (evnt.Key.PressedDown)
                    {
                        PlayerStatusState pss = new PlayerStatusState(m_dev, m_GamePlayer);
                        NewStateCreated(pss);
                        return true;
                    }
                    break;
                case KeyCode.KeyG:
                    if (evnt.Key.PressedDown)
                    {
                        m_SystemCam = !m_SystemCam;
                        if (m_SystemCam)
                            m_dev.SceneManager.ActiveCamera = m_SystemCamera;
                        else
                            m_dev.SceneManager.ActiveCamera = m_PlayerCamera;
                    }
                    break;
            }
            return false;
        }

        public override void BeginState()
        {
            //foreach (MeshGameObject go in GetAllGameObjects((go) => go is MeshGameObject))
            //        go.BuildGameObjectMesh();

            BuildCameras();
            m_GamePlayer.BuildStats();
        }

        /// <summary>
        /// Pauses the game state removing all meshes
        /// </summary>
        public override void PauseState()
        {
            foreach (MeshGameObject go in GetAllGameObjects((go) => go is MeshGameObject))
                go.RemoveMesh();

            base.PauseState();
        }

        /// <summary>
        /// Resumes a paused game state
        /// </summary>
        public override void ResumeState()
        {
            foreach (GameObject go in GetAllGameObjects())
            {
                go.BuildGameObjectRegion();
                if (go is MeshGameObject)
                    ((MeshGameObject)go).BuildGameObjectMesh();
            }

            BuildCameras();

            base.ResumeState();
        }

        /// <summary>
        /// Removes all meshes from the scene manager
        /// </summary>
        public override void EndState()
        {
            foreach (GameObject go in GetAllGameObjects().ToList())
                RemoveGameObject(go);
            m_GamePlayer.RemoveGUI();
            m_Background.Remove();
            base.EndState();
        }

        /// <summary> 
        /// Saves the current player and system
        /// </summary>
        private void SaveGame()
        {
            if (OnSaveGame != null)
                OnSaveGame();
        }

        /// <summary>
        /// Occurs when the player wishes to enter a space station
        /// </summary>
        private void EnterSpaceStation()
        {
            
            foreach (SpaceStation ss in GetAllGameObjects((go) => go is SpaceStation))
            {
                if (MeshGameObject.GetDistanceBetweenMeshes(m_GamePlayer.m_PlayerShip, ss) < 30)
                {
                    NewStateCreated(new SpaceStationState(m_dev, m_GamePlayer, ss.m_SellItems));
                    break;
                }
            }
        }

        /// <summary>
        /// Builds the cameras used by the game state
        /// </summary>
        private void BuildCameras()
        {
            m_PlayerCamera = m_dev.SceneManager.AddCameraSceneNode(m_GamePlayer.m_PlayerShip.m_GameObjectMesh, new Vector3Df(0, 100, -70));
            m_SystemCamera = m_dev.SceneManager.AddCameraSceneNode(m_GamePlayer.m_PlayerShip.m_GameObjectMesh, new Vector3Df(0, 400, -300));
            m_dev.SceneManager.ActiveCamera = m_PlayerCamera;
            m_SystemCam = false;
        }
    }
}
