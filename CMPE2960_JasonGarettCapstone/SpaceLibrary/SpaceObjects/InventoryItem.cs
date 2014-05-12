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
    public class InventoryItem : MeshGameObject, IGameObjectUpdateable
    {
        public enum ItemType
        {
            Asteroid_Ore = 998,
            Money,
            MiningLaser = 1000,
            WeakLaser,
            Laser,
            StrongLaser,
            WeakBullet,
            Bullet,
            StrongBullet,
            WeakMissile,
            Missile,
            StrongMissile,
            WeakMine,
            Mine,
            StrongMine
        };

        public ItemType m_ItemType { get; private set; }

        public BillboardTextSceneNode m_ItemIdentifier { get; private set; }

        public bool m_Lootable { get; private set; }

        public uint m_Cost { get; private set; }

        public static Dictionary<ItemType, uint> m_GunValue;

        static InventoryItem()
        {
            m_GunValue = new Dictionary<ItemType, uint>();

            foreach (ItemType item in Enum.GetValues(typeof(ItemType)))
            {
                switch (item)
                {
                    case ItemType.MiningLaser:
                        m_GunValue.Add(item, 75);
                        break;
                    case ItemType.WeakLaser:
                        m_GunValue.Add(item, 10);
                        break;
                    case ItemType.Laser:
                        m_GunValue.Add(item, 35);
                        break;
                    case ItemType.StrongLaser:
                        m_GunValue.Add(item, 100);
                        break;
                    case ItemType.WeakBullet:
                        m_GunValue.Add(item, 15);
                        break;
                    case ItemType.Bullet:
                        m_GunValue.Add(item, 25);
                        break;
                    case ItemType.StrongBullet:
                        m_GunValue.Add(item, 125);
                        break;
                    case ItemType.WeakMissile:
                        m_GunValue.Add(item, 20);
                        break;
                    case ItemType.Missile:
                        m_GunValue.Add(item, 100);
                        break;
                    case ItemType.StrongMissile:
                        m_GunValue.Add(item, 150);
                        break;
                    case ItemType.WeakMine:
                        m_GunValue.Add(item, 15);
                        break;
                    case ItemType.Mine:
                        m_GunValue.Add(item, 45);
                        break;
                    case ItemType.StrongMine:
                        m_GunValue.Add(item, 75);
                        break;
                }
            }

        }
        
        public InventoryItem(float xPos, float zPos, ItemType it, bool startsLootable)
            :base(new PointF(xPos,zPos), 0)
        {
            m_xPosition = xPos;
            m_yPosition = 0;
            m_zPosition = zPos;

            m_ItemType = it;
            m_Lootable = startsLootable;

            switch (m_ItemType)
            {
                case ItemType.Asteroid_Ore:
                    int rare = Galaxy.m_galaxyGen.Next(101);
                    m_Cost = rare >= 98 ? (uint)Galaxy.m_galaxyGen.Next(50, 71) : (uint)Galaxy.m_galaxyGen.Next(10, 31);
                    break;
                case ItemType.Money:
                    int lots = Galaxy.m_galaxyGen.Next(101);
                    m_Cost = lots >= 95 ? (uint)Galaxy.m_galaxyGen.Next(30, 51) : (uint)Galaxy.m_galaxyGen.Next(5, 26);
                    break;
                case ItemType.MiningLaser:
                case ItemType.WeakLaser:
                case ItemType.Laser:
                case ItemType.StrongLaser:
                case ItemType.WeakBullet:
                case ItemType.Bullet:
                case ItemType.StrongBullet:
                case ItemType.WeakMissile:
                case ItemType.Missile:
                case ItemType.StrongMissile:
                case ItemType.WeakMine:
                case ItemType.Mine:
                case ItemType.StrongMine:
                    m_Cost = m_GunValue[m_ItemType];
                    break;
            }
        }
        public InventoryItem(InventoryItemSave iis)
            :base(iis)
        {
            m_ItemType = iis.m_ItemType;
            m_Cost = iis.m_Cost;
        }

        public void Update(GameState currentState)
        {
            m_GameObjectMesh.Rotation = new Vector3Df(0, m_GameObjectMesh.Rotation.Y + .01f, 0);

            if (m_Lootable)
            {
                if (isInRangeOfPlayer(this, currentState.m_GamePlayer.m_PlayerShip))
                {
                    currentState.m_GamePlayer.AddInventoryItem(this);
                    //m_ItemIdentifier.Remove();
                    GameObjectDied();
                    return;
                }
            }
            else
                if (!isInRangeOfPlayer(this, currentState.m_GamePlayer.m_PlayerShip))
                    m_Lootable = true;
        }
        protected override Region LoadGameObjectRegion()
        {
            return Loader.BuildPlanetRegion(Loader.PlanetType.Asteroid1);   
        }
        protected override MeshSceneNode LoadGameObjectMesh()
        {
            m_ItemIdentifier = Loader.BuildTextBillboard(m_ItemType.ToString().Replace('_', ' '));
            m_ItemIdentifier.Position = new Vector3Df(m_xPosition, 5, m_zPosition);

            switch (m_ItemType)
            {
                case ItemType.Money:
                    string texture = "bronze.png";
                    if (m_Cost > 45)
                        texture = "gold.png";
                    else if (m_Cost > 20)
                        texture = "silver.png";
                    return Loader.LoadMoneyMesh(texture);
                default:
                    return Loader.LoadPlanetMesh(Loader.PlanetType.Asteroid1);
            }
        }
        static public bool isInRangeOfPlayer(InventoryItem ii, Ship ps)
        {
            return GetDistanceBetweenMeshes(ii, ps) < 5;
        }

        public override void RemoveMesh()
        {
            base.RemoveMesh();
            m_ItemIdentifier.Remove();
        }
    }
}
