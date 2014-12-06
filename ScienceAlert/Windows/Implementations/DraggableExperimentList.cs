using System.Collections.Generic;
using System.Linq;
using ReeperCommon;
using UnityEngine;

namespace ScienceAlert.Windows.Implementations
{
    using Observer = Experiments.Observers.ExperimentObserver;

    /// <summary>
    /// This window replaces the old ExperimentManager Drawable. It lists all available
    /// experiments
    /// </summary>
    class DraggableExperimentList : ReeperCommon.Window.DraggableWindow
    {
        private const string WindowTitle = "Available Experiments";

        // members
        private bool adjustedSkin = false;

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        /// <summary>
        /// DraggableWindow setup here
        /// </summary>
        /// <returns></returns>
        protected override Rect Setup()
        {
            // position blocker in front of ApplicationLauncher buttons. The window is going to be drawn on
            // top of them regardless; this will just prevent us from accidentally interacting with them
            backstop.SetZ(ApplicationLauncher.Instance.anchor.transform.position.z - 50f);

            Title = "Available Experiments";
            ShrinkHeightToFit = true;

            Skin = Instantiate(Settings.Skin) as GUISkin; // we'll be altering it a little bit to make sure the buttons are the right size
            Settings.Instance.OnSave += AboutToSave;

 
            LoadFrom(Settings.Instance.additional.GetNode("ExperimentWindow") ?? new ConfigNode());
            return new Rect(windowRect.x, windowRect.y, 256f, 128f);
        }



        /// <summary>
        /// Instead of updating window position every frame, we'll use an event to let us know
        /// when we should update Settings' window position
        /// </summary>
        private void AboutToSave()
        {
            SaveInto(Settings.Instance.additional.GetNode("ExperimentWindow") ?? Settings.Instance.additional.AddNode("ExperimentWindow"));
        }





        /// <summary>
        /// Updates WindowRect size
        /// </summary>
        private void LateUpdate()
        {
            if (FlightGlobals.ActiveVessel != null)
                if (Settings.Instance.DisplayCurrentBiome)
                {
                    // if SCANsat is enabled, don't show biome names for unscanned areas
                    //if (API.scien.Instance.ScanInterfaceType == Settings.ScanInterface.ScanSat && scanInterface != null)
                    //{
                    //    if (!scanInterface.HaveScanData(FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.mainBody))
                    //    {
                    //        Title = "Data not found";
                    //        return;
                    //    }
                    //}
                    if (API.ScienceAlert.ScanInterface.HaveScanData(FlightGlobals.ActiveVessel.latitude,
                        FlightGlobals.ActiveVessel.longitude, FlightGlobals.ActiveVessel.mainBody))
                    {
                        Title = "Not scanned";
                        return;
                    }

                    Title = API.ScienceAlert.BiomeFilter.CurrentBiome;
                    return;
                }  
                    
            Title = WindowTitle; // default experiment window title
        }



        /// <summary>
        /// Main purpose is to tweak our button skin so they're guaranteed not to clip any experiment
        /// title text
        /// </summary>
        new protected void OnGUI()
        {
            if (!adjustedSkin)
            {
                Skin.window.stretchHeight = true;
                var experimentTitles = new List<string>();

                ResearchAndDevelopment.GetExperimentIDs().ForEach(id => experimentTitles.Add(ResearchAndDevelopment.GetExperiment(id).experimentTitle));

                Skin.button.fixedWidth = Mathf.Max(64f, experimentTitles.Max(title =>
                    {
                        float minWidth = 0f;
                        float maxWidth = 0f;

                        Skin.button.CalcMinMaxWidth(new GUIContent(title + " (123.4)"), out minWidth, out maxWidth);

                        return maxWidth;
                    }));

                adjustedSkin = true;
            }

            base.OnGUI();
        }



        /// <summary>
        /// It's pretty much what you'd expect
        /// </summary>
        protected override void DrawUI()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Label("(no experiments available)");

                //var observers = manager.Observers;
                //var observers = new List<Experiments.Monitors.ExperimentSensor>();

                //if (observers.All(eo => !eo.Available))
                //{
                //    GUILayout.Label("(no experiments available)");
                //}
                //else
                //{
                    //-----------------------------------------------------
                    // Experiment list
                    //-----------------------------------------------------

                    // this is old version, check-manager-directly
                    //foreach (var observer in observers)
                    //    if (observer.Available)
                    //    {
                    //        var content = new GUIContent(observer.ExperimentTitle);

                    //        if (Settings.Instance.ShowReportValue) content.text += string.Format(" ({0:0.#})", observer.RecoveryValue);

                    //        if (GUILayout.Button(content, Settings.Skin.button, GUILayout.ExpandHeight(false)))
                    //        {
                    //            Log.Debug("Deploying {0}", observer.ExperimentTitle);
                    //            AudioPlayer.Audio.PlayUI("click2");
                    //            observer.Deploy();
                    //        }
                    //    }
                //}


                
            }
            GUILayout.EndVertical();
        }



        /// <summary>
        /// The DraggableWindow clsoe button was clicked
        /// </summary>
        protected override void OnCloseClick()
        {
            Visible = false;
        }


        #region events



        #endregion
    }
}
