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
    public class SpaceStationState : State
    {
        private enum SpaceStationControls
        {
            Buy = 301,
            Sell,
            UpgradeHull,
            UpgradeShield,
            UpgradeShieldCooldown,
            UpgradeShieldRecharge,
            RepairHull,
            Close
        }

        private const float _SELL_PRICE_MOD = 0.75f;
        private const int _UPGRADE_BUTTON_DISPLACEMENT = 75;

        private const uint _UPGRADE_HULL_COST = 200;
        private const uint _UPGRADE_SHIELD_COST = 300;
        private const uint _UPGRADE_COOLDOWN_COST = 400;
        private const uint _UPGRADE_RECHARGE_COST = 100;
        private const uint _REPAIR_HULL_COST = 25;

        private const uint _REPAIR_AMOUNT = 25;

        private Player m_GamePlayer;

        public GUITable m_InvTable { get; private set; }
        public GUITable m_SSTable { get; private set; }

        private GUIImage m_BackImage;

        public List<InventoryItem> m_ListItemsToSell { get; private set; }
        public List<InventoryItem> m_ListPlayerInventory { get; private set; }

        public SpaceStationState(IrrlichtDevice dev, Player player, List<InventoryItem> selling)
            : base(dev)
        {
            Texture image = m_dev.VideoDriver.GetTexture("MenuImage.png");
            m_BackImage = m_env.AddImage(new Recti((StateManager.m_ScreenDimensions.Width - 773) / 2, (StateManager.m_ScreenDimensions.Height - 690) / 2, StateManager.m_ScreenDimensions.Width, StateManager.m_ScreenDimensions.Height));
            m_BackImage.Image = image;

            m_GamePlayer = player;
            m_ListOfGUIElements.Add(m_InvTable = m_env.AddTable(new Recti(200, 200, 400, 500)));
            m_InvTable.AddColumn("Inventory Item");
            m_InvTable.SetColumnWidth(0, 99);
            m_InvTable.AddColumn("Sell Value");
            m_InvTable.SetColumnWidth(0, 99);

            m_ListOfGUIElements.Add(m_SSTable = m_env.AddTable(new Recti(400, 200, 600, 500)));
            m_SSTable.AddColumn("Item");
            m_SSTable.SetColumnWidth(0, 99);
            m_SSTable.AddColumn("Price");
            m_SSTable.SetColumnWidth(1, 99);

            m_ListPlayerInventory = new List<InventoryItem>();
            PopulatePlayerInventory();

            m_ListItemsToSell = selling;

            int i = 0;
            foreach (InventoryItem ii in m_ListItemsToSell)
            {
                m_SSTable.AddRow(i);
                m_SSTable.SetCellText(i, 0, ii.m_ItemType.ToString().Replace('_', ' '));
                m_SSTable.SetCellData(i, 0, i);
                m_SSTable.SetCellText(i, 1, ii.m_Cost.ToString());
                ++i;
            }

            int halfScreenWidth = (int)(StateManager.m_ScreenDimensions.Width / 2);
            int halfScreenHeight = (int)(StateManager.m_ScreenDimensions.Height / 2);

            int buttonWidth = 100;
            int buttonHeight = 75;
            int startx;
            int starty;

            Recti buttRect = new Recti(275,
                                     starty = 150 + halfScreenHeight,
                                     275 + buttonWidth,
                                     starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)SpaceStationControls.Sell, "Sell", "Sell the currently selected item"));

            buttRect = new Recti(475,
                                 starty = 150 + halfScreenHeight,
                                 475 + buttonWidth,
                                 starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)SpaceStationControls.Buy, "Buy", "Buy the current selected item"));


            buttonWidth = 150;
            buttRect = new Recti(startx = (int)(halfScreenWidth * 1.5 - 100),
                                 starty = (int)(halfScreenHeight * 0.5),
                                 startx + buttonWidth,
                                 starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)SpaceStationControls.UpgradeHull, "Upgrade Hull \n        " + _UPGRADE_HULL_COST, "Upgrades the ship's hull"));

            buttRect = new Recti(startx,
                                starty += _UPGRADE_BUTTON_DISPLACEMENT,
                                startx + buttonWidth,
                                starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)SpaceStationControls.UpgradeShield, "Upgrade Shield \n          " + _UPGRADE_SHIELD_COST, "Upgrades the ship's shield"));

            buttRect = new Recti(startx,
                                starty += _UPGRADE_BUTTON_DISPLACEMENT,
                                startx + buttonWidth,
                                starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)SpaceStationControls.UpgradeShieldCooldown, "Upgrade Shield Cooldown \n                   " + _UPGRADE_COOLDOWN_COST, "Upgrades the ship's shield cooldown"));


            buttRect = new Recti(startx,
                               starty += _UPGRADE_BUTTON_DISPLACEMENT,
                               startx + buttonWidth,
                               starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)SpaceStationControls.UpgradeShieldRecharge, "Upgrade Shield Recharge \n                   " + _UPGRADE_RECHARGE_COST, "Upgrades the ship's shield recharge rate"));

            buttRect = new Recti(startx,
                               starty += _UPGRADE_BUTTON_DISPLACEMENT,
                               startx + buttonWidth,
                               starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)SpaceStationControls.RepairHull, "Repair Hull \n        " + _REPAIR_HULL_COST , "Repairs the ship's hull"));

            buttonWidth = 50;
            buttonHeight = 40;
            startx = StateManager.m_ScreenDimensions.Width - buttonWidth - 25;
            starty = StateManager.m_ScreenDimensions.Height - buttonHeight - 25;

           buttRect = new Recti(startx, starty, startx + buttonWidth, starty + buttonHeight);
            m_ListOfGUIElements.Add(m_env.AddButton(buttRect, null, (int)SpaceStationControls.Close, "Close", "Return to the game"));
            m_ListOfShownGUIElements.AddRange(m_ListOfGUIElements);
        }

        /// <summary>
        /// Handles events in this state
        /// </summary>
        /// <param name="evnt">The event</param>
        /// <returns>Whether or not the event is done being handled</returns>
        public override bool HandleEvent(Event evnt)
        {
            switch (evnt.Type)
            {
                case EventType.Key:
                    return HandleKeyEvent(evnt);
                case EventType.GUI:
                    return HandleGUIEvent(evnt);
            }
            return false;
        }

        public bool HandleKeyEvent(Event evnt)
        {
            if (evnt.Key.PressedDown)
                if (evnt.Key.Key == KeyCode.KeyC)
                {
                    StateFinished();
                    return true;
                }
            return false;
        }

        public bool HandleGUIEvent(Event evnt)
        {
            if (evnt.GUI.Type == GUIEventType.ButtonClicked)
            {
                switch ((SpaceStationControls)evnt.GUI.Caller.ID)
                {
                    //Just need to finish the buying and selling of weapons here
                    case SpaceStationControls.Buy:
                        if (m_SSTable.SelectedRowIndex <= -1 || m_SSTable.SelectedRowIndex > m_ListItemsToSell.Count)
                            return true;

                        InventoryItem selectedToBuy = m_ListItemsToSell[m_SSTable.SelectedRowIndex];
                        if (m_GamePlayer.DeductMoney(selectedToBuy.m_Cost))
                        {

                            if ((int)selectedToBuy.m_ItemType >= (int)InventoryItem.ItemType.MiningLaser &&
                               (int)selectedToBuy.m_ItemType <= (int)InventoryItem.ItemType.StrongMine)
                                m_GamePlayer.m_PlayerShip.EquipWeapon((GunType)selectedToBuy.m_ItemType);
                            else
                                m_GamePlayer.AddInventoryItem(selectedToBuy);
                        }
                        PopulatePlayerInventory();
                        return true;
                    case SpaceStationControls.Sell:
                        if (m_InvTable.SelectedRowIndex <= -1 || m_InvTable.SelectedRowIndex > m_ListPlayerInventory.Count)
                            return true;

                        InventoryItem selectedToSell = m_ListPlayerInventory[m_InvTable.SelectedRowIndex];
                        m_GamePlayer.AddMoney((uint)(selectedToSell.m_Cost * _SELL_PRICE_MOD));

                        if ((int)selectedToSell.m_ItemType >= (int)InventoryItem.ItemType.MiningLaser &&
                               (int)selectedToSell.m_ItemType <= (int)InventoryItem.ItemType.StrongMine)
                            m_GamePlayer.m_PlayerShip.RemoveWeapon((GunType)selectedToSell.m_ItemType);
                        else
                            m_GamePlayer.RemoveInventoryItem(selectedToSell);

                        PopulatePlayerInventory();
                        return true;
                    case SpaceStationControls.UpgradeHull:
                        if(m_GamePlayer.m_PlayerShip.CheckUpgradeAble(Upgrades.Hull) 
                           && m_GamePlayer.DeductMoney(_UPGRADE_HULL_COST))
                            m_GamePlayer.m_PlayerShip.UpgradeShip(Upgrades.Hull);
                        return true;
                    case SpaceStationControls.UpgradeShield:
                        if(m_GamePlayer.m_PlayerShip.CheckUpgradeAble(Upgrades.Shield)
                           && m_GamePlayer.DeductMoney(_UPGRADE_SHIELD_COST))
                            m_GamePlayer.m_PlayerShip.UpgradeShip(Upgrades.Shield);
                        return true;
                    case SpaceStationControls.UpgradeShieldCooldown:
                        if(m_GamePlayer.m_PlayerShip.CheckUpgradeAble(Upgrades.CooldownTime) 
                           && m_GamePlayer.DeductMoney(_UPGRADE_COOLDOWN_COST))
                            m_GamePlayer.m_PlayerShip.UpgradeShip(Upgrades.CooldownTime);
                        return true;
                    case SpaceStationControls.UpgradeShieldRecharge:
                        if(m_GamePlayer.m_PlayerShip.CheckUpgradeAble(Upgrades.RechargeRate) && 
                           m_GamePlayer.DeductMoney(_UPGRADE_RECHARGE_COST))
                            m_GamePlayer.m_PlayerShip.UpgradeShip(Upgrades.RechargeRate);
                        return true;
                    case SpaceStationControls.RepairHull:
                        if(m_GamePlayer.m_PlayerShip.CheckRepair() 
                           && m_GamePlayer.DeductMoney(_REPAIR_HULL_COST))
                            m_GamePlayer.m_PlayerShip.RepairShip(_REPAIR_AMOUNT);
                        return true;
                    case SpaceStationControls.Close:
                        StateFinished();
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Ends the current state
        /// </summary>
        public override void EndState()
        {
            m_BackImage.Remove();
            base.EndState();
        }

        /// <summary>
        /// Builds the player inventory table
        /// </summary>
        private void PopulatePlayerInventory()
        {
            Dictionary<InventoryItem, int> m_Inv = new Dictionary<InventoryItem, int>();

            m_InvTable.ClearRows();
            m_ListPlayerInventory.Clear();
            int i = 0;
            foreach (InventoryItem item in m_GamePlayer.m_PlayerInventory)
            {
                m_ListPlayerInventory.Add(item);

                m_InvTable.AddRow(i);
                m_InvTable.SetCellText(i, 0, item.m_ItemType.ToString().Replace('_', ' '));
                m_InvTable.SetCellData(i, 0, i);
                m_InvTable.SetCellText(i, 1, ((uint)(item.m_Cost * _SELL_PRICE_MOD)).ToString());
                ++i;
            }

            foreach (Gun g in m_GamePlayer.m_PlayerShip.m_PrimaryWeapons)
            {
                if (g.m_GunType == GunType.None)
                    continue;

                InventoryItem ii = new InventoryItem(0, 0, (InventoryItem.ItemType)g.m_GunType, false);
                m_ListPlayerInventory.Add(ii);

                m_InvTable.AddRow(i);
                m_InvTable.SetCellText(i, 0, ii.m_ItemType.ToString().Replace('_', ' '));
                m_InvTable.SetCellData(i, 0, i);
                m_InvTable.SetCellText(i, 1, ((uint)(ii.m_Cost * _SELL_PRICE_MOD)).ToString());
                ++i;
            }

            Gun second = m_GamePlayer.m_PlayerShip.m_SecondaryWeapon;
            if(second != null && second.m_GunType != GunType.None)
            {
                InventoryItem ii = new InventoryItem(0, 0, (InventoryItem.ItemType)second.m_GunType, false);

                m_InvTable.AddRow(i);
                m_InvTable.SetCellText(i, 0, ii.m_ItemType.ToString().Replace('_', ' '));
                m_InvTable.SetCellData(i, 0, i);
                m_InvTable.SetCellText(i, 1, ((uint)(ii.m_Cost * _SELL_PRICE_MOD)).ToString());
                ++i;
            }

            if(m_InvTable.RowCount > 0)
                m_InvTable.SelectedRowIndex = 0;
        }
    }
}
