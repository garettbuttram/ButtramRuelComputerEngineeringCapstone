using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace SpaceLibrary
{
    public abstract class Ship : MeshGameObject, IHitable, IGameObjectUpdateable, IGDIDrawable
    {
        public enum Faction
        {
            Orange,
            Blue,
            Red,
            Green,
        }

        public enum ShipState
        {
            Roam,
            Attack,
            Flee,
            Defend,
            Dodge,
            Player
        }
        //The ships target
        public MeshGameObject m_Target { get; protected set; }
        //What state the ships is currently in
        public ShipState m_ShipState { get; protected set; }
        //What faction the ship is a part of
        public Faction m_ShipFaction { get; protected set; }
        //The fuzzy data associated with the ship
        public AIData m_ShipData { get; protected set; }

        public const int _SHIP_VIEW_RANGE = 200;
        public const int _DODGE_DISTANCE = 30;
        public const int _STOP_DODGE_DISTANCE = 45;
        public const float _CHASE_DISTANCE = _SHIP_VIEW_RANGE * 1.5f;
        public const float _WEAK_TEAM_STRENGTH = 0.7f;
        public const float _STRONG_TEAM_STRENGTH = 1.2f;
        public const float _TURN_TOLERENCE = .8f;
        public const float _FIRE_GUN_TOLERANCE = 0.3f;
        public const float _FLEE_DISTANCE = _SHIP_VIEW_RANGE * 1.75f;

        //used by npc's to set which direction they would like to be heading
        protected float m_HeadingDestination;

        //Used to rotate regions 
        //How much the regions needs to rotate
        protected float m_TotalHeadingChange;
        protected float m_TotalHeadingChangeDegrees
        {
            get
            {
                return (float)(m_TotalHeadingChange * 180 / Math.PI);
            }
        }

        //Which direction the gun is looking
        protected float m_FiringHeading;

        //The maximum heading change a ship can make in one update
        public float m_MaxTurnSpeed { get; protected set; }

        //The current speed of the ship
        public float m_CurrentSpeed { get; protected set; }
        //The max possible speed for the ship
        public float m_MaxSpeed { get; protected set; }

        //The current health of the hull
        public int m_CurrentHull { get; protected set; }
        //The maximum possible health of the hull
        public int m_MaxHull { get; protected set; }

        //The current health of the shield
        public int m_CurrentShield { get; protected set; }
        //The maximum possible health of the shield
        public int m_MaxShield { get; protected set; }

        //The Time it takes before the ship shield begins regenerating
        public int m_ShieldCooldownTime { get; protected set; }
        //The current time left in shield cooldown time
        public int m_CurrentCooldownTick { get; protected set; }
        //The rate at which the shield recharges
        public int m_ShieldRechargeRate { get; protected set; }

        //The primary weapons the ship is holding
        public List<Gun> m_PrimaryWeapons { get; protected set; }
        //Which primary weapon to use
        public int m_currentPrimWeapon { get; protected set; }
        //The secondary weapon of the ship
        public Gun m_SecondaryWeapon { get; protected set; }

        //The type of ship used to load the mesh and region
        public Loader.ShipMeshName m_ShipMeshName { get; protected set; }

        private BillboardSceneNode m_HealthBoard = null;
        private BillboardSceneNode m_ShieldBoard = null;

        private BillboardSceneNode m_FactionBoard;
        private BillboardSceneNode m_StateBoard;

        public Ship(/*Loader.ShipMeshName shipType,*/ Faction fact)
            :base( new Point(Galaxy.m_galaxyGen.Next(-500, 501), Galaxy.m_galaxyGen.Next(-500, 501)), (float)(Galaxy.m_galaxyGen.NextDouble() * Math.PI * 2))
        {
            //m_ShipMeshName = shipType;
            m_ShipFaction = fact;

            //BuildGameObjectMesh();
            //BuildGameObjectRegion();

            //Set Region position and rotation
            //System.Drawing.Drawing2D.Matrix transMat = new System.Drawing.Drawing2D.Matrix();
            //transMat.Translate(m_GameObjectMesh.Position.X, m_GameObjectMesh.Position.Z);
            //m_GameObjectRegion.Transform(transMat);
            //System.Drawing.Drawing2D.Matrix rotMat = new System.Drawing.Drawing2D.Matrix();
            //rotMat.RotateAt(-m_HeadingDegrees, new PointF(m_GameObjectMesh.Position.X, m_GameObjectMesh.Position.Z));
            //m_GameObjectRegion.Transform(rotMat);
            
            m_MaxSpeed = 5f;
            m_MaxTurnSpeed = 0.1f;

            m_PrimaryWeapons = new List<Gun>();

            m_ShipData = new AIData();
        }
        public Ship(ShipSave shippy)
            : base(shippy)
        {
            m_MaxTurnSpeed = shippy.m_MaxTurnSpeed;
            m_CurrentSpeed = shippy.m_CurrentSpeed;
            m_MaxSpeed = shippy.m_MaxSpeed;

            m_CurrentHull = shippy.m_CurrentHull;
            m_MaxHull = shippy.m_MaxHull;

            m_CurrentShield = shippy.m_CurrentShield;
            m_MaxShield = shippy.m_MaxShield;

            m_ShieldCooldownTime = shippy.m_ShieldCooldownTime;
            m_CurrentCooldownTick = shippy.m_CurrentCooldownTick;
            m_ShieldRechargeRate = shippy.m_ShieldRechargeRate;

            m_PrimaryWeapons = new List<Gun>();
            foreach(GunType gt in shippy.m_PrimaryWeapons)
                m_PrimaryWeapons.Add(new Gun(this, gt));
            m_currentPrimWeapon = shippy.m_currentPrimWeapon;

            m_SecondaryWeapon = new Gun(this, shippy.m_SecondaryWeapon);

            m_ShipFaction = shippy.m_ShipFaction;
            m_ShipState = shippy.m_ShipState;

            m_ShipData = new AIData();
        }
        /// <summary>
        /// Update the ship and all pieces of the ship that require updating
        /// </summary>
        /// <param name="currentState">The current game state the ship exists in</param>
        public virtual void Update(GameState currentState)
        {
            //I am dead remove me
            if(m_CurrentHull <= 0)
            {
                Loader.m_se.Play2D("Sound/S_Explosion2.wav");
                MadeNewObject(new InventoryItem(m_xPosition, m_zPosition, InventoryItem.ItemType.Money, true));
                GameObjectDied();
                return;
            }

            if (m_CurrentHull > m_MaxHull)
                m_CurrentHull = m_MaxHull;

            if (m_CurrentShield > m_MaxShield)
                m_CurrentShield = m_MaxShield;

            m_ShipData.Update(this);

            //hold speed to max speed
            if (m_CurrentSpeed > m_MaxSpeed)
                m_CurrentSpeed = m_MaxSpeed;
            else if (m_CurrentSpeed < -m_MaxSpeed / 2)
                m_CurrentSpeed = -m_MaxSpeed / 2;

            if (m_TotalHeadingChange > m_MaxTurnSpeed)
                m_TotalHeadingChange = m_MaxTurnSpeed;
            else if (m_TotalHeadingChange < -m_MaxTurnSpeed)
                m_TotalHeadingChange = -m_MaxTurnSpeed;

            //m_Heading += m_TotalHeadingChange;

            if (m_CurrentShield != m_MaxShield)
            {
                if (m_CurrentCooldownTick == m_ShieldCooldownTime)
                {
                    m_CurrentShield += m_ShieldRechargeRate;
                }
                else
                    m_CurrentCooldownTick++;
            }
            else
                m_CurrentCooldownTick = 0;

            m_CurrentSpeed *= .99f;

            float xDis = (float)(Math.Sin(m_Heading) * m_CurrentSpeed);
            float zDis = (float)(Math.Cos(m_Heading) * m_CurrentSpeed);

            Move(xDis, 0, zDis, m_TotalHeadingChange);

            m_PrimaryWeapons.ForEach((gun) => gun.Update());
            if(m_SecondaryWeapon != null)
                m_SecondaryWeapon.Update();
            m_TotalHeadingChange = 0;

            if (m_HealthBoard != null)
            {
                m_HealthBoard.Remove();
                m_HealthBoard = Loader.BuildBillBoard("HullBar.png");
                m_HealthBoard.Position = new Vector3Df(m_xPosition, 10, m_zPosition);
                m_HealthBoard.Height = 1;
                float width = ((float)m_CurrentHull / (float)m_MaxHull) * 10 + .01f;
                if (width <= 0)
                    m_HealthBoard.Remove();
                else{
                m_HealthBoard.TopWidth = width;
                m_HealthBoard.BottomWidth = width;}
            }
            if (m_ShieldBoard != null)
            {
                m_ShieldBoard.Remove();
                m_ShieldBoard = Loader.BuildBillBoard("SheildBar.png");
                m_ShieldBoard.Position = new Vector3Df(m_xPosition, 12, m_zPosition);
                m_ShieldBoard.Height = 1;
                float width = ((float)m_CurrentShield / (float)m_MaxShield) * 10 + .01f;
                if (width <= 0)
                    m_ShieldBoard.Remove();
                else
                {
                    m_ShieldBoard.TopWidth = width;
                    m_ShieldBoard.BottomWidth = width;
                }
            }

            //m_FactionBoard.Position = new Vector3Df(m_xPosition, 20, m_zPosition);

            //if (m_StateBoard != null)
            //{
            //    m_StateBoard.Remove();
            //    m_StateBoard = Loader.BuildTextBillboard(m_ShipState.ToString().Replace('_', ' '));
            //    m_StateBoard.Position = new Vector3Df(m_xPosition, 35, m_zPosition);
            //}
        }
        ///// <summary>
        ///// Moves the Ship
        ///// </summary>
        //protected void Move()
        //{
        //    float xDis = (float)(Math.Sin(m_Heading) * m_CurrentSpeed);
        //    float zDis = (float)(Math.Cos(m_Heading) * m_CurrentSpeed);

        //    m_GameObjectMesh.Rotation = new Vector3Df(0, m_HeadingDegrees, 0);

        //    System.Drawing.Drawing2D.Matrix rotMat = new System.Drawing.Drawing2D.Matrix();
        //    rotMat.RotateAt(-m_TotalHeadingChangeDegrees, new PointF(m_GameObjectMesh.Position.X, m_GameObjectMesh.Position.Z));
        //    m_TotalHeadingChange = 0;
        //    m_GameObjectRegion.Transform(rotMat);

        //    m_GameObjectMesh.Position = new Vector3Df(m_GameObjectMesh.Position.X + xDis, 0, m_GameObjectMesh.Position.Z + zDis);

        //    System.Drawing.Drawing2D.Matrix tranMat = new System.Drawing.Drawing2D.Matrix();
        //    tranMat.Translate(xDis, zDis);
        //    m_GameObjectRegion.Transform(tranMat);
        //}

        /// <summary>
        /// Add a bullet fired by the primary weapon to the game state
        /// </summary>
        /// <param name="gs">The game state to add the bullet to</param>
        protected void FireCurrentPrimaryWeapon()
        {
            if (m_currentPrimWeapon < m_PrimaryWeapons.Count)
                MadeNewObject(m_PrimaryWeapons[m_currentPrimWeapon].FireGun());
            else
                m_currentPrimWeapon = 0;
        }

        /// <summary>
        /// Add a bullet fired by the secondary weapon to the game state
        /// </summary>
        /// <param name="gs">The game state to add the bullet to</param>
        protected void FireSecondaryWeapon()
        {
            if(m_SecondaryWeapon != null)
                MadeNewObject(m_SecondaryWeapon.FireGun());
        }

        public float GetDistanceFrom(Bullet B)
        {
            return GetDistanceBetweenMeshes(this, B);
        }

        /// <summary>
        /// Checks if the given bullet has collided with the ship
        /// </summary>
        /// <param name="B">The bullet to check collision with</param>
        /// <returns></returns>
        public bool CheckforCollision(Bullet B)
        {
            //RectangleF[] ShipRectArray = m_shipRegion.GetRegionScans(new System.Drawing.Drawing2D.Matrix());
            //RectangleF[] BulletRectArray = B.m_bulletRegion.GetRegionScans(new System.Drawing.Drawing2D.Matrix());

            //System.Diagnostics.Stopwatch stoppy = new System.Diagnostics.Stopwatch();
            //stoppy.Start();

            //for(int sI = 0; sI < ShipRectArray.Length; ++sI)
            //    for(int bI = 0; bI < BulletRectArray.Length; ++bI)
            //        if (ShipRectArray[sI].Contains(BulletRectArray[bI]))
            //        {
            //            stoppy.Stop();
            //            Console.WriteLine("Ship collision check milliseconds : " + stoppy.ElapsedMilliseconds);
            //            return true;
            //        }
            //stoppy.Stop();
            //Console.WriteLine("Ship collision check milliseconds : " + stoppy.ElapsedMilliseconds);
            //return false;

            return CheckGameObjectCollision(this, B);
        }

        /// <summary>
        /// A bullet has hit me so it will now deal damage to me
        /// </summary>
        /// <param name="B">The bullet dealing damage</param>
        public void DealDamage(Bullet B)
        {
            //Have shield so deal damage to the shield
            if (m_CurrentShield > 0)
            {
                m_CurrentShield -= (int)Math.Max((B.m_FiredFrom.m_BulletDamage * B.m_FiredFrom.m_BulletShieldModifier), 1);
                if (m_CurrentShield < 0)
                    m_CurrentShield = 0;
            }
            //Shield down deal damage to the hull
            else
            {
                m_CurrentHull -= (int)Math.Max(B.m_FiredFrom.m_BulletDamage * B.m_FiredFrom.m_BulletHullModifier, 1);
                if (m_CurrentHull < 0)
                    m_CurrentHull = 0;
            }
            //Reset shield recharge
            m_CurrentCooldownTick = 0;
        }

        public bool TestCollision(Ship otherShip)
        {
            RectangleF[] ShipRectArray = m_GameObjectRegion.GetRegionScans(new System.Drawing.Drawing2D.Matrix());
            RectangleF[] otherShipRectArray = otherShip.m_GameObjectRegion.GetRegionScans(new System.Drawing.Drawing2D.Matrix());

            if (m_GameObjectMesh.Position.GetDistanceFrom(otherShip.m_GameObjectMesh.Position) < 20)
            {
                for (int sI = 0; sI < ShipRectArray.Length; sI++)
                    for (int osI = 0; osI < otherShipRectArray.Length; osI++)
                        if (ShipRectArray[sI].Contains(otherShipRectArray[osI]))
                            return true;
            }

            return false;
        }

        /// <summary>
        /// Draw the ships region
        /// </summary>
        /// <param name="gr"></param>
        public void GDIDraw(Graphics gr)
        {
            gr.FillRegion(new SolidBrush(System.Drawing.Color.HotPink), m_GameObjectRegion);
        }

        /// <summary>
        /// Builds the Ships Mesh
        /// </summary>
        protected override MeshSceneNode LoadGameObjectMesh()
        {
            //m_StateBoard = Loader.BuildTextBillboard(m_ShipState.ToString().Replace('_', ' '));
            //m_StateBoard.Position = new Vector3Df(m_xPosition, 35, m_zPosition);

            //m_FactionBoard = Loader.BuildTextBillboard(m_ShipFaction.ToString().Replace('_', ' '));
            //m_FactionBoard.Position = new Vector3Df(m_xPosition, 20, m_zPosition);

            m_HealthBoard = Loader.BuildBillBoard("HullBar.png");
            m_HealthBoard.Position = new Vector3Df(m_xPosition, 10, m_zPosition);

            m_ShieldBoard = Loader.BuildBillBoard("SheildBar.png");
            m_ShieldBoard.Position = new Vector3Df(m_xPosition, 12, m_zPosition);

            MeshSceneNode msn = null;

            switch (m_ShipFaction)
            {
                case Faction.Orange: 
                        msn = Loader.LoadShipMesh(Loader.ShipMeshName.OrangeShip);
                    break;
                case Faction.Blue:
                    msn = Loader.LoadShipMesh(Loader.ShipMeshName.BlueShip);
                    break;
                case Faction.Red:
                    msn = Loader.LoadShipMesh(Loader.ShipMeshName.RedShip);
                    break;
                case Faction.Green:
                    msn = Loader.LoadShipMesh(Loader.ShipMeshName.GreenShip);
                    break;
            }

            return msn;
        }

        /// <summary>
        /// Builds the Ships Region
        /// </summary>
        protected override Region LoadGameObjectRegion()
        {
            return Loader.BuildShipRegion(Loader.ShipMeshName.BlueShip);
        }

        public override void RemoveMesh()
        {
            //m_StateBoard.Remove();
            //m_FactionBoard.Remove();

            m_HealthBoard.Remove();
            m_ShieldBoard.Remove();

            base.RemoveMesh();
        }
    }
}
