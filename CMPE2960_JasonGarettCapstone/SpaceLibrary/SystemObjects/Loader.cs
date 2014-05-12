using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;
using IrrKlang;
using System.Drawing.Drawing2D;

namespace SpaceLibrary
{
    public class Loader
    {
        //The irrlicht device meshes need to be registered with to be shown
        static public IrrlichtDevice m_dev;

        //The graphics context regions are built in
        static public Graphics m_gr;

        //The sound engine to play sounds from
        static public ISoundEngine m_se;

        static public Random m_rng = new Random();

        static public void LoadStuff()
        {
            m_se = new ISoundEngine();
            float defVol = m_se.SoundVolume;
            m_se.SoundVolume = 0;
            //load audio
            {
                m_se.Play2D("Sound/M_BattleSounds.wav");
                m_se.Play2D("Sound/M_Eerie.wav");
                m_se.Play2D("Sound/M_Ethereal.wav");
                m_se.Play2D("Sound/S_Beep.wav");
                m_se.Play2D("Sound/S_Explosion1.wav");
                m_se.Play2D("Sound/S_Explosion2.wav");
                m_se.Play2D("Sound/S_MachineGun1.wav");
                m_se.Play2D("Sound/S_MachineGun2.wav");
                m_se.Play2D("Sound/S_MachineGun3.wav");
                m_se.Play2D("Sound/S_MachineGun4.wav");
                m_se.Play2D("Sound/S_MuffledExplosion1.wav");
                m_se.Play2D("Sound/S_MuffledExplosion2.wav");
                m_se.Play2D("Sound/S_MuffledExplosion3.wav");
                m_se.Play2D("Sound/S_Pistol1.wav");
                m_se.Play2D("Sound/S_Pistol2.wav");
                m_se.Play2D("Sound/S_Pistol3.wav");
                m_se.Play2D("Sound/S_Pistol4.wav");
                m_se.Play2D("Sound/S_Plasma1.wav");
                m_se.Play2D("Sound/S_Plasma2.wav");
            }
            MeshSceneNode temp;
            foreach (ShipMeshName item in Enum.GetValues(typeof(ShipMeshName)))
                try { 
                    temp = Loader.LoadShipMesh(item);
                    temp.Remove();
                }
                catch { }
            foreach (GunType item in Enum.GetValues(typeof(GunType)))
                try
                {
                    temp = Loader.LoadBulletMesh(item);
                    temp.Remove();
                }
                catch { }
            foreach (PlanetType item in Enum.GetValues(typeof(PlanetType)))
                try
                {
                    temp = Loader.LoadPlanetMesh(item);
                    temp.Remove();
                }
                catch { }

            m_se.StopAllSounds();
            m_se.SoundVolume = defVol;
        }

