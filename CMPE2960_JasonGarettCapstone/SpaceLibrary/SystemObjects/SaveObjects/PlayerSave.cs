using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class PlayerSave
    {
        //The players ship
        public ShipSave m_PlayerShip;
        //The players name
        public string m_PlayerName;
        //The players score
        public ulong m_Score;
        //How much money the player has
        public uint m_Money;
        //The players inventory
        public List<InventoryItemSave> m_PlayerInventory;

        public PlayerSave(Player gamePlayer)
        {
            m_PlayerShip = new ShipSave(gamePlayer.m_PlayerShip);
            m_PlayerName = gamePlayer.m_PlayerName;
            m_Score = gamePlayer.m_Score;
            m_Money = gamePlayer.m_Money;

            m_PlayerInventory = new List<InventoryItemSave>();
            foreach (InventoryItem ii in gamePlayer.m_PlayerInventory)
                m_PlayerInventory.Add(new InventoryItemSave(ii));
        }
        public PlayerSave()
        { }
    }
}
