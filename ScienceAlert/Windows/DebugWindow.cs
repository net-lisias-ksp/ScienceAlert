/******************************************************************************
                   Science Alert for Kerbal Space Program                    
 ******************************************************************************
    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert
{
    /// <summary>
    /// This is an extra window accessible via the toolbar + middle mouse
    /// that should help in tracking down issues.
    /// </summary>
    public class DebugWindow : MonoBehaviour, IDrawable
    {
        private readonly int windowId = UnityEngine.Random.Range(0, int.MaxValue);

        // --------------------------------------------------------------------
        //    Members of DebugWindow
        // --------------------------------------------------------------------
        private Rect windowRect = new Rect(0, 0, 324, Screen.height / 5);
        private ScienceAlert scienceAlert;
        private BiomeFilter biomeFilter;
        private StorageCache storage;



/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/
        void Awake()
        {
            scienceAlert = gameObject.GetComponent<ScienceAlert>();
            biomeFilter = gameObject.GetComponent<BiomeFilter>();
            storage = gameObject.GetComponent<StorageCache>();
        }


        public void OnToolbarClicked(Toolbar.ClickInfo ci)
        {
            if (scienceAlert.Button.Drawable is DebugWindow)
            {
                AudioUtil.Play("click2", 0.05f);
                scienceAlert.Button.Drawable = null;
            } else if (scienceAlert.Button.Drawable == null && ci.button == 2)
            {
                AudioUtil.Play("click2", 0.05f);
                scienceAlert.Button.Drawable = this;
            }
        }


        public Vector2 Draw(Vector2 position)
        {
            var oldSkin = GUI.skin;
            GUI.skin = Settings.Skin;

            windowRect.x = position.x;
            windowRect.y = position.y;

            GUILayout.Window(windowId, windowRect, RenderOptions, "Science Alert Debug Options");

            GUI.skin = oldSkin;

            return new Vector2(windowRect.width, windowRect.height);
        }


        /// <summary>
        /// List all onboard science data
        /// </summary>
        private void DumpScienceData()
        {
            var containers = FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataContainer>();

            Log.Write("Dumping all science found in {0} containers aboard {1}", containers.Count, FlightGlobals.ActiveVessel.vesselName);
            Log.Write("StorageCache is keeping track of {0} containers", storage.StorageContainerCount);

            foreach (var can in containers)
            {
                var storedData = can.GetData();

                foreach (var data in storedData)
                    Log.Write("{0} - {1}", data.subjectID, data.title);
            }

            Log.Write("End science dump.");
        }

        private float GetBodyScienceValueMultipler(ExperimentSituations sit)
        {
            var b = FlightGlobals.currentMainBody;

            switch (sit)
            {
                case ExperimentSituations.FlyingHigh:
                    return b.scienceValues.FlyingHighDataValue;
                case ExperimentSituations.FlyingLow:
                    return b.scienceValues.FlyingLowDataValue;
                case ExperimentSituations.InSpaceHigh:
                    return b.scienceValues.InSpaceHighDataValue;
                case ExperimentSituations.InSpaceLow:
                    return b.scienceValues.InSpaceLowDataValue;
                case ExperimentSituations.SrfLanded:
                    return b.scienceValues.LandedDataValue;
                case ExperimentSituations.SrfSplashed:
                    return b.scienceValues.SplashedDataValue;
                default:
                    return 0f;
            }

        }

        // test calculations
        private void ExperimentValues()
        {
            foreach (var expid in ResearchAndDevelopment.GetExperimentIDs())
            {
                
                ScienceExperiment exp = ResearchAndDevelopment.GetExperiment(expid);
                ScienceSubject subject = ResearchAndDevelopment.GetExperimentSubject(exp, ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel), FlightGlobals.currentMainBody, ScienceUtil.GetExperimentBiome(FlightGlobals.currentMainBody, FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude));

                Log.Write("---- {0} -----", exp.experimentTitle);
 
                Log.Write("Current main body: {0}", FlightGlobals.currentMainBody.name);
                Log.Write("Current subject: {0}", subject.id);

                Log.Write("exp baseValue = {0}", exp.baseValue);
                Log.Write("exp dataScale = {0}", exp.dataScale);
                Log.Write("exp body multiplier = {0}", GetBodyScienceValueMultipler(ScienceUtil.GetExperimentSituation(FlightGlobals.ActiveVessel)));
                Log.Write("sub dataScale = {0}", subject.dataScale);
                Log.Write("sub subjectValue = {0}", ResearchAndDevelopment.GetSubjectValue(exp.baseValue * exp.dataScale, subject));
                Log.Write("sub scientificValue = {0}", subject.scientificValue);
                
                // for my reference: these are all correct
                Log.Write("reference value = {0}", ResearchAndDevelopment.GetReferenceDataValue(exp.baseValue * exp.dataScale, subject));
                Log.Write("this value = {0}", ResearchAndDevelopment.GetScienceValue(exp.dataScale * exp.baseValue, subject));
                Log.Write("next value = {0}", ResearchAndDevelopment.GetNextScienceValue(exp.baseValue * exp.dataScale, subject));

                Log.Write("//////////////");
            }
        }


        /// <summary>
        /// Writes the current situation being checked against for
        /// the eva experiment to the log.
        /// </summary>
        private void CurrentSituationEva()
        {
            var vessel = FlightGlobals.ActiveVessel;
            //var expSituation = indicator.VesselSituationToExperimentSituation();
            var expSituation = ScienceUtil.GetExperimentSituation(vessel);
            var biome = string.Empty;

            if (ResearchAndDevelopment.GetExperiment("evaReport").BiomeIsRelevantWhile(expSituation))
                if (!biomeFilter.GetBiome(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad, out biome))
                    biome = "[bad biome result]";

            var subject = ResearchAndDevelopment.GetExperimentSubject(ResearchAndDevelopment.GetExperiment("evaReport"), expSituation, vessel.mainBody, biome);

            Log.Warning("Current eva situation: {0}, science {1}", subject.id, subject.science);
        }


        private void CurrentSituationGravityScan()
        {
            var vessel = FlightGlobals.ActiveVessel;
            //var expSituation = indicator.VesselSituationToExperimentSituation();
            var expSituation = ScienceUtil.GetExperimentSituation(vessel);
            var biome = string.Empty;

            if (ResearchAndDevelopment.GetExperiment("gravityScan").BiomeIsRelevantWhile(expSituation))
                if (!biomeFilter.GetBiome(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad, out biome))
                    biome = "[bad biome result]";

            var subject = ResearchAndDevelopment.GetExperimentSubject(ResearchAndDevelopment.GetExperiment("gravityScan"), expSituation, vessel.mainBody, biome);

            Log.Warning("Current gravScan situation: {0}, total science {1}", subject.id, subject.science);
        }



        /// <summary>
        /// List real and fake transmitter counts
        /// </summary>
        private void TransmitterCheck()
        {
            var txs = FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataTransmitter>();

            Log.Write("There are {0} total transmitters - {1} real, {2} magic", txs.Count, txs.Count(tx => !(tx is MagicDataTransmitter)), txs.Count(tx => tx is MagicDataTransmitter));

            var magics = txs.Where(tx => tx is MagicDataTransmitter).ToList();

            foreach (var tx in magics)
            {
                var magic = tx as MagicDataTransmitter;

                Log.Write("MagicDataTransmitter {0} has {1} queued transmissions", magic.part.ConstructID, magic.QueuedData.Count);

                foreach (var data in magic.QueuedData)
                    Log.Write("  - {0}", data.subjectID);
            }
        }


        private void RenderOptions(int winid)
        {
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                GUILayout.Label("Debug Options", GUILayout.ExpandWidth(true));
                GUILayout.Space(10f);

                if (GUILayout.Button("Log Onboard ScienceData", GUILayout.ExpandWidth(true)))
                    DumpScienceData();

                if (GUILayout.Button("Log Current Eva SitID", GUILayout.ExpandWidth(true)))
                    CurrentSituationEva();

                if (GUILayout.Button("Log Current GravityScan SitID", GUILayout.ExpandWidth(true)))
                    CurrentSituationGravityScan();

                if (GUILayout.Button("Onboard Transmitters", GUILayout.ExpandWidth(true)))
                    TransmitterCheck();

                if (GUILayout.Button("Experiment current values", GUILayout.ExpandWidth(true)))
                    ExperimentValues();

            GUILayout.EndVertical();
        }

        public void Update()
        {
            // required by IDrawable
        }

        #region Message handling functions

        /// <summary>
        /// This message will be sent by ScienceAlert when the user
        /// changes scan interface types
        /// </summary>
        public void Notify_ScanInterfaceChanged()
        {
            Log.Debug("DebugWindow.Notify_ScanInterfaceChanged");

        }



        /// <summary>
        /// This message sent when toolbar has changed and re-registering
        /// for events is necessary
        /// </summary>
        public void Notify_ToolbarInterfaceChanged()
        {
            Log.Debug("DebugWindow.Notify_ToolbarInterfaceChanged");

            scienceAlert.Button.OnClick += OnToolbarClicked;
        }

        #endregion
    }
}
