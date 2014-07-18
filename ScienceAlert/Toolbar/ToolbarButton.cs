using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Toolbar
{
    public delegate void ToolbarClick(ClickInfo clickInfo);

    /// <summary>
    /// Abstract actual toolbar in use away; user can choose between
    /// blizzy's toolbar or the stock ApplicationLauncher
    /// </summary>
    public class ToolbarButton: MonoBehaviour, IToolbar
    {
        public enum InterfaceType
        {
            ApplicationLauncher = 0,
            BlizzyToolbar
        }

        IToolbar button;
        InterfaceType current = InterfaceType.ApplicationLauncher;
        public static event ToolbarClick OnClick = delegate { };


//-----------------------------------------------------------------------------
// Begin implementation
//-----------------------------------------------------------------------------
        public InterfaceType Interface
        {
            set
            {
                if (value == current) return;

                switch (value)
                {
#region Blizzy's Toolbar
                    case InterfaceType.BlizzyToolbar:
                        if (!ToolbarManager.ToolbarAvailable)
                        {
                            Log.Error("ToolbarInterface: Blizzy toolbar unavailable. Using stock toolbar.");
                            Interface = InterfaceType.ApplicationLauncher;
                            return;
                        }
                        else
                        {
                            Log.Verbose("Setting up Blizzy toolbar interface...");

                            // todo: delete other interface
                            button = gameObject.AddComponent<BlizzyInterface>();
                            current = value;
                        }

                        break;
#endregion
                    case InterfaceType.ApplicationLauncher:
                        Log.Verbose("Setting up ApplicationLauncher interface...");
                        break; // todo
                }
            }

            get
            {
                return current;
            }
        }

        public static void TriggerOnClick(ClickInfo ci)
        {
            OnClick(ci);
        }

        void Start()
        {
            Init();
        }

        void OnDestroy()
        {
            DeInit();
        }

        void Init() 
        {
            // todo: load default from settings
            Interface = InterfaceType.BlizzyToolbar;

        }

        void DeInit() 
        {

        }


        public void SetUnlit()
        {
            button.SetUnlit();   
        }

        public void StopAnimation()
        {
            button.StopAnimation();
        }

        public void PlayAnimation()
        {
            button.PlayAnimation();
        }

        public IDrawable Drawable
        {
            get
            {
                return button.Drawable;
            }

            set
            {
                button.Drawable = value;
            }
        }

        public bool Important
        {
            get
            {
                return button.Important;
            }

            set
            {
                button.Important = value;
            }
        }
    }
}
