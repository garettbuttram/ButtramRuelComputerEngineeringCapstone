using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class ShipSave : GameObjectSave
    {
        //The maximum heading change a ship can make in one update
        public float m_MaxTurnSpeed;

        //The current speed of the ship
        public float m_CurrentSpeed;
        //The max possible speed for the ship
        public float m_MaxSpeed;

        //The current health of the hull
        public int m_CurrentHull;
        //The maximum possible health of the hull
        public int m_MaxHull;

        //The current health of the shield
        public int m_CurrentShield;
        //The maximum possible health of the shield
        public int m_MaxShield;

        //The Time it takes before the ship shield begins regenerating
        public int m_ShieldCooldownTime;
        //The current time left in shield cooldown time
        public int m_CurrentCooldownTick;
        //The rate at which the shield recharges
        public int m_ShieldRechargeRate;

        //The primary weapons the ship is holding
        public List<GunType> m_PrimaryWeapons;
        //Which primary weapon to use
        public int m_currentPrimWeapon;
        //The secondary weapon of the ship
        public GunType m_SecondaryWeapon;

        //What faction the ship belongs to
        public Ship.Faction m_ShipFaction; 
        //The ships current state
        public Ship.ShipState m_ShipState;

        public ShipSave(Ship s)
            : base (s)
        {
            m_MaxTurnSpeed = s.m_MaxTurnSpeed;
            m_CurrentSpeed = s.m_CurrentSpeed;
            m_MaxSpeed = s.m_MaxSpeed;

            m_CurrentHull = s.m_CurrentHull;
            m_MaxHull = s.m_MaxHull;

            m_CurrentShield = s.m_CurrentShield;
            m_MaxShield = s.m_MaxShield;

            m_ShieldCooldownTime = s.m_ShieldCooldownTime;
            m_CurrentCooldownTick = s.m_CurrentCooldownTick;
            m_ShieldRechargeRate = s.m_ShieldRechargeRate;

            m_PrimaryWeapons = new List<GunType>();
            foreach (Gun g in s.m_PrimaryWeapons)
                m_PrimaryWeapons.Add(g.m_GunType);

            m_currentPrimWeapon = s.m_currentPrimWeapon;
            m_SecondaryWeapon = s.m_SecondaryWeapon.m_GunType;

            m_ShipFaction = s.m_ShipFaction;
            m_ShipState = s.m_ShipState;
        }
        public ShipSave()
        { }
    }
}
