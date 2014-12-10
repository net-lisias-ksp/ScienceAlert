using ReeperCommon;
using ScienceAlert.Experiments.Science;
using UnityEngine;
using System.Collections.Generic;
using ScienceAlert;
#if DEBUG
using System.Linq;
#endif


namespace ScienceAlert.Windows.Implementations
{
    class DraggableDebugWindow : ReeperCommon.Window.DraggableWindow
    {
        public BiomeFilter BiomeFilter; // note: find a way to clean this up
        public IStoredVesselScience StoredVesselScience;


        /// <summary>
        /// DraggableWindow setup here
        /// </summary>
        /// <returns></returns>
        protected override Rect Setup()
        {
            // position blocker in front of ApplicationLauncher buttons. The window is going to be drawn on
            // top of them regardless; this will just prevent us from accidentally interacting with them
            backstop.SetZ(ApplicationLauncher.Instance.anchor.transform.position.z - 50f);

            Title = "Debug";
            Skin = Settings.Skin;
            Settings.Instance.OnSave += AboutToSave;

            LoadFrom(Settings.Instance.additional.GetNode("DebugWindow") ?? new ConfigNode());

            Log.Debug("DraggableDebugWindow.Setup");

            return new Rect(windowRect.x, windowRect.y, 256f, 128f);
        }


        private void AboutToSave()
        {
            Log.Debug("DraggableDebugWindow.AboutToSave");
            SaveInto(Settings.Instance.additional.GetNode("DebugWindow") ?? Settings.Instance.additional.AddNode("DebugWindow"));
        }



        protected override void DrawUI()
        {
            GUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.MinHeight(128f));

            // current biome
            GUILayout.Label(string.Format("Biome: {0}", BiomeFilter.CurrentBiome), GUILayout.MinWidth(256f));

#region debug-only
#if DEBUG
            if (GUILayout.Button("SurfaceSample Transmit Value"))
                CheckSurfaceSampleTransmit();

            if (GUILayout.Button("Dump xmitScalar values"))
                DumpXmitScalars();
#endif
#endregion

            GUILayout.EndVertical();
        }



        protected override void OnCloseClick()
        {
            Visible = false;
        }





#if DEBUG
        // Check our ability to calculate correct transmit value for an experiment without a ModuleScienceExperiment
        // to use
        private void CheckSurfaceSampleTransmit()
        {
            var exp = ResearchAndDevelopment.GetExperiment("surfaceSample");

            ScienceSubject subject;

            if (exp.BiomeIsRelevantWhile(ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel)))
            {
                subject = ResearchAndDevelopment.GetExperimentSubject(exp, ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel), FlightGlobals.currentMainBody, BiomeFilter.CurrentBiome);

                // we need to look at the eva kerbal part
                PartLoader.LoadedPartsList.ForEach(ap =>
                {
                    if (ap.name.StartsWith("k") || ap.name.StartsWith("K"))
                        Log.Normal("Loaded part: {0}", ap.name);
                });

                //PartLoader.getPartInfoByName("kerbalEVA").partPrefab.gameObject.PrintComponents();
                List<ModuleScienceExperiment> modules = PartLoader.getPartInfoByName("kerbalEVA").partPrefab.gameObject.GetComponentsInChildren<ModuleScienceExperiment>(true).ToList();

                modules.ForEach(mse => Log.Normal("MSE: " + mse.experimentID));

                float xmitScalar = modules.Single(mse => mse.experimentID == "surfaceSample").xmitDataScalar;

                // note: hoping to find a correlation between module xmit scalar and something in subject

                Log.Debug("Checking current surface sample transmit value for {0}", subject.id);
                Log.Debug("current: " + subject.science);
                Log.Debug("cap: " + subject.scienceCap);
                Log.Debug("dataScale: " + subject.dataScale);

                Log.Debug("scientific value: " + subject.scientificValue);
                Log.Debug("subject value: " + subject.subjectValue);
                Log.Debug("body multiplier: " + API.GetBodyScienceValueMultipler(ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel)));
                Log.Debug("xmitScalar: " + xmitScalar);
                Log.Debug("sample count: " + StoredVesselScience.ScienceData.Count(sd => sd.subjectID == subject.id));

                FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>().Where(mse => mse.experimentID == exp.id).ToList().ForEach(mse => Log.Debug("xmitScalar: {0}", mse.xmitDataScalar));

                // this should be correct (note: doesn't consider current science onboard)
                Log.Normal("Next absolute recovery: {0}", subject.CalculateNextReport(exp, new List<ScienceData>()));

                // this should also be correct (considers stored experiments)
                Log.Normal("Next recovered sample worth: {0}", subject.CalculateNextReport(exp, StoredVesselScience.ScienceData.Where(sd => sd.subjectID == subject.id));

                // value to be checked, absolute
                Log.Normal("next absolute transmission: {0}", subject.CalculateNextReport(exp, new List<ScienceData>(), xmitScalar));

                // value to be checked, next transmitted sample
                Log.Normal("next transmitted sample worth: {0}", subject.CalculateNextReport(exp, StoredVesselScience.FindStoredData(subject.id), xmitScalar));
            }
        }


        /// <summary>
        /// Dump all onboard ModuleScienceExperiment xmitScalars
        /// </summary>
        private void DumpXmitScalars()
        {
            Log.Debug("Dumping xmit scalars");
            FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceExperiment>().ToList().ForEach(mse => Log.Normal("{0} xmit scalar: {1}", mse.experimentID, mse.xmitDataScalar));
        }
#endif
    }
}
