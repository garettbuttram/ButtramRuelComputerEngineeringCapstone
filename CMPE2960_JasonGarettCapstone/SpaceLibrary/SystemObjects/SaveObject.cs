using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    [Serializable]
    public class SaveObject
    {
        //The player to save
        public Player m_SavePlayer;

        //The galaxy to be saved
        //Which is the game world
        public Galaxy m_SaveGalaxy;
    }
}
