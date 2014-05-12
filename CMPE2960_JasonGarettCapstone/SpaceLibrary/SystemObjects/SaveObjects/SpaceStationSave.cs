using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class SpaceStationSave : GameObjectSave
    {
        //The planet the station is orbitting
        //public Planet m_Orbiting { get; protected set; }
        //How quickly the space station should rotate around the planet
        public float m_dOrbitSpeed;

        //The orbit radius from the planet
        public float m_dOrbitRadius;

        public float m_dAnimateValue;
        //The items the space station has to sell
        public List<InventoryItemSave> m_SellItems;

        public short m_OrbitingID;

        public SpaceStationSave(SpaceStation ss)
            : base (ss)
        {
            m_dOrbitSpeed = ss.m_dOrbitSpeed;
            m_dOrbitRadius = ss.m_dOrbitRadius;
            m_dAnimateValue = ss.m_dAnimateValue;

            m_SellItems = new List<InventoryItemSave>();
            foreach (InventoryItem ii in ss.m_SellItems)
                m_SellItems.Add(new InventoryItemSave(ii));

            m_OrbitingID = ss.m_Orbiting.m_ID;
        }
        public SpaceStationSave()
        { }
    }
}
