using System.Collections.Generic;
using System.Linq;
using ReeperCommon;
using System;
using ScienceAlert.Experiments.Sensors;
using UnityEngine;

namespace ScienceAlert
{
    /// <summary>
    /// A common interface. Some people requested something like this, plus it'll help simplify some other
    /// planned features like the alert log
    /// </summary>
    public static class API
    {

        /// <summary>
        /// See GetNextReportValue
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="experiment"></param>
        /// <param name="onboard"></param>
        /// <param name="xmitScalar"></param>
        /// <returns></returns>
        public static float CalculateNextReport(this ScienceSubject subject, ScienceExperiment experiment, IEnumerable<ScienceData> onboard, float xmitScalar = 1f)
        {
            return GetNextReportValue(subject, experiment, onboard, xmitScalar);
        }


        /// <summary>
        /// Calculates the next report's value, based on existing onboard data and the transmission scalar. If xmitScalar is
        /// left at 1, GetNextReportValue will return the value of the next sample with the assumption that all onboard 
        /// samples will be recovered. If set to < 1, the assumption will be that all samples will be transmitted instead
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="experiment"></param>
        /// <param name="onboard"></param>
        /// <param name="xmitScalar"></param>
        /// <returns></returns>
        public static float GetNextReportValue(ScienceSubject subject, ScienceExperiment experiment, IEnumerable<ScienceData> onboard, float xmitScalar = 1f)
        {
            // I dislike having to create a new ScienceData but it's the simplest way to get the game to 
            // calculate the next report value for us
            //
            // note: it won't apply any bonus if there isn't a crewed lab on the active vessel
            var data = new ScienceData(experiment.baseValue * experiment.dataScale * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier, xmitScalar, 0f, subject.id, string.Empty);
            data.labBoost = ModuleScienceLab.GetBoostForVesselData(FlightGlobals.ActiveVessel, data);

            xmitScalar += data.labBoost;

            int itemCount = onboard.Count();
#if DEBUG
            //Log.Debug("GetNextReportValue for {0}, calculated labBoost of {1}", experiment.experimentTitle, data.labBoost);
#endif
            if (itemCount == 0)
                return ResearchAndDevelopment.GetScienceValue(experiment.baseValue * experiment.dataScale, subject, xmitScalar) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

            float experimentValue = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject, xmitScalar) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

            if (itemCount == 1)
                return experimentValue;

            // we'll have to estimate
            return experimentValue / UnityEngine.Mathf.Pow(4f, itemCount - 1);
        }



        /// <summary>
        /// Calculates the total science the player has for a given experiment's subject. This considers 
        /// recovered (or transmitted) experiments, plus ScienceData already onboard. The theoretical value
        /// of the next report isn't included.
        /// </summary>
        /// <param name="experiment"></param>
        /// <param name="subject"></param>
        /// <param name="onboard"></param>
        /// <param name="xmitScalar"></param>
        /// <returns></returns>
        public static float CalculateScienceTotal(ScienceExperiment experiment, ScienceSubject subject, List<ScienceData> onboard, float xmitScalar = 1f)
        {
            if (onboard.Count == 0)
            {
                // straight stored data
                return subject.science;
            }
            else
            {
                // we've got at least one report we need to consider
                float potentialScience = subject.science + ResearchAndDevelopment.GetScienceValue(onboard[0].dataAmount, subject) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

                if (onboard.Count > 1)
                {
                    float secondReport = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

                    potentialScience += secondReport;

                    // there's some kind of interpolation that the game does for
                    // subsequent experiments. Dividing by four seems to give fairly
                    // decent estimate. It's very unlikely that the exact science value
                    // after the second report is going to matter one way or the other
                    // though, so this is a decent enough solution for now
                    if (onboard.Count > 2)
                        for (int i = 3; i < onboard.Count; ++i)
                            potentialScience += secondReport / Mathf.Pow(4f, i - 2);
                }
                return potentialScience;
            }
        }



        /// <summary>
        /// A convenience method
        /// </summary>
        /// <param name="sit"></param>
        /// <returns></returns>
        public static float GetBodyScienceValueMultipler(ExperimentSituations sit)
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
    }
}
