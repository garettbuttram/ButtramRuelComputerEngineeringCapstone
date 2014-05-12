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
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace SpaceLibrary
{
    public class SpaceStation : MeshGameObject, IGameObjectUpdateable, IGDIDrawable
    {
        //The planet the station is orbitting
        public Planet m_Orbiting { get; protected set; }
        //How quickly the space station should rotate around the planet
        public float m_dOrbitSpeed { get; protected set; }

        //The orbit radius from the planet
        public float m_dOrbitRadius { get; protected set; }

        public float m_dAnimateValue { get; protected set; }
        //The items the space station has to sell
        public List<InventoryItem> m_SellItems { get; private set; }

        public SpaceStation(Planet isOrbiting, float orbitRadius, float orbitSpeed)
            :base(new Point(0,0), 0)
        {
            m_Orbiting = isOrbiting;
            m_dOrbitRadius = orbitRadius;
            m_yPosition = Planet.planetYPos + 10;
            m_dOrbitSpeed = orbitSpeed;

            m_SellItems = new List<InventoryItem>();

            int itemsForSale = Galaxy.m_galaxyGen.Next(5, 11);
            for (int i = 0; i < itemsForSale; ++i)
            {
                InventoryItem.ItemType iType = (InventoryItem.ItemType)Galaxy.m_galaxyGen.Next((int)InventoryItem.ItemType.MiningLaser, (int)InventoryItem.ItemType.StrongMine + 1);
                m_SellItems.Add(new InventoryItem(0,0, iType, false));
            }
        }
        public SpaceStation(SpaceStationSave sss, Planet isOrbiting)
            : base(sss)
        {
            m_Orbiting = isOrbiting;

            m_dOrbitRadius = sss.m_dOrbitRadius;
            m_dOrbitSpeed = sss.m_dOrbitSpeed;
            m_dAnimateValue = sss.m_dAnimateValue;

            m_SellItems = new List<InventoryItem>();
            foreach (InventoryItemSave iis in sss.m_SellItems)
                m_SellItems.Add(new InventoryItem(iis));
        }

        public void Update(GameState gs)
        {
            float xPos = m_Orbiting.m_xPosition + (float)(Math.Sin(m_dAnimateValue) * m_dOrbitRadius);
            float zPos = m_Orbiting.m_zPosition + (float)(Math.Cos(m_dAnimateValue) * m_dOrbitRadius);

            m_xPosition = xPos;
            m_zPosition = zPos;

            float xTrans = xPos - m_GameObjectMesh.Position.X;
            float yTrans = zPos - m_GameObjectMesh.Position.Z;

            m_dAnimateValue += m_dOrbitSpeed;

            m_GameObjectMesh.Position = new Vector3Df(xPos, m_yPosition, zPos);

            System.Drawing.Drawing2D.Matrix tranMat = new System.Drawing.Drawing2D.Matrix();
            tranMat.Translate(xTrans, yTrans);
            m_GameObjectRegion.Transform(tranMat);
        }

        protected override MeshSceneNode LoadGameObjectMesh()
        { 
            MeshSceneNode m = Loader.LoadMiscMesh(Loader.MiscMesh.SpaceStation);
            m.Scale = new Vector3Df(.7f, .7f, .7f);
            return m;
        }

        protected override Region LoadGameObjectRegion()
        {
            return Loader.BuildShipRegion(Loader.ShipMeshName.BlueShip);
        }

        public void GDIDraw(Graphics gr)
        {
            gr.FillRegion(new SolidBrush(System.Drawing.Color.Green), m_GameObjectRegion);
        }
    }
}
