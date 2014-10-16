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
        private const string WindowTitle = "Available Experiments";

        // members
        public ExperimentManager manager;
        public BiomeFilter biomeFilter;

        
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
            Title = "Available Experiments";
            ShrinkHeightToFit = true;

            Skin = Instantiate(Settings.Skin) as GUISkin; // we'll be altering it a little bit to make sure the buttons are the right size

            return new Rect(0, 0, 256f, 128f);
        }



        /// <summary>
        /// Updates WindowRect size
        /// </summary>
        private void LateUpdate()
        {
            if (Settings.Instance.DisplayCurrentBiome)
            {
                string biome = Title;

                if (biomeFilter.GetCurrentBiome(out biome))
                    Title = biome;
            }
            else Title = WindowTitle;

        }



        /// <summary>
        /// Main purpose is to tweak our button skin so they're guaranteed not to clip any experiment
        /// title text
        /// </summary>
        new protected void OnGUI() 
        {
            if (!adjustedSkin)
            {
                //Skin.window.stretchHeight = true;
                //List<string> experimentTitles = new List<string>();

                //ResearchAndDevelopment.GetExperimentIDs().ForEach(id => experimentTitles.Add(ResearchAndDevelopment.GetExperiment(id).experimentTitle));

                //Skin.button.fixedWidth = Mathf.Max(64f, experimentTitles.Max(title =>
                //    {
                //        float minWidth = 0f;
                //        float maxWidth = 0f;

                //        Skin.button.CalcMinMaxWidth(new GUIContent(title + " (123.4)"), out minWidth, out maxWidth);

                //        return maxWidth;
                //    }));

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
                var observers = manager.Observers;

                if (observers.All(eo => !eo.Available))
                {
                    GUILayout.Label("(no experiments available)");
                }
                else
                {
                    //-----------------------------------------------------
                    // Experiment list
                    //-----------------------------------------------------
                    foreach (var observer in observers)
                        if (observer.Available)
                        {
                            var content = new GUIContent(observer.ExperimentTitle);

                            if (Settings.Instance.ShowReportValue) content.text += string.Format(" ({0:0.#})", observer.NextReportValue);

                            if (GUILayout.Button(content, Settings.Skin.button, GUILayout.ExpandHeight(false)))
                            {
                                Log.Debug("Deploying {0}", observer.ExperimentTitle);
                                AudioPlayer.Audio.Play("click2");
                                observer.Deploy();
                            }
                        }
                }


                
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
