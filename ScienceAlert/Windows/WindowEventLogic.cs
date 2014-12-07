using System.Linq;
using ReeperCommon;
using ReeperCommon.Window;

using UnityEngine;

namespace ScienceAlert.Windows
{
    /// <summary>
    /// This class controls which window(s) are shown when
    /// </summary>
    class WindowEventLogic : MonoBehaviour
    {
        // Windows
        Implementations.DraggableExperimentList _experimentList;
        Implementations.DraggableOptionsWindow _optionsWindow;
        Implementations.DraggableDebugWindow _debugWindow;

        // other components
        ScienceAlertCore _scienceAlert;
        SSUICamera _uiCamera;

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        /// <summary>
        /// Create the window components we'll be using
        /// </summary>
        private void Awake()
        {
            Log.Verbose("Customizing DraggableWindow");

            DraggableWindow.CloseTexture = ResourceUtil.LocateTexture("ScienceAlert.Resources.btnClose.png");
            DraggableWindow.LockTexture = ResourceUtil.LocateTexture("ScienceAlert.Resources.btnLock.png");
            DraggableWindow.UnlockTexture = ResourceUtil.LocateTexture("ScienceAlert.Resources.btnUnlock.png");
            DraggableWindow.ButtonHoverBackground = ResourceUtil.LocateTexture("ScienceAlert.Resources.btnBackground.png");
            DraggableWindow.ButtonSound = "click1";


            _scienceAlert = GetComponent<ScienceAlertCore>();
            

            // note to self: these are on separate gameobjects because they use a UIButton to catch
            // clickthrough, parented to the window's GO. It's simpler to simply disable the window rather than
            // also disable the UIButton every time

            Log.Normal("Creating options window");
            _optionsWindow = new GameObject("ScienceAlert.OptionsWindow").AddComponent<Implementations.DraggableOptionsWindow>();
            

            Log.Normal("Creating experiment window");
            _experimentList = new GameObject("ScienceAlert.ExperimentList").AddComponent<Implementations.DraggableExperimentList>();
            

            Log.Normal("Creating debug window");
            _debugWindow = new GameObject("ScienceAlert.DebugWindow").AddComponent<Implementations.DraggableDebugWindow>();
            

            // initially hide windows
            _optionsWindow.Visible = _experimentList.Visible = _debugWindow.Visible = false;
        }



        /// <summary>
        /// Hook up events to control windows with
        /// </summary>
        private void Start()
        {
            _uiCamera = ScreenSafeUI.referenceCam.GetComponent<SSUICamera>();
            if (_uiCamera == null) Log.Error("WindowEventLogic: SSUICamera not found");

            
            _scienceAlert.OnToolbarButtonChanged += OnToolbarChanged;
            _scienceAlert.OnScanInterfaceChanged += OnInterfaceChanged;

            _optionsWindow.OnVisibilityChange += this.OnWindowVisibilityChanged;
            _experimentList.OnVisibilityChange += this.OnWindowVisibilityChanged;
            _debugWindow.OnVisibilityChange += this.OnWindowVisibilityChanged;

            
            OnToolbarChanged();
            OnInterfaceChanged();
        }



        private void OnToolbarChanged()
        {
            _scienceAlert.Button.OnClick += OnToolbarClick;
        }



        /// <summary>
        /// We're only interested in this one so we can give the experiment list the new interface
        /// </summary>
        private void OnInterfaceChanged()
        {
            //experimentList.scanInterface = GetComponent<ScanInterface>();
        }


        private void OnDestroy()
        {
            // no cleanup; we're attached to ScienceAlert so it'll clean us up
        }



        /// <summary>
        /// Handle a toolbar button click (for selected toolbar; blizzy's or ApplicationLauncher).
        /// Figure out which window to show (or hide) and do so
        /// </summary>
        /// <param name="clickInfo"></param>
        private void OnToolbarClick(Toolbar.ClickInfo clickInfo)
        {
            Log.Debug("WindowEventLogic.OnToolbarClick: button " + clickInfo.button);

            // Logic:
            //   If any window is visible, hide it
            //
            //   If left-click, show experiment list
            //   If right-click, show options window

#if DEBUG
            Log.Debug("OptionsWindow: {0}", _optionsWindow.Visible);
            Log.Debug("ExperimentList: {0}", _experimentList.Visible);
#endif

            if (_optionsWindow.Visible || _experimentList.Visible || _debugWindow.Visible)
            {
                Log.Debug("WindowEventLogic: Hiding window(s)");
                _optionsWindow.Visible = _experimentList.Visible = _debugWindow.Visible = false;
                AudioPlayer.Audio.PlayUI("click1", 0.5f);

                return;
            }
            else
            {
                switch (clickInfo.button)
                {
                    case 0: // left click, show experiment list
                        _experimentList.Visible = true;
                        break;

                    case 1: // right click, show options window
                        _optionsWindow.Visible = true;
                        break;

                    case 2: // middle click, show debug window (for AppLauncher this is alt + right click)
                        _debugWindow.Visible = true;
                        break;
                }

                AudioPlayer.Audio.PlayUI("click1", 0.5f);
            }
        }



        /// <summary>
        /// Make sure the AppLauncher button state matches window state
        /// </summary>
        /// <param name="tf"></param>
        private void OnWindowVisibilityChanged(bool tf)
        {
            if (_scienceAlert.ToolbarType == Settings.ToolbarInterface.ApplicationLauncher)
                if (tf)
                {
                    GetComponent<Toolbar.AppLauncherInterface>().button.SetTrue(false);
                }
                else
                {
                    if (!_experimentList.Visible && !_optionsWindow.Visible && !_debugWindow.Visible)
                        GetComponent<Toolbar.AppLauncherInterface>().button.SetFalse(false);
                }
        }



        /// <summary>
        /// Prevent clickthrough to ScreenSafe GUI controls
        /// </summary>
        private void Update()
        {
            var mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            ReeperCommon.Window.DraggableWindow[] windows = new DraggableWindow[] { _optionsWindow, _experimentList, _debugWindow };

            _uiCamera.enabled = !windows.Any(win => win.Visible && win.WindowRect.Contains(mouse));
        }
    }
}
