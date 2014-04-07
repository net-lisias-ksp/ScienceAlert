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
using Toolbar;
using DebugTools;

namespace ScienceAlert
{
    /// <summary>
    /// This is an extra window accessible via the toolbar + middle mouse
    /// that should help in tracking down issues.
    /// </summary>
    public class DebugWindow : IDrawable
    {
        private readonly int windowId = UnityEngine.Random.Range(0, int.MaxValue);
        private Rect windowRect = new Rect(0, 0, 324, Screen.height / 5);
        ScienceAlert indicator;
        BiomeFilter biomeFilter;
        StorageCache storage;

        internal DebugWindow(ScienceAlert ind, BiomeFilter filter, StorageCache cache)
        {
            indicator = ind;
            biomeFilter = filter;
            storage = cache;
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


        /// <summary>
        /// Writes the current situation being checked against for
        /// the eva experiment to the log.
        /// </summary>
        private void CurrentSituationEva()
        {
            var vessel = FlightGlobals.ActiveVessel;
            var expSituation = indicator.VesselSituationToExperimentSituation();
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
            var expSituation = indicator.VesselSituationToExperimentSituation();
            var biome = string.Empty;

            if (ResearchAndDevelopment.GetExperiment("gravityScan").BiomeIsRelevantWhile(expSituation))
                if (!biomeFilter.GetBiome(vessel.latitude * Mathf.Deg2Rad, vessel.longitude * Mathf.Deg2Rad, out biome))
                    biome = "[bad biome result]";

            var subject = ResearchAndDevelopment.GetExperimentSubject(ResearchAndDevelopment.GetExperiment("gravityScan"), expSituation, vessel.mainBody, biome);

            Log.Warning("Current gravScan situation: {0}, science {1}", subject.id, subject.science);
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

            GUILayout.EndVertical();
        }

        public void Update()
        {
            // required by IDrawable
        }
    }
}
