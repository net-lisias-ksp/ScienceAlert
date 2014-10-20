using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon;

/// WIP; cut this release because nothing is broken, redesign takes time and people have been waiting a long
/// time for this update already
namespace ScienceAlert.API
{
    public static class AlertUtil
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
    }
}
