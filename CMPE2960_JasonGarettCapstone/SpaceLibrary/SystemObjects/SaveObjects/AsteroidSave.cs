using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class AsteroidSave : PlanetSave
    {
        public int m_toughness;

        public AsteroidSave(Asteroid a)
            : base (a)
        {
            m_toughness = a.m_toughness;
        }
        public AsteroidSave()
        { }
    }
}
