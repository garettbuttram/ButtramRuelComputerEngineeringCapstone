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
    [Serializable]
    public class Galaxy
    {
        //Used to generate the systems
        public static Random m_galaxyGen = new Random();

        //The current system the player is in
        public SpaceSystem m_currentLevel { get; private set; }
        //All systems in the galaxy
        public List<SpaceSystem> m_AllSystems = new List<SpaceSystem>();

        public Galaxy()
        {
            m_currentLevel = new SpaceSystem();
            m_AllSystems = new List<SpaceSystem>();
            m_AllSystems.Add(m_currentLevel);
        }
        public Galaxy(GalSave gal)
        {
            m_currentLevel = new SpaceSystem(gal.m_CurrentSystem);

            m_AllSystems = new List<SpaceSystem>();
            foreach(SpaceSystemSave sss in gal.m_AllSystems)
                m_AllSystems.Add(new SpaceSystem(sss));
        }

        /// <summary>
        /// Gets the level the player is currently in
        /// </summary>
        /// <returns>The system the player is in</returns>
        public SpaceSystem GetCurrentLevel()
        {
            return m_currentLevel;
        }

        /// <summary>
        /// Create a new system
        /// </summary>
        public void CreateNewSystemAndSetAsCurrent()
        {
            SpaceSystem ss = new SpaceSystem();
            m_currentLevel = ss;
            m_AllSystems.Add(ss);
        }

        public void SetCurrentSystemByName(string systemChoice)
        {
            SpaceSystem choice;
            if ((choice = m_AllSystems.FirstOrDefault((ss) => ss.m_SystemName == systemChoice)) != null)
                m_currentLevel = choice;
        }
    }
}
