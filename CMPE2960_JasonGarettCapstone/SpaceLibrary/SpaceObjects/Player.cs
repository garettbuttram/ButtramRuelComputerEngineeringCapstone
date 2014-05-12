using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using IrrlichtLime;
using IrrlichtLime.GUI;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace SpaceLibrary
{
    public class Player
    {
        static public float debugXOffset = 0;
        static public float debugYOffset = 0;
        static public float debugXOffsetPos = 0;
        static public float debugYOffsetPos = 0;

        //The player's personal ship
        public PlayerShip m_PlayerShip { get; private set; }
        //public GroundUnit m_PlayerGU { get; private set; }

        //The players name
        public string m_PlayerName { get; private set; }

        //The players score
        public ulong m_Score { get; private set; }

        public uint m_Money { get; private set; }

        //The players controls
        public Controller m_PlayerControls { get; private set; }

        //The camera into the 3d environment
        public CameraSceneNode m_PlayerCamera { get; private set; }

        //private CameraSceneNode m_PlayerCamera;
        public List<InventoryItem> m_PlayerInventory { get; private set; }

        //The list of all gui elements used by the palyer
        protected List<GUIElement> m_ListOfGUIElements = null;

        public Player(Controller playerControls, IrrlichtDevice _dev, string name)
        {
            m_PlayerShip = new PlayerShip(playerControls);
            m_PlayerControls = playerControls;
            m_PlayerInventory = new List<InventoryItem>();
            m_PlayerName = name;
            //m_PlayerCamera = _dev.SceneManager.AddCameraSceneNode(m_PlayerShip.m_GameObjectMesh, new Vector3Df(0, 80, -40));
            //_dev.SceneManager.ActiveCamera = m_PlayerCamera;

            m_ListOfGUIElements = new List<GUIElement>();

            m_Money = 25;
        }
        public Player(PlayerSave gamePlayer, Controller playerControls, IrrlichtDevice _dev)
        {
            m_PlayerControls = playerControls;
            m_PlayerShip = new PlayerShip(gamePlayer.m_PlayerShip, playerControls);

            m_PlayerInventory = new List<InventoryItem>();
            foreach (InventoryItemSave iis in gamePlayer.m_PlayerInventory)
                m_PlayerInventory.Add(new InventoryItem(iis));

            m_PlayerName = gamePlayer.m_PlayerName;
            m_Money = gamePlayer.m_Money;
            m_Score = gamePlayer.m_Score;

            m_ListOfGUIElements = new List<GUIElement>();
        }

        public void BuildStats()
        {
            m_ListOfGUIElements.Clear();
            GUIImage m_StatsImage;
            Texture image = Loader.GetTexture("MenuImage.png");
            m_StatsImage = Loader.GetImage(0, 720, 400, 1000);
            m_StatsImage.Image = image;
            m_ListOfGUIElements.Add(m_StatsImage);
        }

        /// <summary>
        /// Update the player and all components attached that need updating
        /// </summary>
        /// <param name="currentState">The current state the player resides in</param>
        public void Update(State currentState)
        {
            //m_PlayerShip.Update(currentState);
            //if(currentState is GameState)
            //    m_PlayerCamera.Target = m_PlayerShip.m_GameObjectMesh.Position; //Look at the player
            //else if(currenState is PlanetState)
            //  m_PlayerCamer.Target = m_PlayerGU.m_PlanetObjectMesh.Position;
            //_camera.Rotation = _player.m_PlayerShip.m_shipMesh.Rotation; //Keep player angle

            if (currentState is GameState)
            {
                debugXOffset = m_PlayerShip.m_GameObjectMesh.Position.X - debugXOffsetPos;
                debugXOffsetPos += debugXOffset;
                debugYOffset = m_PlayerShip.m_GameObjectMesh.Position.Z - debugYOffsetPos;
                debugYOffsetPos += debugYOffset;
            }
        }
        public void RenderStats()
        {
            State.m_DrawFront.Draw("Hull:", 70, 730, new IrrlichtLime.Video.Color(255, 255, 255));
            State.m_DrawFront.Draw(m_PlayerShip.m_CurrentHull+"/"+m_PlayerShip.m_MaxHull, 110, 730, new IrrlichtLime.Video.Color(255, 255, 255));
            
            State.m_DrawFront.Draw("Shield:", 70, 750, new IrrlichtLime.Video.Color(255, 255, 255));
            State.m_DrawFront.Draw(m_PlayerShip.m_CurrentShield + "/" + m_PlayerShip.m_MaxShield, 110, 750, new IrrlichtLime.Video.Color(255, 255, 255));

            State.m_DrawFront.Draw("Money:", 160, 730, new IrrlichtLime.Video.Color(255, 255, 255));
            State.m_DrawFront.Draw(m_Money.ToString(), 200, 730, new IrrlichtLime.Video.Color(255, 255, 255));

            //State.m_DrawFront.Draw("Score:", 160, 750, new IrrlichtLime.Video.Color(255, 255, 255));
            //State.m_DrawFront.Draw(m_Score.ToString(), 200, 750, new IrrlichtLime.Video.Color(255, 255, 255));

            State.m_DrawFront.Draw("Player:", 220, 730, new IrrlichtLime.Video.Color(255, 255, 255));
            State.m_DrawFront.Draw(m_PlayerName.ToString(), 260, 730, new IrrlichtLime.Video.Color(255, 255, 255));

            State.m_DrawFront.Draw("Player Position:", 220, 750, new IrrlichtLime.Video.Color(255, 255, 255));
            State.m_DrawFront.Draw(m_PlayerShip.m_xPosition.ToString("F0") + "," + m_PlayerShip.m_zPosition.ToString("F0"), 300, 750, new IrrlichtLime.Video.Color(255, 255, 255));
        }

        /// <summary>
        /// Adds an item to the player's inventory
        /// </summary>
        /// <param name="ii"> Item to add </param>
        public void AddInventoryItem(InventoryItem ii)
        {
            if (ii.m_ItemType == InventoryItem.ItemType.Money)
                m_Money += ii.m_Cost;
            else
                m_PlayerInventory.Add(ii);
        }
        /// <summary>
        /// Removes an item from the player's inventory
        /// </summary>
        /// <param name="ii"> Item to remove </param>
        public void RemoveInventoryItem(InventoryItem ii)
        {
            m_PlayerInventory.Remove(ii);
        }

        /// <summary>
        /// Gives money from the player
        /// </summary>
        /// <param name="cash">Value to add</param>
        public void AddMoney(uint cash)
        {
            m_Money += cash;
        }
        /// <summary>
        /// Removes money from the player
        /// </summary>
        /// <param name="cash">Value to deduct</param>
        /// <returns>Returns true if money is added succesfully</returns>
        public bool DeductMoney(uint cash)
        {
            if (m_Money < cash)
                return false;

            m_Money -= cash;
            return true;
        }

        /// <summary>
        /// Removes the players gui
        /// </summary>
        public void RemoveGUI()
        {
            m_ListOfGUIElements.ForEach((e) => e.Remove());
        }
    }
}
