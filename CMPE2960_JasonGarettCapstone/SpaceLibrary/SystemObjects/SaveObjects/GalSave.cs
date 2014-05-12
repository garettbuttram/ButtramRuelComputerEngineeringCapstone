using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class GalSave
    {
        //The saved system the player is currently in
        public SpaceSystemSave m_CurrentSystem;
        //All systems in the players game
        public List<SpaceSystemSave> m_AllSystems;

        public GalSave(Galaxy gal)
        {
            m_CurrentSystem = new SpaceSystemSave(gal.m_currentLevel);

            m_AllSystems = new List<SpaceSystemSave>();
            foreach (SpaceSystem ss in gal.m_AllSystems)
                m_AllSystems.Add(new SpaceSystemSave(ss));
        }
        public GalSave()
        { }

    }
}
