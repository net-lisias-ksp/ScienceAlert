using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon;
using UnityEngine;

namespace ScienceAlert.Windows.Implementations
{
    /// <summary>
    /// This window replaces the old ExperimentManager Drawable. It lists all available
    /// experiments
    /// </summary>
    class DraggableExperimentList : ReeperCommon.Window.DraggableWindow
    {
        protected override Rect Setup()
        {
            Title = "Available Experiments";

            return new Rect(0, 0, 256f, 128f);
        }


        /// <summary>
        /// Updates WindowRect size
        /// </summary>
        private void LateUpdate()
        {
        }

        protected override void DrawUI()
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            {
                GUILayout.Label("(no experiments available)");
            }
            GUILayout.EndVertical();
        }

        protected override void OnCloseClick()
        {
        
        }
    }
}
