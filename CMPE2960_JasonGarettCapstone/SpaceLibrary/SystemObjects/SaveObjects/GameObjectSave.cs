using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class GameObjectSave
    {
        public float m_xPosition;
        public float m_zPosition;
        public float m_yPosition;

        public float m_Heading;

        public GameObjectSave(GameObject go)
        {
            m_xPosition = go.m_xPosition;
            m_zPosition = go.m_zPosition;
            m_yPosition = go.m_yPosition;

            m_Heading = go.m_Heading;
        }
        public GameObjectSave()
        { }
    }
}
