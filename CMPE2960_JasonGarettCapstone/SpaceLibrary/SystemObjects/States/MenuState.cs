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
using IrrlichtLime.GUI;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace SpaceLibrary
{
    public abstract class MenuState : State
    {
        protected List<GUIElement> m_ListOfGUIElements = null;

        public MenuState(IrrlichtDevice dev)
            : base(dev)
        {
            m_ListOfGUIElements = new List<GUIElement>();
        }

        public override void Update()
        { }

        public override void GDIDraw(Graphics gr)
        { }

        public override void PauseState()
        {
            m_ListOfGUIElements.ForEach((G) => G.Enabled = false);
            m_ListOfGUIElements.ForEach((G) => G.Visible = false);

            base.PauseState();
        }

        public override void ResumeState()
        {
            m_ListOfGUIElements.ForEach((G) => G.Enabled = true);
            m_ListOfGUIElements.ForEach((G) => G.Visible = true);

            base.ResumeState();
        }

        public override void EndState()
        {
            m_ListOfGUIElements.ForEach((G) => G.Remove());
        }
    }
}
