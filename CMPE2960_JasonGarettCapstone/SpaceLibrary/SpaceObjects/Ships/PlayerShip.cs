using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;
using System.Drawing.Drawing2D;

namespace SpaceLibrary
{
    public enum Upgrades
    {
        Shield,
        Hull,
        RechargeRate,
        CooldownTime
    }

    public class PlayerShip : Ship
    {
        //The controls to control the ship
        public Controller m_PlayerControls { get; private set; }

        private const int _UPGRADE_SHIELD = 10;
        private const int _UPGRADE_HULL = 25;
        private const int _UPGRADE_RECHARGE_RATE = 1;
        private const int _UPGRADE_SHIELD_COOLDOWN = 5;
        private const int _REPAIR_HULL = 25;

        private const int _HULL_MAXIMUM_UPGRADE = 300;
        private const int _SHIELD_MAXIMUM_UPGRADE = 150;
        private const int _RECHARGE_MAXIMUM_UPGRADE = 8;
        private const int _COOLDOWN_MINIMUM_UPGRADE = 75;

        private const float _TARGET_TOLERANCE = 1.0f;

        public PlayerShip(Controller playerControls)
            : base(Faction.Orange)
        {
            m_ShipState = ShipState.Player;

            m_PlayerControls = playerControls;

            m_CurrentHull = m_MaxHull = 100;
            m_CurrentShield = m_MaxShield = 30;

            m_ShieldCooldownTime = 175;
            m_ShieldRechargeRate = 2;

            m_PrimaryWeapons.Add(new Gun(this, GunType.WeakBullet));
            m_SecondaryWeapon = new Gun(this, GunType.None);
        }

        public PlayerShip(ShipSave shippy, Controller playerControls)
            : base(shippy)
        {
            m_PlayerControls = playerControls;
        }
        /// <summary>
        /// Updates the players ship
        /// mostly based off keys hit
        /// </summary>
        /// <param name="currentState">The current state the planet exsist in</param>
        public override void Update(GameState currentState)
        {
            //I am dead so game over
            if (m_CurrentHull <= 0)
            {
                currentState.GameOver();
                return;
            }

            if (m_PlayerControls.isPressed(m_PlayerControls.Forward))
                m_CurrentSpeed += .1f;
            else if (m_PlayerControls.isPressed(m_PlayerControls.Reverse))
                m_CurrentSpeed -= .1f;

            if (m_PlayerControls.isPressed(m_PlayerControls.Left))
            { m_TotalHeadingChange -= .05f; }
            else if (m_PlayerControls.isPressed(m_PlayerControls.Right))
            { m_TotalHeadingChange += .05f; }

            if(m_PlayerControls.isNewlyPressed(m_PlayerControls.ChangeWeapon))
            {
                m_currentPrimWeapon++;
                if(m_currentPrimWeapon == m_PrimaryWeapons.Count)
                    m_currentPrimWeapon = 0;
            }

            if (m_PlayerControls.isPressed(m_PlayerControls.FirePrimary))
                FireCurrentPrimaryWeapon();
            else if (m_PlayerControls.isPressed(m_PlayerControls.FireSecondary))
                FireSecondaryWeapon();

            m_PlayerControls.Update();
            base.Update(currentState);
        }

        /// <summary>
        /// Moves the player ship to a random point inside the system
        /// </summary>
        public void MoveToRandom()
        {
            m_xPosition = Galaxy.m_galaxyGen.Next(-1000, 1001);
            m_zPosition = Galaxy.m_galaxyGen.Next(-1000, 1001);
            m_Heading = (float)(Galaxy.m_galaxyGen.NextDouble() * Math.PI * 2);
        }

        //Faces the player towards the center of the system
        public void TowardsCenter()
        {
            m_Heading = (float)(Math.Atan2(-m_xPosition, -m_zPosition));
        }

        /// <summary>
        /// Called when the player chooses to stay within the system instead of leaving 
        /// Returning the player into bounds
        /// </summary>
        public void MoveInward()
        {
            float xDis = (float)(Math.Sin(m_Heading) * 50);
            float zDis = (float)(Math.Cos(m_Heading) * 50);

            Move(xDis, 0, zDis, 0);
        }

        /// <summary>
        /// Check whether the ship currently has the given guntype
        /// </summary>
        /// <param name="gType">The gun type to check for</param>
        /// <returns></returns>
        public bool HasWeapon(GunType gType)
        {
            if (gType == GunType.MiningLaser)
                return m_SecondaryWeapon == null;

            return m_PrimaryWeapons.FirstOrDefault((g) => g.m_GunType == gType) != null;
        }

        /// <summary>
        /// Adds the guntype to the available weapons for the player
        /// </summary>
        /// <param name="gType">the gun to add</param>
        public void EquipWeapon(GunType gType)
        {
            if (gType == GunType.MiningLaser)
                m_SecondaryWeapon = new Gun(this, GunType.MiningLaser);
            else
                m_PrimaryWeapons.Add(new Gun(this, gType));
        }

        /// <summary>
        /// Removes the given weapon from the player
        /// </summary>
        /// <param name="gType">The gun to remove</param>
        public void RemoveWeapon(GunType gType)
        {
            if (gType == GunType.MiningLaser)
                m_SecondaryWeapon = null;
            else
                m_PrimaryWeapons.RemoveAll((g) => g.m_GunType == gType);
        }

        /// <summary>
        /// Upgrades the ship by the const ammount
        /// </summary>
        /// <param name="upgrade">what is being upgraded</param>
        public void UpgradeShip(Upgrades upgrade)
        {
            switch (upgrade)
            {
                case Upgrades.Hull:
                    m_MaxHull += _UPGRADE_HULL;
                    m_CurrentHull += _UPGRADE_HULL;
                    break;
                case Upgrades.Shield:
                    m_MaxShield += _UPGRADE_SHIELD;
                    m_CurrentShield += _UPGRADE_SHIELD;
                    break;
                case Upgrades.RechargeRate:
                    m_ShieldRechargeRate += _UPGRADE_RECHARGE_RATE;
                    break;
                case Upgrades.CooldownTime:
                    m_ShieldCooldownTime -= _UPGRADE_SHIELD_COOLDOWN;
                    break;
            }
        }

        /// <summary>
        /// Checks if the given upgrade is capable of being upgraded
        /// </summary>
        /// <param name="upgrade">The upgrade being checked</param>
        /// <returns>Whether or not the ship is upgradeable</returns>
        public bool CheckUpgradeAble(Upgrades upgrade)
        {
            switch (upgrade)
            {
                case Upgrades.Hull:
                    return m_MaxHull < _HULL_MAXIMUM_UPGRADE;
                case Upgrades.Shield:
                    return m_MaxShield < _SHIELD_MAXIMUM_UPGRADE;
                case Upgrades.RechargeRate:
                    return m_ShieldRechargeRate < _RECHARGE_MAXIMUM_UPGRADE;
                case Upgrades.CooldownTime:
                    return m_ShieldCooldownTime > _COOLDOWN_MINIMUM_UPGRADE;
            }
            return false;
        }

        /// <summary>
        /// Checks if the ship needs repair
        /// </summary>
        /// <returns></returns>
        public bool CheckRepair()
        {
            return m_CurrentHull < m_MaxHull;
        }

        /// <summary>
        /// Repairs the ship hull
        /// </summary>
        /// <param name="repairAmmount"></param>
        public void RepairShip(uint repairAmount)
        {
            m_CurrentHull += (int)repairAmount;
            if (m_CurrentHull > m_MaxHull)
                m_CurrentHull = m_MaxHull;
        }
    }
}
