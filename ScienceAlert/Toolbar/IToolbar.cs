//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using UnityEngine;

namespace ScienceAlert.Toolbar
{
    /// <summary>
    /// Common interface shared by blizzy's toolbar interface and the
    /// application launcher interface
    /// </summary>
    interface IToolbar
    {
        void PlayAnimation();
        void StopAnimation();
        void SetUnlit();

        IDrawable Drawable
        {
            get;
            set;
        }

        bool Important
        {
            get;
            set;
        }
    }

    public struct ClickInfo
    {
        public int button;
    }
}
