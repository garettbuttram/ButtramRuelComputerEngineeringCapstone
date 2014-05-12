using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using IrrlichtLime;
using IrrlichtLime.GUI;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace SpaceLibrary
{
    public enum HappenRegionShape
    {
        TestShape,
    }

    abstract public class HappenRegion : GameObject, IGameObjectUpdateable, IGDIDrawable
    {
        protected HappenRegionShape m_RegionShape;

        public HappenRegion(HappenRegionShape shape, int xPos, int zPos)
            :base(new Point(xPos, zPos), 0)
        {
            m_RegionShape = shape;
        }

        /// <summary>
        /// Updates the happen region
        /// Checks if any player collides with the region
        /// And if one does happen
        /// </summary>
        /// <param name="gs">The current game state</param>
        public void Update(GameState gs)
        {
            if (gs.GetAllGameObjects((go) => go is PlayerShip).FirstOrDefault((go) => CheckGameObjectCollision(this, go)) != null)
                Happen(gs);
        }

        abstract protected void Happen(GameState gs);

        /// <summary>
        /// Draw the ships region
        /// </summary>
        /// <param name="gr"></param>
        public void GDIDraw(Graphics gr)
        {
            gr.FillRegion(new SolidBrush(System.Drawing.Color.Blue), m_GameObjectRegion);
        }

        protected override Region LoadGameObjectRegion()
        {
            return Loader.BuildHappenRegion(m_RegionShape);
        }
    }
}
