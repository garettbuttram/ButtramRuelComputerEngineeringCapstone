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
    [Serializable]
    abstract public class GameObject
    {
        public delegate void delVoidGameObject(GameObject go);

        //Called when the game object needs to be removed from the state
        public event delVoidGameObject ThisGameObjectDied;
        //Called when the game object needs to add a new object to the state
        public event delVoidGameObject ThisGameObjectMadeANewObject;
        //Whether or not the objects is still alive
        public bool m_IsAlive { get; protected set; }

        //The region of the game the object is located in
        public Region m_GameObjectRegion { get; protected set; }

        public float m_xPosition { get; protected set; }
        public float m_zPosition { get; protected set; }
        public float m_yPosition { get; protected set; }

        public float m_Heading { get; protected set; }
        public float m_HeadingDegrees
        {
            get
            {
                return (float)(m_Heading * 180 / Math.PI);
            }
        }

        public GameObject(PointF position, float heading)
        {
            m_xPosition = position.X;
            m_zPosition = position.Y;

            m_Heading = heading;
            m_IsAlive = true;
        }

        public GameObject(GameObjectSave gos)
        {
            m_xPosition = gos.m_xPosition;
            m_yPosition = gos.m_yPosition;
            m_zPosition = gos.m_zPosition;

            m_Heading = gos.m_Heading;
            m_IsAlive = true;
        }

        /// <summary>
        /// The game object has died so call the death event
        /// </summary>
        public void GameObjectDied()
        {
            if (ThisGameObjectDied != null)
                ThisGameObjectDied(this);
            m_IsAlive = false;
        }

        /// <summary>
        /// The game object has made a new object so call the event
        /// </summary>
        /// <param name="go">The created Game Object</param>
        public void MadeNewObject(GameObject go)
        {
            if (ThisGameObjectMadeANewObject != null)
                ThisGameObjectMadeANewObject(go);        
        }

        /// <summary>
        /// Checks whether or not two game objects collided
        /// </summary>
        /// <param name="A">The first Game Object</param>
        /// <param name="B">The second Game Object</param>
        /// <returns>Whether or not they collided</returns>
        static public bool CheckGameObjectCollision(GameObject A, GameObject B)
        {
            Region Clone = A.m_GameObjectRegion.Clone();
            Clone.Intersect(B.m_GameObjectRegion);
            bool isEmpty = Clone.IsEmpty(StateManager.m_gr);
            return !isEmpty;
        }

        /// <summary>
        /// Build the region for the object
        /// </summary>
        public void BuildGameObjectRegion()
        {
            m_GameObjectRegion = LoadGameObjectRegion();
            //Move to the correct position
            System.Drawing.Drawing2D.Matrix tranMat = new System.Drawing.Drawing2D.Matrix();
            tranMat.Translate(m_xPosition, m_zPosition);
            m_GameObjectRegion.Transform(tranMat);

            //Rotate to correct rotation
            System.Drawing.Drawing2D.Matrix rotMat = new System.Drawing.Drawing2D.Matrix();
            rotMat.RotateAt(-m_HeadingDegrees, new PointF(m_xPosition, m_zPosition));
            m_GameObjectRegion.Transform(rotMat);
        }

        abstract protected Region LoadGameObjectRegion();

        virtual protected void Move(float xDisp, float yDisp, float zDisp, float totalHeadingChange)
        {
            m_xPosition += xDisp;
            m_zPosition += zDisp;
            m_Heading += totalHeadingChange;

            System.Drawing.Drawing2D.Matrix tranMat = new System.Drawing.Drawing2D.Matrix();
            tranMat.Translate(xDisp, zDisp);
            m_GameObjectRegion.Transform(tranMat);

            System.Drawing.Drawing2D.Matrix rotMat = new System.Drawing.Drawing2D.Matrix();
            rotMat.RotateAt(-(float)(totalHeadingChange * 180 / Math.PI), new PointF(m_xPosition, m_zPosition));
            m_GameObjectRegion.Transform(rotMat);
        }

        virtual public void PrepareForSave()
        {
            m_GameObjectRegion = null;
        }
    }
}
