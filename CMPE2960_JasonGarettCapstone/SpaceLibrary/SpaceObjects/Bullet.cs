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
    public class Bullet : MeshGameObject, IGameObjectUpdateable, IGDIDrawable
    {
        //The gun from which the bullet was fired
        public Gun m_FiredFrom { get; private set; }

        //The speed at which the bullet moves
        private float m_BulletSpeed;

        private int m_BulletLife;

        private Ship.Faction m_Faction;
        private Ship m_Target;

        private const float _TURN_TOLERANCE = 0.4f;

        protected float m_TotalHeadingChange;
        protected float m_TotalHeadingChangeDegrees
        {
            get
            {
                return (float)(m_TotalHeadingChange * 180 / Math.PI);
            }
        }

        public Bullet(Gun firedFrom)
            :base(new Point(0,0), 0)
        {
            m_FiredFrom = firedFrom;
            m_Heading = firedFrom.m_GunHeading;
            m_BulletLife = firedFrom.m_BulletLife;
            m_Faction = m_FiredFrom.m_WhosFiring.m_ShipFaction;

            if(m_FiredFrom.m_WhosFiring.m_Target is Ship)
                m_Target = (Ship)m_FiredFrom.m_WhosFiring.m_Target;

            float xDis = 0;
            float zDis = 0;

            switch (m_FiredFrom.m_GunType)
            {
                case GunType.WeakLaser:
                case GunType.Laser:
                case GunType.StrongLaser:
                case GunType.MiningLaser:
                    xDis = (float)(Math.Sin(m_Heading) * 10);
                    zDis = (float)(Math.Cos(m_Heading) * 10);
                    break;
                case GunType.WeakBullet:
                case GunType.Bullet:
                case GunType.StrongBullet:
                case GunType.Missile:
                case GunType.WeakMissile:
                case GunType.StrongMissile:
                    if (m_FiredFrom.m_firingRight)
                    {
                        xDis = (float)(Math.Sin(m_Heading - Gun.BULLET_ANGLE_OFFSET) * Gun.GUN_OFFSET);
                        zDis = (float)(Math.Cos(m_Heading - Gun.BULLET_ANGLE_OFFSET) * Gun.GUN_OFFSET);
                    }
                    else
                    {
                        xDis = (float)(Math.Sin(m_Heading + Gun.BULLET_ANGLE_OFFSET) * Gun.GUN_OFFSET);
                        zDis = (float)(Math.Cos(m_Heading + Gun.BULLET_ANGLE_OFFSET) * Gun.GUN_OFFSET);
                    }
                    break;
                case GunType.WeakMine:
                case GunType.Mine:
                case GunType.StrongMine:
                    xDis = 0;
                    zDis = 0;
                    break;
                default:
                    throw new Exception("This shouldn't be here");
            }
            //BuildGameObjectMesh();
            //BuildGameObjectRegion();

            m_xPosition = firedFrom.m_WhosFiring.m_GameObjectMesh.Position.X + xDis;
            m_zPosition = firedFrom.m_WhosFiring.m_GameObjectMesh.Position.Z + zDis;


            float BulletSpeedAdjust = (m_FiredFrom.m_WhosFiring.m_CurrentSpeed > 0) ? m_FiredFrom.m_WhosFiring.m_CurrentSpeed : 0;
                m_BulletSpeed = firedFrom.m_BulletSpeed + BulletSpeedAdjust;

            if (m_FiredFrom.m_GunType == GunType.WeakMine && m_FiredFrom.m_GunType == GunType.Mine && m_FiredFrom.m_GunType == GunType.StrongMine)
                m_BulletSpeed = 0;
            //Set bullet position and rotation
            //m_GameObjectMesh.Position = firedFrom.m_WhosFiring.m_GameObjectMesh.Position;
            //m_GameObjectMesh.Rotation = new Vector3Df(0, m_HeadingDegrees, 0);
            ////Set Region position and rotation
            //System.Drawing.Drawing2D.Matrix transMat = new System.Drawing.Drawing2D.Matrix();
            //transMat.Translate(m_GameObjectMesh.Position.X, m_GameObjectMesh.Position.Z);
            //m_GameObjectRegion.Transform(transMat);
            //System.Drawing.Drawing2D.Matrix rotMat = new System.Drawing.Drawing2D.Matrix();
            //rotMat.RotateAt(-m_HeadingDegrees, new PointF(m_GameObjectMesh.Position.X, m_GameObjectMesh.Position.Z));
            //m_GameObjectRegion.Transform(rotMat);
        }

        /// <summary>
        /// Updates the bullet
        /// </summary>
        /// <param name="currentState">The current state of the game</param>
        public void Update(GameState currentState)
        {
            float xStart = m_xPosition;
            float yStart = m_zPosition;

            float xDis = (float)(Math.Sin(m_Heading) * m_BulletSpeed);
            float zDis = (float)(Math.Cos(m_Heading) * m_BulletSpeed);

            GunType gt = m_FiredFrom.m_GunType;
            if (gt == GunType.WeakMissile || gt == GunType.Missile || gt == GunType.StrongMissile )
            {
                if (m_Target == null || !m_Target.m_IsAlive || !(m_Target is Ship))
                {
                    IEnumerable<GameObject> possibleTargets = currentState.GetAllGameObjects((go) => go.m_IsAlive && go is Ship 
                                                                                              && ((Ship)go).m_ShipFaction != m_Faction);
                    m_Target = (Ship)possibleTargets.FirstOrDefault((s) => GetDistanceBetweenMeshes(this, (MeshGameObject)s) 
                                                     == possibleTargets.Min((min) => GetDistanceBetweenMeshes(this, (MeshGameObject)min)));
                }

                if (m_Target != null && m_Target.m_IsAlive && m_Target is Ship)
                {
                    double localXDistance = (m_Target.m_xPosition - m_xPosition) * Math.Cos(-m_Heading) + (m_Target.m_zPosition - m_zPosition) * Math.Sin(-m_Heading);
                    double localZDistance = -(m_Target.m_xPosition - m_xPosition) * Math.Sin(-m_Heading) + (m_Target.m_zPosition - m_zPosition) * Math.Cos(-m_Heading);

                    if (localXDistance < -_TURN_TOLERANCE)
                        m_TotalHeadingChange -= 0.005f;
                    else if (localXDistance > _TURN_TOLERANCE)
                        m_TotalHeadingChange += 0.005f;
                }
            }

            Move(xDis, 0, zDis, m_TotalHeadingChange);
            m_TotalHeadingChange = 0;

            float xEnd = m_xPosition;
            float yEnd = m_zPosition;

            if ((m_FiredFrom.m_GunType != GunType.MiningLaser && m_FiredFrom.m_GunType != GunType.WeakLaser && m_FiredFrom.m_GunType != GunType.Laser && m_FiredFrom.m_GunType != GunType.StrongLaser) && (xStart != xEnd || yStart != yEnd))
            {
                GraphicsPath bPath = new GraphicsPath();
                bPath.AddLine(xStart, yStart, xEnd, yEnd);
                Pen p = new Pen(System.Drawing.Color.HotPink, 1.5f);
                p.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
                bPath.Widen(p);
                m_GameObjectRegion = new Region(bPath);
            }

            //Find anything the bullet is hitting getting only the first found if multiple
            float distanceCheck = 3.5f;
            if (m_FiredFrom.m_GunType == GunType.MiningLaser || m_FiredFrom.m_GunType == GunType.WeakLaser || m_FiredFrom.m_GunType == GunType.Laser || m_FiredFrom.m_GunType == GunType.StrongLaser)
                distanceCheck = 25f;
            IHitable hit;
            if (m_FiredFrom.m_WhosFiring is PlayerShip)
            {
                hit = (IHitable)currentState.GetAllGameObjects((go) => go is IHitable)
                                            .FirstOrDefault((hitCheck) => hitCheck != m_FiredFrom.m_WhosFiring &&
                                                                          ((IHitable)hitCheck).GetDistanceFrom(this) < distanceCheck &&
                                                                          ((IHitable)hitCheck).CheckforCollision(this));
                if (hit is Ship && ((Ship)hit).m_ShipFaction == m_Faction)
                    hit = null;
            }
            else
                hit = (IHitable)currentState.GetAllGameObjects((go) => go is IHitable && go is Ship && ((Ship)go).m_ShipFaction != m_Faction)
                                            .FirstOrDefault((hitCheck) => hitCheck != m_FiredFrom.m_WhosFiring &&
                                                                          ((IHitable)hitCheck).GetDistanceFrom(this) < distanceCheck &&
                                                                          ((IHitable)hitCheck).CheckforCollision(this));
            //Deal damage only if we actually hit something
            if (hit != null)
            {
                hit.DealDamage(this);
                GameObjectDied();
                return;
            }
            else if (m_BulletLife == 0)
            {
                GameObjectDied();
                return;
            }

            m_BulletLife--;
        }

        ///// <summary>
        ///// Moves the bullet
        ///// </summary>
        //private void Move()
        //{
        //    float xDis = (float)(Math.Sin(m_Heading) * m_BulletSpeed);
        //    float zDis = (float)(Math.Cos(m_Heading) * m_BulletSpeed);

        //    m_GameObjectMesh.Position = new Vector3Df(m_GameObjectMesh.Position.X + xDis, 0, m_GameObjectMesh.Position.Z + zDis);

        //    System.Drawing.Drawing2D.Matrix tranMat = new System.Drawing.Drawing2D.Matrix();
        //    tranMat.Translate(xDis, zDis);
        //    m_GameObjectRegion.Transform(tranMat);

        //    Move();
        //}

        /// <summary>
        /// Draws the bullet region
        /// </summary>
        /// <param name="gr"></param>
        public void GDIDraw(Graphics gr)
        {
            gr.FillRegion(new SolidBrush(System.Drawing.Color.Green), m_GameObjectRegion);
        }

        /// <summary>
        /// Builds the bullets mesh
        /// </summary>
        protected override MeshSceneNode LoadGameObjectMesh()
        {
            return Loader.LoadBulletMesh(m_FiredFrom.m_GunType);
        }

        /// <summary>
        /// Builds the bullets region
        /// </summary>
        protected override Region LoadGameObjectRegion()
        {
            return Loader.BuildBulletRegion(m_FiredFrom.m_GunType);
        }
    }
}
