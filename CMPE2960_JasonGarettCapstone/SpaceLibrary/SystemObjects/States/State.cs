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
    [Serializable]
    public abstract class State
    {
        public delegate void delVoidVoid();
        public delegate void delVoidState(State newState);

        public static GUIFont m_DrawFront;

        //Called when the state is finished
        public event delVoidVoid OnStateFinished;

        //Called when the state makes a new state
        public event delVoidState OnNewStateCreated;

        //Called when the state replaces the state stack with a new state
        public event delVoidState OnNewStateStack;

        //The irrlicht device
        protected IrrlichtDevice m_dev;

        //The gui envirnmont of the irrlicht device
        protected GUIEnvironment m_env;

        //The list of all gui elements used by the state
        protected List<GUIElement> m_ListOfGUIElements = null;
        //The list of GUIElements currently seen
        protected List<GUIElement> m_ListOfShownGUIElements = null;

        //Whether or not the state should be updated
        public bool isActive { get; protected set; }

        public State(IrrlichtDevice dev)
        {
            m_dev = dev;
            m_env = dev.GUIEnvironment;
            m_ListOfGUIElements = new List<GUIElement>();
            m_ListOfShownGUIElements = new List<GUIElement>();
            isActive = true;
        }

        abstract public bool HandleEvent(Event evnt);

        /// <summary>
        /// Stop the state from being updated
        /// </summary>
        public virtual void PauseState()
        {
            isActive = false;

            m_ListOfShownGUIElements.ForEach((G) => G.Enabled = false);
            m_ListOfShownGUIElements.ForEach((G) => G.Visible = false);
        }

        /// <summary>
        /// Called by the state when the state is finished
        /// </summary>
        protected void StateFinished()
        {
            if (OnStateFinished != null)
                OnStateFinished();
        }

        /// <summary>
        /// Called when the state creates a new state to be added to the state manager
        /// </summary>
        /// <param name="newState">The state to be added</param>
        protected void NewStateCreated(State newState)
        {
            if (OnNewStateCreated != null)
                OnNewStateCreated(newState);
        }

        /// <summary>
        /// Called when the state creates a new state to be added to the state manager as a replacement 
        /// </summary>
        /// <param name="newState">The state to be added as the base state</param>
        protected void NewStateStack(State newState)
        {
            if (OnNewStateStack != null)
                OnNewStateStack(newState);
        }

        /// <summary>
        /// Resume updates to the state
        /// </summary>
        public virtual void ResumeState()
        {
            isActive = true;

            m_ListOfShownGUIElements.ForEach((G) => G.Enabled = true);
            m_ListOfShownGUIElements.ForEach((G) => G.Visible = true);
        }
        
        /// <summary>
        /// Called by the state manager when the state is first added
        /// </summary>
        public virtual void BeginState()
        {
            m_ListOfShownGUIElements.ForEach((G) => G.Enabled = true);
            m_ListOfShownGUIElements.ForEach((G) => G.Visible = true);
        }

        /// <summary>
        /// Called by the state manager when the state is about to be removed
        /// </summary>
        public virtual void EndState()
        {
            m_ListOfGUIElements.ForEach((G) => G.Remove());
        } 
    }
}
