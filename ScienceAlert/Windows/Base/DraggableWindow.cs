using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Windows.Base
{
    abstract class DraggableWindow : MonoBehaviour
    {
        protected UIButton backstop;                        // prevent players from accidentally clicking things in the background
                                                            // this is a bit more reliable than InputLockManager, especially since
                                   
        protected Rect windowRect = new Rect();
        protected int winId = UnityEngine.Random.Range(2444, int.MaxValue);

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/


        /// <summary>
        /// Perform any needful setup here, such as locating textures for the lock button, setting up
        /// the drag listener, etc
        /// </summary>
        protected void Awake()
        {

            Log.Debug("DraggableWindow.Awake");
            backstop = GuiUtil.CreateBlocker(windowRect, GuiUtil.GetGuiCamera().nearClipPlane + 1f, "DraggableWindow.Backstop");

            windowRect = Setup();

            //backstop.Setup(windowRect.width, windowRect.height);
            backstop.Move(windowRect);
            
            backstop.transform.parent = transform;

            // check for programmer error
            if (windowRect.width < 1f || windowRect.height < 1f)
                Log.Warning("DraggableWindow.Base: Derived class did not set up initial window Rect");

            GameEvents.onHideUI.Add(OnHideUI);
            GameEvents.onShowUI.Add(OnShowUI);
        }

        

        /// <summary>
        /// Called when this Component is destroyed
        /// </summary>
        protected void OnDestroy()
        {
            Log.Debug("DraggableWindow.OnDestroy");

            GameEvents.onHideUI.Remove(OnHideUI);
            GameEvents.onShowUI.Remove(OnShowUI);
        }



        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            Log.Debug("DraggableWindow.OnEnable");
        }



        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            Log.Debug("DraggableWindow.OnDisable");
        }



        private void OnGUI()
        {
            windowRect = GUILayout.Window(winId, windowRect, _InternalDraw, "Draggable Window");
            backstop.Move(windowRect);

            //GUILayout.BeginArea(windowRect);
            //GUILayout.Box("Hello, world!");
            //GUILayout.EndArea();
        }



        private void _InternalDraw(int winid)
        {
            DrawUI();
            GUI.DragWindow();
        }


        /// <summary>
        /// Implementations of a draggable window should set up an initial Rect here and do
        /// any other needful setup
        /// </summary>
        /// <returns></returns>
        protected abstract Rect Setup();


        protected abstract void DrawUI();


        #region GameEvents

        private void OnHideUI()
        {
            gameObject.SetActive(false);
        }

        private void OnShowUI()
        {
            gameObject.SetActive(true);
        }

        #endregion
    }
}
