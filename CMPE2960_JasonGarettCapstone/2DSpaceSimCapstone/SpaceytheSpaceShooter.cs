using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

using IrrlichtLime;
using IrrlichtLime.GUI;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

using SpaceLibrary;
using System.Diagnostics;

namespace _2DSpaceSimCapstone
{
    public partial class SpaceytheSpaceShooter : Form
    {
        //The graphics device our meshes will be displayed on
        private IrrlichtDevice _dev = null;

        //The graphics context the regions will be based around
        private Graphics _gr = null;

        static public bool _debug = false;

        //Draw GDI+ Regions
        private bool _GDIView = false;

        private int _frameCount = 0;

        //Manages the various states of the games
        private StateManager _sm = null;

        private GUIFont _debugFont = null;

        private Dimension2Di _screenDimensions = new Dimension2Di(1024, 768);

        public SpaceytheSpaceShooter()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Occurs when the form is loaded
        /// Initializes the irrlicht device and all other base game components
        /// Starts the update timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpaceytheSpaceShooter_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            _dev = IrrlichtDevice.CreateDevice(IrrlichtLime.Video.DriverType.Direct3D9,_screenDimensions);
            _dev.OnEvent += new IrrlichtDevice.EventHandler(Irrlicht_Dev_OnEvent);

            if (_dev != null)
                _dev.Run();
            else
                throw new Exception("Unable to create device.");

            Loader.m_dev = _dev;
            _gr = CreateGraphics();
            Loader.m_gr = _gr;
            Loader.LoadStuff();

            _dev.SetWindowCaption("2D Space Sim");

            _dev.FileSystem.WorkingDirectory = @"..\..\..\SpaceLibrary\Media";

            SceneNode skyDome = _dev.SceneManager.AddSkyDomeSceneNode(_dev.VideoDriver.GetTexture("SpaceDome.png"));
            skyDome.Rotation = new Vector3Df(0, 0, 90);
            _dev.SceneManager.AmbientLight = new Colorf(1f, 1f, 1f);

            _debugFont = _dev.GUIEnvironment.BuiltInFont;

            _sm = new StateManager(_gr, _dev, _screenDimensions);
            _sm.OnCloseApplication += new State.delVoidVoid(EndProgram);

            _gr.TranslateTransform(_gr.VisibleClipBounds.Width / 2, _gr.VisibleClipBounds.Height / 2);
            _gr.ScaleTransform(3f, -3f);
            Player.debugXOffset = 0;
            Player.debugYOffset = 0;

            _dev.SceneManager.AddLightSceneNode(null, new Vector3Df(0, 0, 0), new Colorf(1,1,1), 5000);

            Thread GameThread = new Thread(new ThreadStart(StartRunGame));
            GameThread.IsBackground = true;
            GameThread.Name = "Game Thread";
            GameThread.Start();
        }

        /// <summary>
        /// Starts the running of the game
        /// </summary>
        private void StartRunGame()
        {
            Invoke(new delVoidVoid(RunGame));
        }

        /// <summary>
        /// Runs the Game
        /// </summary>
        private delegate void delVoidVoid();
        public void RunGame()
        {
            Stopwatch stoppy = new Stopwatch();
            while (_dev.Run())
            {
                stoppy.Start();
                UpdateGame();
                DrawGame();
                stoppy.Stop();
                if (stoppy.ElapsedMilliseconds <20)
                    Thread.Sleep(20 - (int)stoppy.ElapsedMilliseconds);
                stoppy.Reset();
            }
            EndProgram();
        }

        /// <summary>
        /// Processes events fired by the irrlicht device
        /// </summary>
        /// <param name="evnt">The event fired</param>
        /// <returns>Whether or not the event is done being processed</returns>
        public bool Irrlicht_Dev_OnEvent(Event evnt)
        {
            if (evnt.Type == EventType.Log)
            {
                if (_debug)
                    System.Diagnostics.Trace.WriteLine(evnt.Log.Text);
                return true;
            }
            else if (evnt.Type == EventType.Mouse)
                return false;
            //This cause broken things
            //return (bool)Invoke(new delBoolEvent(InvokedHandleEvent), evnt);
            return InvokedHandleEvent(evnt);
        }

        private delegate bool delBoolEvent(Event evnt);
        private bool InvokedHandleEvent(Event evnt)
        {
            if (evnt.Type == EventType.Key)
            {
                if (evnt.Key.PressedDown)
                {
                    if (evnt.Key.Key == KeyCode.Backquote)
                        if (_GDIView)
                            _GDIView = false;
                        else
                            _GDIView = true;
                    else
                    if (evnt.Key.Key == KeyCode.Backquote)
                        if (_debug)
                            _debug = false;
                        else
                            _debug = true;
                }
            }

            if (_sm != null)
                return _sm.HandleEvent(evnt);
            else
                return false;
        }

        /// <summary>
        /// Updates all active states
        /// </summary>
        private void UpdateGame()
        {
            _sm.Update();
            _frameCount++;
        }

        /// <summary>
        /// Draws the meshes of all the game objects
        /// Or their regions if _GDIView
        /// </summary>
        private void DrawGame()
        {
            _gr.TranslateTransform(-Player.debugXOffset, -Player.debugYOffset);

            if (!_GDIView && _dev != null)
            {                
                _dev.VideoDriver.BeginScene(true, true, IrrlichtLime.Video.Color.OpaqueBlack);

                _dev.SceneManager.DrawAll();

                _dev.GUIEnvironment.DrawAll();

                if (_debug)
                {
                    Player _player = _sm.GetPlayer();
                    _debugFont.Draw(_player.m_PlayerShip.m_GameObjectMesh.Position.ToString(), new Vector2Di(5, 5), new IrrlichtLime.Video.Color(255, 255, 255, 255));
                }

                _sm.Draw();

                _dev.VideoDriver.EndScene();
            }


            GDIDraw();
        }

        /// <summary>
        /// Draws the regions of every active state
        /// </summary>
        private void GDIDraw()
        {
            _gr.Clear(System.Drawing.Color.Black);

            _sm.GDIDraw(_gr);
        }

        /// <summary>
        /// Closes the Irrlicht device and ends the application
        /// </summary>
        private void EndProgram()
        {
            if (_dev != null)
            {
                _dev.Drop();
                _dev = null;
                _sm.EndGame();
                _gr = null;
                _debugFont = null;
                _screenDimensions = null;
            }
            this.Close();
        }
    }
}
