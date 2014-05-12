using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class PlanetSave : GameObjectSave
    {
        //The planets orbit speed
        public float m_dOrbitSpeed;
        //How far from the sun the planet rotates
        public float m_dOrbitRadius;
        //The current animation stage
        public float m_dAnimateValue;
        //The planet type
        public Loader.PlanetType m_PlanetType;

        //The speed at which the planet is rotating
        public float m_RotationX;
        public float m_RotationY;
        public float m_RotationZ;

        public short m_ID;

        public PlanetSave(Planet p)
            : base(p)
        {
            m_dOrbitSpeed = p.m_dOrbitSpeed;
            m_dOrbitRadius = p.m_dOrbitRadius;
            m_dAnimateValue = p.m_dAnimateValue;

            m_PlanetType = p.m_PlanetType;

            m_RotationX = p.m_RotationX;
            m_RotationY = p.m_RotationY;
            m_RotationZ = p.m_RotationZ;

            m_ID = p.m_ID;
        }
        public PlanetSave()
        { }
    }
}
