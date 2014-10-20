using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        Implementations.DraggableExperimentList experimentList;
        Implementations.DraggableOptionsWindow optionsWindow;
        Implementations.DraggableDebugWindow debugWindow;

        // other components
        ScienceAlert scienceAlert;
        SSUICamera uiCamera;

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


            scienceAlert = GetComponent<ScienceAlert>();
            

            // note to self: these are on separate gameobjects because they use a UIButton to catch
            // clickthrough, parented to the window's GO. It's simpler to simply disable the window rather than
            // also disable the UIButton every time

            Log.Normal("Creating options window");
            optionsWindow = new GameObject("ScienceAlert.OptionsWindow").AddComponent<Implementations.DraggableOptionsWindow>();
            optionsWindow.scienceAlert = GetComponent<ScienceAlert>();
            optionsWindow.manager = GetComponent<Experiments.ExperimentManager>();
            

            Log.Normal("Creating experiment window");
            experimentList = new GameObject("ScienceAlert.ExperimentList").AddComponent<Implementations.DraggableExperimentList>();
            experimentList.biomeFilter = GetComponent<BiomeFilter>();
            experimentList.manager = GetComponent<Experiments.ExperimentManager>();
            

            Log.Normal("Creating debug window");
            debugWindow = new GameObject("ScienceAlert.DebugWindow").AddComponent<Implementations.DraggableDebugWindow>();
            

            // initially hide windows
            optionsWindow.Visible = experimentList.Visible = debugWindow.Visible = false;
        }



        /// <summary>
        /// Hook up events to control windows with
        /// </summary>
        private void Start()
        {
            uiCamera = ScreenSafeUI.referenceCam.GetComponent<SSUICamera>();
            if (uiCamera == null) Log.Error("WindowEventLogic: SSUICamera not found");

            
            scienceAlert.OnToolbarButtonChanged += OnToolbarChanged;
            scienceAlert.OnScanInterfaceChanged += OnInterfaceChanged;

            optionsWindow.OnVisibilityChange += this.OnWindowVisibilityChanged;
            experimentList.OnVisibilityChange += this.OnWindowVisibilityChanged;
            debugWindow.OnVisibilityChange += this.OnWindowVisibilityChanged;

            
            OnToolbarChanged();
            OnInterfaceChanged();
        }



        private void OnToolbarChanged()
        {
            scienceAlert.Button.OnClick += OnToolbarClick;
        }



        /// <summary>
        /// We're only interested in this one so we can give the experiment list the new interface
        /// </summary>
        private void OnInterfaceChanged()
        {
            experimentList.scanInterface = GetComponent<ScanInterface>();
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
            Log.Debug("OptionsWindow: {0}", optionsWindow.Visible);
            Log.Debug("ExperimentList: {0}", experimentList.Visible);
#endif

            if (optionsWindow.Visible || experimentList.Visible || debugWindow.Visible)
            {
                Log.Debug("WindowEventLogic: Hiding window(s)");
                optionsWindow.Visible = experimentList.Visible = debugWindow.Visible = false;
                AudioPlayer.Audio.PlayUI("click1", 0.5f);

                return;
            }
            else
            {
                switch (clickInfo.button)
                {
                    case 0: // left click, show experiment list
                        experimentList.Visible = true;
                        break;

                    case 1: // right click, show options window
                        optionsWindow.Visible = true;
                        break;

                    case 2: // middle click, show debug window (for AppLauncher this is alt + right click)
                        debugWindow.Visible = true;
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
            if (scienceAlert.ToolbarType == Settings.ToolbarInterface.ApplicationLauncher)
                if (tf)
                {
                    GetComponent<Toolbar.AppLauncherInterface>().button.SetTrue(false);
                }
                else
                {
                    if (!experimentList.Visible && !optionsWindow.Visible && !debugWindow.Visible)
                        GetComponent<Toolbar.AppLauncherInterface>().button.SetFalse(false);
                }
        }



        /// <summary>
        /// Prevent clickthrough to ScreenSafe GUI controls
        /// </summary>
        private void Update()
        {
            var mouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            ReeperCommon.Window.DraggableWindow[] windows = new DraggableWindow[] { optionsWindow, experimentList, debugWindow };

            uiCamera.enabled = !windows.Any(win => win.Visible && win.WindowRect.Contains(mouse));
        }
    }
}
