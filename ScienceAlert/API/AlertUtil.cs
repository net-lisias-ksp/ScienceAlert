using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon;

namespace ScienceAlert.API
{
    public class AlertUtil
    {

        public static float GetNextReportValue(ScienceSubject subject, ScienceExperiment experiment, List<ScienceData> onboard, float xmitScalar = 1f)
        {
            var data = new ScienceData(experiment.baseValue * experiment.dataScale, xmitScalar, 0f, subject.id, string.Empty);
            data.labBoost = ModuleScienceLab.GetBoostForVesselData(FlightGlobals.ActiveVessel, data);

            //xmitScalar *= (data.labBoost + 1f);
            xmitScalar += data.labBoost;

            Log.Debug("GetNextReportValue for {0}, calculated labBoost of {1}", experiment.experimentTitle, data.labBoost);

            if (onboard.Count == 0)
                return ResearchAndDevelopment.GetScienceValue(experiment.baseValue * experiment.dataScale, subject, xmitScalar); 

            float experimentValue = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject, xmitScalar);

            if (onboard.Count == 1)
                return experimentValue;

            // we'll have to estimate
            return experimentValue / UnityEngine.Mathf.Pow(4f, onboard.Count - 1);

            //if (stored.Count == 0)
            //    return ResearchAndDevelopment.GetScienceValue(experiment.baseValue * experiment.dataScale, subject);

            //float experimentValue = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject);

            //if (stored.Count == 1)
            //    return experimentValue;


            //// for two or more, we'll have to estimate. Presumably there's some
            //// kind of interpolation going on. I've found that just dividing the previous
            //// value by four is a good estimate.
            //return experimentValue / Mathf.Pow(4f, stored.Count - 1);
        }
    }
}
