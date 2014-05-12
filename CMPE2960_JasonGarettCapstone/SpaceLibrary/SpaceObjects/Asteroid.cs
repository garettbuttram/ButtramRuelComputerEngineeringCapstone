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
    public class Asteroid : Planet, IHitable
    {
        //How much damage the asteroid needs to take before dieing
        public int m_toughness { get; private set; }

        public Asteroid(SpaceSystem system, Loader.PlanetType pType, float orbitRadius, float orbitSpeed, float animateValue)
            : base(system, pType, orbitRadius, 0)
        {
            //m_GameObjectMesh.SetMaterialFlag(MaterialFlag.Lighting, false);
            m_toughness = Galaxy.m_galaxyGen.Next(400,600);
            m_dOrbitSpeed = orbitSpeed;
            m_dAnimateValue = animateValue;
            m_regionRotateSpeed = (float)(m_dOrbitSpeed * 180 / Math.PI);

            m_xPosition = (float)(Math.Sin(m_dAnimateValue) * m_dOrbitRadius);
            m_zPosition = (float)(Math.Cos(m_dAnimateValue) * m_dOrbitRadius);
            m_yPosition = AsteroidYPos;

            //m_GameObjectMesh.Position = new Vector3Df(xPos, 0, zPos);
            //m_GameObjectMesh.Scale = new Vector3Df(4f, 4f, 4f);

            //Rebuild planet region
            //m_GameObjectRegion = Loader.BuildPlanetRegion(pType);
            //relocate planet region to the planet's location
            //System.Drawing.Drawing2D.Matrix tranMat = new System.Drawing.Drawing2D.Matrix();
            //tranMat.Translate(xPos, zPos);
            //m_GameObjectRegion.Transform(tranMat);
        }
        public Asteroid(AsteroidSave astSave)
            : base (astSave)
        {
            m_toughness = astSave.m_toughness;
        }

        /// <summary>
        /// Updates the asteroid removing it if it has died
        /// </summary>
        /// <param name="gs">The current state the asteroid exists in</param>
        public override void Update(GameState gs)
        {
            if (m_toughness <= 0)
            {
                MadeNewObject(new InventoryItem(m_xPosition, m_zPosition, InventoryItem.ItemType.Asteroid_Ore, true));
                GameObjectDied();
                return;
            }
            base.Update(gs);
        }

        public float GetDistanceFrom(Bullet B)
        {
            return GetDistanceBetweenMeshes(this, B);
        }

        /// <summary>
        /// Checks if the given bullet has collided with the asteroid
        /// </summary>
        /// <param name="B">The bullet to check against</param>
        /// <returns>Whether or not the bullet collided with the asteroid</returns>
        public bool CheckforCollision(Bullet B)
        {
            //RectangleF[] AsteroidRectArray = m_PlanetRegion.GetRegionScans(new System.Drawing.Drawing2D.Matrix());
            //RectangleF[] BulletRectArray = B.m_bulletRegion.GetRegionScans(new System.Drawing.Drawing2D.Matrix());

            //for (int sI = 0; sI < AsteroidRectArray.Length; ++sI)
            //    for (int bI = 0; bI < BulletRectArray.Length; ++bI)
            //        if (AsteroidRectArray[sI].Contains(BulletRectArray[bI]))
            //            return true;
            //return false;

             return CheckGameObjectCollision(this, B);
        }
        /// <summary>
        /// Asteroid hit so bullet deals damage to the asteroid
        /// </summary>
        /// <param name="B">The bullet dealing damage to the asteroid</param>
        public void DealDamage(Bullet B)
        {
            m_toughness -= B.m_FiredFrom.m_BulletDamage;
        }

        protected override MeshSceneNode LoadGameObjectMesh()
        {
            MeshSceneNode ast = base.LoadGameObjectMesh();
            //ast.Scale = new Vector3Df(4f, 4f, 4f);
            return ast;
        }
    }
}
