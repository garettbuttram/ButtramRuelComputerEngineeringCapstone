using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

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
    abstract public class MeshGameObject : GameObject
    {
        //The Mesh shown on screen when the object is displayed
        public MeshSceneNode m_GameObjectMesh { get; protected set; }

        public MeshGameObject(PointF position, float heading)
            :base(position, heading)
        { }

        public MeshGameObject(GameObjectSave gos)
            : base(gos)
        { }

        /// <summary>
        /// Gets the distance between game object meshes
        /// </summary>
        /// <param name="A">The first Mesh Game Object</param>
        /// <param name="B">The second Mesh Game Object</param>
        /// <returns></returns>
        static public float GetDistanceBetweenMeshes(MeshGameObject A, MeshGameObject B)
        {
            double xDisp = Math.Pow(A.m_GameObjectMesh.Position.X - B.m_GameObjectMesh.Position.X, 2);
            double yDisp = Math.Pow(A.m_GameObjectMesh.Position.Z - B.m_GameObjectMesh.Position.Z, 2);
            float dist = (float)Math.Sqrt(xDisp + yDisp);
            return dist;
        }

        /// <summary>
        /// Loads in the game object Mesh
        /// </summary>
        public void BuildGameObjectMesh()
        {
            m_GameObjectMesh = LoadGameObjectMesh();

            m_GameObjectMesh.Position = new Vector3Df(m_xPosition, m_yPosition, m_zPosition);
            m_GameObjectMesh.Rotation = new Vector3Df(0, m_HeadingDegrees, 0);
        }

        /// <summary>
        /// Removes the game object Mesh
        /// </summary>
        public virtual void RemoveMesh()
        {
            if(m_IsAlive)
                m_GameObjectMesh.Remove();
        }

        /// <summary>
        /// Moves the games object
        /// </summary>
        /// <param name="xDis">x move</param>
        /// <param name="yDis">y move</param>
        /// <param name="zDis">z move</param>
        /// <param name="totalHeadingChange">heading change</param>
        protected override void Move(float xDis, float yDis, float zDis, float totalHeadingChange)
        {
            base.Move(xDis, yDis, zDis, totalHeadingChange);

            m_GameObjectMesh.Position = new Vector3Df(m_GameObjectMesh.Position.X + xDis, m_GameObjectMesh.Position.Y + yDis, m_GameObjectMesh.Position.Z + zDis);
            m_GameObjectMesh.Rotation = new Vector3Df(0, m_HeadingDegrees, 0);
        }

        abstract protected MeshSceneNode LoadGameObjectMesh();

        public override void PrepareForSave()
        {
            m_GameObjectMesh = null;
            base.PrepareForSave();
        }
    }
}
