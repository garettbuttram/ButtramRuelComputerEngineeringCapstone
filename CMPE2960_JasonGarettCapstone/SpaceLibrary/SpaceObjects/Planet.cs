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
    public class Planet : MeshGameObject, IGameObjectUpdateable, IGDIDrawable
    {
        public const short planetYPos = -20;
        public const float AsteroidYPos = 0;

        //How quickly the planet moves around its sun
        public float m_dOrbitSpeed { get; protected set; }
        //Speed of the planet's region to rotate around the sun (in degrees)
        public float m_regionRotateSpeed { get; protected set; }

        //How quickly the planet rotates
        public float m_dOrbitRadius { get; protected set; }

        public float m_dAnimateValue { get; protected set; }
        
        //protected Vector3Df m_rotationValue;

        //The speed at which the planet is rotating
        public float m_RotationX { get; protected set; }
        public float m_RotationY { get; protected set; }
        public float m_RotationZ { get; protected set; }

        public short m_ID { get; protected set; }

        public Loader.PlanetType m_PlanetType { get; protected set; }

        public Planet(SpaceSystem system, Loader.PlanetType pType, short ID)
            :base(new PointF(0,0),0)
        {
            m_PlanetType = pType;
            m_yPosition = planetYPos;
            m_ID = ID;

            //BuildGameObjectMesh();
            //BuildGameObjectRegion();

            //m_GameObjectMesh.Position = new Vector3Df(0, planetYPos, 0);
            //m_GameObjectMesh.Scale = new Vector3Df(30, 30, 30); //Later on randomize planet size by a little bit

            m_dOrbitSpeed = 0;
            m_dAnimateValue = 0;

            m_RotationX = (float)(Galaxy.m_galaxyGen.NextDouble() * .5 - 1);
            m_RotationY = 0;
            m_RotationZ = (float)(Galaxy.m_galaxyGen.NextDouble() * .5 - 1);
        }
        public Planet(SpaceSystem system, Loader.PlanetType pType, float orbitRadius, short ID)
            :base(new PointF(0,0), 0)
        {
            m_PlanetType = pType;

            m_dOrbitRadius = orbitRadius;

            m_dOrbitSpeed = (float)Galaxy.m_galaxyGen.NextDouble() * .001f + .001f;
            m_regionRotateSpeed = (float)(m_dOrbitSpeed * 180 / Math.PI);
            m_dAnimateValue = (float)(Galaxy.m_galaxyGen.NextDouble() * Math.PI / 2);

            m_RotationX = (float)(Galaxy.m_galaxyGen.NextDouble() * .5 - 1);
            m_RotationY = 0;
            m_RotationZ = (float)(Galaxy.m_galaxyGen.NextDouble() * .5 - 1);

            m_xPosition = (float)(Math.Sin(m_dAnimateValue) * m_dOrbitRadius);
            m_zPosition = (float)(Math.Cos(m_dAnimateValue) * m_dOrbitRadius);
            m_yPosition = planetYPos;

            m_ID = ID;

            //BuildGameObjectMesh();
            //BuildGameObjectRegion();
        }
        public Planet(PlanetSave ps)
            : base (ps)
        {
            m_dOrbitSpeed = ps.m_dOrbitSpeed;
            m_dOrbitRadius = ps.m_dOrbitRadius;
            m_dAnimateValue = ps.m_dAnimateValue;
            m_PlanetType = ps.m_PlanetType;

            m_RotationX = ps.m_RotationX;
            m_RotationY = ps.m_RotationY;
            m_RotationZ = ps.m_RotationZ;

            m_ID = ps.m_ID;
        }

        /// <summary>
        /// Updates the planet moving and rotating it
        /// </summary>
        /// <param name="gs">The current state the planet exists in</param>
        public virtual void Update(GameState gs)
        {
            float xPos = (float)(Math.Sin(m_dAnimateValue) * m_dOrbitRadius);
            float zPos = (float)(Math.Cos(m_dAnimateValue) * m_dOrbitRadius);

            m_xPosition = xPos;
            m_zPosition = zPos;

            float xTrans = xPos - m_GameObjectMesh.Position.X;
            float yTrans = zPos - m_GameObjectMesh.Position.Z;

            m_dAnimateValue += m_dOrbitSpeed;

            m_GameObjectMesh.Position = new Vector3Df(xPos, m_yPosition, zPos);
            m_GameObjectMesh.Rotation = new Vector3Df(m_GameObjectMesh.Rotation.X + m_RotationX, m_GameObjectMesh.Rotation.Y + m_RotationY, m_GameObjectMesh.Rotation.Z + m_RotationZ);

            System.Drawing.Drawing2D.Matrix tranMat = new System.Drawing.Drawing2D.Matrix();
            tranMat.Translate(xTrans, yTrans);
            m_GameObjectRegion.Transform(tranMat);
        }

        /// <summary>
        /// Draws the gdi+ region of the planet
        /// </summary>
        /// <param name="gr"></param>
        public void GDIDraw(Graphics gr)
        {
            gr.FillRegion(new SolidBrush(System.Drawing.Color.BlueViolet), m_GameObjectRegion);
        }

        protected override MeshSceneNode LoadGameObjectMesh()
        {
            return Loader.LoadPlanetMesh(m_PlanetType);

            ////Set the planet to it's location
            //float xPos = (float)(Math.Sin(m_dAnimateValue) * m_dOrbitRadius);
            //float zPos = (float)(Math.Cos(m_dAnimateValue) * m_dOrbitRadius);
            //m_GameObjectMesh.Position = new Vector3Df(xPos, 0, zPos);
            //m_GameObjectMesh.Scale = new Vector3Df(30, 30, 30);
            ////relocate planet region to the planet's location
            //System.Drawing.Drawing2D.Matrix tranMat = new System.Drawing.Drawing2D.Matrix();
            //tranMat.Translate(xPos, zPos);
            //m_GameObjectRegion.Transform(tranMat);
        }

        /// <summary>
        /// Builds the planets region
        /// </summary>
        protected override Region LoadGameObjectRegion()
        {
            return Loader.BuildPlanetRegion(m_PlanetType);
        }
    }
}
