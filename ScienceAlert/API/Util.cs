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
using System.Collections.Generic;
using ReeperCommon;


namespace ScienceAlert.API
{
    public static class Util
    {
        public static float CalculateNextReport(this ScienceSubject subject, ScienceExperiment experiment, List<ScienceData> onboard, float xmitScalar = 1f)
        {
            return GetNextReportValue(subject, experiment, onboard, xmitScalar);
        }


        public static float GetNextReportValue(ScienceSubject subject, ScienceExperiment experiment, List<ScienceData> onboard, float xmitScalar = 1f)
        {
            var data = new ScienceData(experiment.baseValue * experiment.dataScale * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier, xmitScalar, 0f, subject.id, string.Empty);
            data.labBoost = ModuleScienceLab.GetBoostForVesselData(FlightGlobals.ActiveVessel, data);

            xmitScalar += data.labBoost;

#if DEBUG
            //Log.Debug("GetNextReportValue for {0}, calculated labBoost of {1}", experiment.experimentTitle, data.labBoost);
#endif
            if (onboard.Count == 0)
                return ResearchAndDevelopment.GetScienceValue(experiment.baseValue * experiment.dataScale, subject, xmitScalar) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

            float experimentValue = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject, xmitScalar) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

            if (onboard.Count == 1)
                return experimentValue;

            // we'll have to estimate
            return experimentValue / UnityEngine.Mathf.Pow(4f, onboard.Count - 1);
        }

        public static string GetCurrentBiome()
        {
            if (FlightGlobals.ActiveVessel != null)
            if (FlightGlobals.ActiveVessel.mainBody.BiomeMap != null)
                return !string.IsNullOrEmpty(FlightGlobals.ActiveVessel.landedAt)
                                ? Vessel.GetLandedAtString(FlightGlobals.ActiveVessel.landedAt)
                                : ScienceUtil.GetExperimentBiome(FlightGlobals.ActiveVessel.mainBody,
                                            FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude); 

            return string.Empty;
        }
    }
}