        #region ShipRegion
        /// <summary>
        /// Possible Meshes a ship can have
        /// </summary>
        public enum ShipMeshName
        {
            RedShip = 998,
            BlueShip,
            GreenShip,
            OrangeShip
        };
        /// <summary>
        /// Loads a mesh and registers it with the scenemanager based on a name
        /// </summary>
        /// <param name="mName">The name of the mesh to be loaded</param>
        /// <returns>The loaded mesh</returns>
        static public MeshSceneNode LoadShipMesh(ShipMeshName mName)
        {
            switch (mName)
            {
                case ShipMeshName.RedShip:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("galaga.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("galagatextureRed.jpg"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case ShipMeshName.BlueShip:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("galaga.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("galagatextureBlue.jpg"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case ShipMeshName.GreenShip:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("galaga.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("galagatextureGreen.jpg"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case ShipMeshName.OrangeShip:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("galaga.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("galagatextureOrange.jpg"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                default:
                    throw new ArgumentException("Attempting to load a mesh that doesn't exist.");
            }
        }
        /// <summary>
        /// Builds the region for the ship based on a mesh
        /// </summary>
        /// <param name="mName">The ship name</param>
        /// <returns>The created region</returns>
        static public Region BuildShipRegion(ShipMeshName mName)
        {
            GraphicsPath gp = new GraphicsPath();
            PointF[] path = null;
            switch (mName)
            {
                case ShipMeshName.RedShip:
                case ShipMeshName.BlueShip:
                case ShipMeshName.GreenShip:
                case ShipMeshName.OrangeShip:
                    path = new PointF[14];
                    path[0] = new PointF(0f,-3.75f);
                    path[1] = new PointF(1f,-2.8f);
                    path[2] = new PointF(2.5f, -2.8f);
                    path[3] = new PointF(3.75f, -4f);
                    path[4] = new PointF(4f, -4f);
                    path[5] = new PointF(4f, 1.2f);
                    path[6] = new PointF(2.2f, -1.2f);
                    path[7] = new PointF(0f, 4f);
                    path[8] = new PointF(-2.2f, -1.2f);
                    path[9] = new PointF(-4f, 1.2f);
                    path[10] = new PointF(-4f, -4f);
                    path[11] = new PointF(-3.75f, -4f);
                    path[12] = new PointF(-2.5f, -2.8f);
                    path[13] = new PointF(-1f,-2.8f);
                    gp.StartFigure();
                    gp.AddLines(path);
                    gp.CloseFigure();
                    return new Region(gp);
                default:
                    throw new ArgumentException("Region isn't built yet.");
            }
        }
        #endregion

        #region BulletRegion
        /// <summary>
        /// Loads the mesh of a bullet based on it's the type of gun it is fired by
        /// </summary>
        /// <param name="mName">The type of gun firing the bullet</param>
        /// <returns>The loaded mesh</returns>
        static public MeshSceneNode LoadBulletMesh(GunType mName)
        {
            switch (mName)
            {
                case GunType.MiningLaser:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Laser.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("MissileLaserTexture4.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        msn.SetMaterialType(MaterialType.TransparentAlphaChannel);
                        return msn;
                    }
                case GunType.WeakLaser:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Laser.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("MissileLaserTexture1.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        msn.SetMaterialType(MaterialType.TransparentAlphaChannel);
                        return msn;
                    }
                case GunType.Laser:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Laser.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("MissileLaserTexture2.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        msn.SetMaterialType(MaterialType.TransparentAlphaChannel);
                        return msn;
                    }
                case GunType.StrongLaser:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Laser.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("MissileLaserTexture3.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        msn.SetMaterialType(MaterialType.TransparentAlphaChannel);
                        return msn;
                    }
                case GunType.WeakBullet:
                    {
                        m_se.Play2D("Sound/S_Beep.wav");
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("PlasmaBullet.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("PlasmaBullet1.jpg"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case GunType.Bullet:
                    {
                        m_se.Play2D("Sound/S_Beep.wav");
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("PlasmaBullet.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("PlasmaBullet2.jpg"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case GunType.StrongBullet:
                    {
                        m_se.Play2D("Sound/S_Beep.wav");
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("PlasmaBullet.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("PlasmaBullet3.jpg"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case GunType.WeakMissile:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Missile.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("MissileLaserTexture1.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case GunType.Missile:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Missile.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("MissileLaserTexture2.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case GunType.StrongMissile:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Missile.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("MissileLaserTexture3.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case GunType.WeakMine:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Mine.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("Mine1.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case GunType.Mine:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Mine.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("Mine2.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case GunType.StrongMine:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Mine.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("Mine3.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                default:
                    throw new ArgumentException("Attempting to load a mesh that doesn't exist.");
            }
        }
        /// <summary>
        /// Builds the region for the bullet
        /// </summary>
        /// <param name="mName">The type of gun firing the bullet</param>
        /// <returns>The region built</returns>
        static public Region BuildBulletRegion(GunType mName)
        {
            GraphicsPath gp = new GraphicsPath();
            PointF[] path = null;
            switch (mName)
            {
                case GunType.WeakBullet:
                case GunType.Bullet:
                case GunType.StrongBullet:
                case GunType.WeakMissile:
                case GunType.Missile:
                case GunType.StrongMissile:
                case GunType.WeakMine:
                case GunType.Mine:
                case GunType.StrongMine:
                    path = new PointF[1];
                    break;
                case GunType.MiningLaser:
                case GunType.WeakLaser:
                case GunType.Laser:
                case GunType.StrongLaser:
                    gp.AddLine(0, -5, 0, 20);
                    Pen p = new Pen(System.Drawing.Color.HotPink, 5);
                    p.SetLineCap(LineCap.Round, LineCap.Round, DashCap.Round);
                    gp.Widen(p);
                    return new Region(gp);
                default:
                    throw new ArgumentException("Region isn't built yet.");
            }

            gp.StartFigure();
            gp.AddLines(path);
            gp.CloseFigure();
            return new Region(gp);
        }
        #endregion

        #region PlanetRegion
        /// <summary>
        /// Possible types a planet can be
        /// </summary>
        public enum PlanetType
        {
            Sun1,
            Sun2,
            Sun3,
            SunMax,
            Planet1,
            PlanetMax,
            Asteroid1,
            Asteroid2,
            Asteroid3,
            AsteroidMax
        };
        /// <summary>
        /// Loads the planets mesh based on its type
        /// </summary>
        /// <param name="mName">The planet type</param>
        /// <returns>The loaded mesh</returns>
        static public MeshSceneNode LoadPlanetMesh(PlanetType mName)
        {
            switch (mName)
            {
                case PlanetType.Sun1:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Planet.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("Sun1.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case PlanetType.Sun2:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Planet.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("Sun2.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case PlanetType.Sun3:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Planet.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("Sun3.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case PlanetType.SunMax:
                    throw new ArgumentException("This isn't a thing");
                case PlanetType.Planet1:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Planet.obj"));
                        msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture("Planet1.png"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        Vector3Df boop = msn.Scale;
                        return msn;
                    }
                case PlanetType.PlanetMax:
                    throw new ArgumentException("This isn't a thing");
                case PlanetType.Asteroid1:
                    return m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Asteroid1.obj"));
                case PlanetType.Asteroid2:
                    return m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Asteroid2.obj"));
                case PlanetType.Asteroid3:
                    return m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Asteroid3.obj"));
                case PlanetType.AsteroidMax:
                    throw new ArgumentException("This isn't a thing");
                default:
                    throw new ArgumentException("Attempting to load a mesh that doesn't exist.");
            }
        }
        /// <summary>
        /// Builds the region for the planet
        /// </summary>
        /// <param name="mName">The planet type</param>
        /// <returns>The built region</returns>
        static public Region BuildPlanetRegion(PlanetType mName)
        {
            GraphicsPath gp = new GraphicsPath();
            PointF[] path = null;

            switch (mName)
            {
                default:
                    path = new PointF[8];
                    path[0] = new PointF(0f, 3f);
                    path[1] = new PointF(2.25f, 2.25f);
                    path[2] = new PointF(3, 0f);
                    path[3] = new PointF(2.25f, -2.25f);
                    path[4] = new PointF(0f, -3);
                    path[5] = new PointF(-2.25f, -2.25f);
                    path[6] = new PointF(-3, 0f);
                    path[7] = new PointF(-2.25f, 2.25f);
                    gp.StartFigure();
                    gp.AddLines(path);
                    gp.CloseFigure();
                    return new Region(gp);
                    //throw new ArgumentException("Region isn't built yet.");
            }
        }
        #endregion

        #region HappenRegionRegion
        static public Region BuildHappenRegion(HappenRegionShape shape)
        {
            GraphicsPath gp = new GraphicsPath();
            PointF[] path = null;
            switch (shape)
            {
                case HappenRegionShape.TestShape:
                    path = new PointF[8];
                    path[0] = new PointF(0f, 3f);
                    path[1] = new PointF(2, 2);
                    path[2] = new PointF(3, 0f);
                    path[3] = new PointF(2, -2);
                    path[4] = new PointF(0f, -3);
                    path[5] = new PointF(-2, -2);
                    path[6] = new PointF(-3, 0f);
                    path[7] = new PointF(-2, 2);
                    gp.StartFigure();
                    gp.AddLines(path);
                    gp.CloseFigure();
                    return new Region(gp);

                default:
                    throw new ArgumentException("Region isn't built yet.");
            }
        }
        #endregion

        public enum MiscMesh
        {
            SpaceStation,
            Spacefloor
        };

        static public MeshSceneNode LoadMoneyMesh(string texture)
        {
            MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("Money.obj"));
            msn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture(texture));
            msn.SetMaterialFlag(MaterialFlag.Lighting, false);
            return msn;
        }

        static public MeshSceneNode LoadMiscMesh(MiscMesh mName)
        {
            switch (mName)
            {
                case MiscMesh.SpaceStation:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("SpaceStation.obj"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                case MiscMesh.Spacefloor:
                    {
                        MeshSceneNode msn = m_dev.SceneManager.AddMeshSceneNode(m_dev.SceneManager.GetMesh("SpaceFloor.obj"));
                        msn.SetMaterialFlag(MaterialFlag.Lighting, false);
                        return msn;
                    }
                default:
                    throw new ArgumentException("Attempting to load a mesh that doesn't exist.");
            }
        }

        static public BillboardTextSceneNode BuildTextBillboard(string text)
        {
             return m_dev.SceneManager.AddBillboardTextSceneNode(text);
        }
        static public BillboardSceneNode BuildBillBoard(string material)
        {
            BillboardSceneNode bsn = m_dev.SceneManager.AddBillboardSceneNode();

            bsn.SetMaterialTexture(0, m_dev.VideoDriver.GetTexture(material));

            return bsn;
        }
        static public Texture GetTexture(string image)
        {
            return m_dev.VideoDriver.GetTexture(image);
        }
        static public IrrlichtLime.GUI.GUIImage GetImage(int xStart, int yStart, int xEnd, int yEnd)
        {
            return m_dev.GUIEnvironment.AddImage(new Recti(xStart, yStart, xEnd, yEnd));
        }
        static public void MuteAudio()
        {
            m_se.SoundVolume = 0;
        }
        static public void EnableAudio()
        {
            m_se.SoundVolume = 1;
        }
    }
}
