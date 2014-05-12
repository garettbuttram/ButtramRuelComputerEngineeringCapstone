using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class SpaceSystemSave
    {
        //The saved sun of the system
        public PlanetSave m_Sun;
        //The saved planets in the system
        public List<PlanetSave> m_Planets;
        //The saved asteroids in the system
        public List<AsteroidSave> m_Asteroids;
        //The saved space stations
        public List<SpaceStationSave> m_SpaceStations;
        //The saved ships in the system
        public List<ShipSave> m_Ships;

        public SpaceSystemSave(SpaceSystem ss)
        {
            m_Sun = new PlanetSave(ss.m_sun);

            m_Planets = new List<PlanetSave>();
            foreach (Planet p in ss.m_planets)
                m_Planets.Add(new PlanetSave(p));

            m_Asteroids = new List<AsteroidSave>();
            foreach (Asteroid a in ss.m_asteroids)
                m_Asteroids.Add(new AsteroidSave(a));

            m_SpaceStations = new List<SpaceStationSave>();
            foreach (SpaceStation spaceStation in ss.m_SpaceStations)
                m_SpaceStations.Add(new SpaceStationSave(spaceStation));

            m_Ships = new List<ShipSave>();
            foreach (Ship s in ss.m_Ships)
                m_Ships.Add(new ShipSave(s));

        }
        public SpaceSystemSave()
        { }
    }
}
