using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class Save
    {
        public GalSave m_GalaxySave;

        public PlayerSave m_PlayerSave;

        public Save(Galaxy gal, Player gamePlayer)
        {
            m_GalaxySave = new GalSave(gal);
            m_PlayerSave = new PlayerSave(gamePlayer);
        }
        public Save()
        { }
    }
}
