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
    public class SpaceSystem : IGameObjectUpdateable, IGDIDrawable
    {
        //The center of the system
        public Planet m_sun { get; private set; }
        //All other planets in the system
        public List<Planet> m_planets { get; private set; }
        //All the asteroids in the system
        public List<Asteroid> m_asteroids { get; private set; }
        //All space stations in the system
        public List<SpaceStation> m_SpaceStations { get; private set; }
        //All the ships in the system
        public List<Ship> m_Ships { get; private set; }
        //The system name
        public string m_SystemName { get; private set; }

        public SpaceSystem()
        {
            m_planets = new List<Planet>();
            m_asteroids = new List<Asteroid>();
            m_SpaceStations = new List<SpaceStation>();
            m_Ships = new List<Ship>();

            m_sun = new Planet(this, (Loader.PlanetType)Galaxy.m_galaxyGen.Next((int)Loader.PlanetType.Sun1, (int)Loader.PlanetType.SunMax), 1);

            int planetCount = Galaxy.m_galaxyGen.Next(5, 12);
            for (int p = 2; p <= planetCount + 1; p++)
            {
                float raduis = (float)((Galaxy.m_galaxyGen.NextDouble() * 40 + 50) * p + 60);

                m_planets.Add(new Planet(this, (Loader.PlanetType)Galaxy.m_galaxyGen.Next((int)Loader.PlanetType.Planet1, (int)Loader.PlanetType.PlanetMax), raduis, (short)p));
            }

            int asteroidBeltCount = Galaxy.m_galaxyGen.Next(1, 2);
            for (int belt = 1; belt <= asteroidBeltCount; belt++)
            {
                float BeltRad = (float)((Galaxy.m_galaxyGen.NextDouble() * 400)*belt + 200*belt);

                float beltSpeed = (float)(Galaxy.m_galaxyGen.NextDouble() * .001);

                float asteroidCount = Galaxy.m_galaxyGen.Next(80, 140);
                for (int a = 0; a < asteroidCount; a++)
                {
                    float asteroidRad = (float)(BeltRad + Galaxy.m_galaxyGen.NextDouble() * 12 - 6);
                    float animateValue = (float)(a / asteroidCount * Math.PI * 2 + (Galaxy.m_galaxyGen.NextDouble() *1-.5) / asteroidCount * Math.PI * 2);

                    Asteroid ast = new Asteroid(this, (Loader.PlanetType)Galaxy.m_galaxyGen.Next((int)Loader.PlanetType.Asteroid1, (int)Loader.PlanetType.AsteroidMax), asteroidRad, beltSpeed, animateValue);
                    ast.ThisGameObjectDied += new GameObject.delVoidGameObject(RemoveAsteroid);
                    m_asteroids.Add(ast);
                }
            }

            int spaceStationCount = Galaxy.m_galaxyGen.Next(1, 3);
            for (int i = 0; i < spaceStationCount; ++i)
            {
                int planetOrbiting = Galaxy.m_galaxyGen.Next(m_planets.Count);
                int orbitRadius = Galaxy.m_galaxyGen.Next(30, 40);

                m_SpaceStations.Add(new SpaceStation(m_planets[planetOrbiting], orbitRadius, (float)(Galaxy.m_galaxyGen.NextDouble() * .006 + .003)));
            }

            //Generate ships here
            //Have their ThisGameObjectDied event subscribe to RemoveShip
            int numShips = Galaxy.m_galaxyGen.Next(15,30);
            m_Ships = new List<Ship>();
            for (int i = 0; i < numShips; ++i)
            {
                int faction = Galaxy.m_galaxyGen.Next((int)Ship.Faction.Orange, (int)Ship.Faction.Green + 1);
                Ship shippy = new TestShip((Ship.Faction)faction);
                shippy.ThisGameObjectDied += new GameObject.delVoidGameObject(RemoveShip);
                m_Ships.Add(shippy);
            }

            //Name gen
            m_SystemName = "";
            int numChars = Galaxy.m_galaxyGen.Next(5, 21);
            for (int i = 0; i < numChars; ++i)
                m_SystemName += (char)Galaxy.m_galaxyGen.Next((int)'a', (int)'z');
        }
        public SpaceSystem(SpaceSystemSave sss)
        {
            m_sun = new Planet(sss.m_Sun);

            m_planets = new List<Planet>();
            foreach (PlanetSave ps in sss.m_Planets)
                m_planets.Add(new Planet(ps));

            m_asteroids = new List<Asteroid>();
            foreach (AsteroidSave astSave in sss.m_Asteroids)
            {
                Asteroid ast = new Asteroid(astSave);
                ast.ThisGameObjectDied += new GameObject.delVoidGameObject(RemoveAsteroid);
                m_asteroids.Add(ast);
            }

            m_SpaceStations = new List<SpaceStation>();
            foreach (SpaceStationSave stationSave in sss.m_SpaceStations)
                m_SpaceStations.Add(new SpaceStation(stationSave, m_planets.First((p) => p.m_ID == stationSave.m_OrbitingID)));

            m_Ships = new List<Ship>();
            foreach (ShipSave ss in sss.m_Ships)
            {
                Ship shippy = new TestShip(ss);
                shippy.ThisGameObjectDied += new GameObject.delVoidGameObject(RemoveShip);
                m_Ships.Add(shippy);
            }

        }

        /// <summary>
        /// Updates all game objects in the space system
        /// </summary>
        /// <param name="gs">The current state the system exists in</param>
        public void Update(GameState gs)
        {
            m_sun.Update(gs);
            m_planets.ForEach((P) => P.Update(gs));
            m_asteroids.ForEach((A) => A.Update(gs));
        }

        /// <summary>
        /// Removes an Asteroid from the list of asteroids
        /// </summary>
        /// <param name="A">The asteroid to be removed</param>
        private void RemoveAsteroid(GameObject A)
        {
            if(A != null && A is Asteroid)
                m_asteroids.Remove((Asteroid)A);
        }

        /// <summary>
        /// Removes a ship from the list of ships
        /// </summary>
        /// <param name="S">The ship to be removed</param>
        private void RemoveShip(GameObject S)
        {
            if (S != null && S is Ship)
                m_Ships.Remove((Ship)S);
        }

        /// <summary>
        /// Draws the regions of all the game objects in the system
        /// </summary>
        /// <param name="gr"></param>
        public void GDIDraw(Graphics gr)
        {
            m_sun.GDIDraw(gr);
            m_planets.ForEach((P) => P.GDIDraw(gr));
            m_asteroids.ForEach((A) => A.GDIDraw(gr));
        }

        /// <summary>
        /// Get all game objects held by the system
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameObject> GetAllGameObjects()
        {
            foreach (Asteroid a in m_asteroids)
                yield return a;
            foreach (Planet p in m_planets)
                yield return p;
            yield return m_sun;

            foreach (SpaceStation ss in m_SpaceStations)
                yield return ss;

            foreach (Ship s in m_Ships)
                yield return s;
        }
    }
}
