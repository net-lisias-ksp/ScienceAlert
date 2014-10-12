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

        // other components
        ScienceAlert scienceAlert;

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

            Log.Normal("Creating options window");
            optionsWindow = gameObject.AddComponent<Implementations.DraggableOptionsWindow>();

            Log.Normal("Creating experiment window");
            experimentList = gameObject.AddComponent<Implementations.DraggableExperimentList>();

            // initially hide windows
            optionsWindow.Visible = experimentList.Visible = false;
        }



        /// <summary>
        /// Hook up events to control windows with
        /// </summary>
        private void Start()
        {
            scienceAlert = GetComponent<ScienceAlert>();

            scienceAlert.Button.OnClick += OnToolbarClick;
        }



        private void OnDestroy()
        {
            // no cleanup; we're attached to ScienceAlert so it dies when we do
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

            if (optionsWindow.Visible || experimentList.Visible)
            {
                Log.Debug("WindowEventLogic: Hiding window(s)");
                optionsWindow.Visible = experimentList.Visible = false;
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

                    case 2: // middle click
                        throw new NotImplementedException("Middle-click not handled by WindowEventLogic");
                        
                }
            }
        }
    }
}
