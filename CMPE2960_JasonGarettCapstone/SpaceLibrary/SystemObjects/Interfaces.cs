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
    /// <summary>
    /// Allows an object to be hit by bullets
    /// </summary>
    public interface IHitable
    {
        float GetDistanceFrom(Bullet B);
        bool CheckforCollision(Bullet B);
        void DealDamage(Bullet B);
    }

    /// <summary>
    /// Allows a game object to be updated
    /// </summary>
    public interface IGameObjectUpdateable
    {
        void Update(GameState currentState);
    }

    /// <summary>
    /// Allows a state to be updated
    /// </summary>
    public interface IStateUpdateable
    {
        void Update();
    }

    /// <summary>
    /// Allows the state to draw it's drawable componants
    /// </summary>
    public interface IStateDrawable
    {
        void Draw();
    }

    /// <summary>
    /// Allows an objects GDI+ Region to be drawn
    /// </summary>
    public interface IGDIDrawable
    {
        void GDIDraw(Graphics gr);
    }
}
