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
using System;
using ScienceAlert.Experiments.Monitors;


namespace ScienceAlert
{
    /// <summary>
    /// A common interface. Some people requested something like this, plus it'll help simplify some other
    /// planned features like the alert log
    /// </summary>
    public static class API
    {

        [Flags]
        public enum ExperimentStatus
        {
            NoAlert         = 0,                                      // This monitor's settings + current situation = don't alert user
            Recoverable     = 1 << 0,                                 // Alert user; this experiment matches their recovery filter
            Transmittable   = 1 << 1,                                 // Alert user; matches transmission filter
            
            Both = Recoverable | Transmittable
        }

        //---------------------------------------------------------------------
        // Delegate prototypes
        //---------------------------------------------------------------------
        public delegate void ExperimentStatusChanged(ExperimentStatus newStatus, ScienceExperiment experiment, ExperimentMonitor monitor);
        
        // Simpler events if subscriber doesn't need to know all the details, only that an alert has popped
        public delegate void ExperimentRecoveryAlert(ScienceExperiment experiment, float recoveryValue);
        public delegate void ExperimentTransmittableAlert(ScienceExperiment experiment, float transmissionValue);


        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------
        public static event ExperimentStatusChanged OnExperimentStatusChanged = delegate { };
        public static event ExperimentRecoveryAlert OnExperimentRecoveryAlert = delegate { };
        public static event ExperimentTransmittableAlert OnExperimentTransmittableAlert = delegate { };




        /// <summary>
        /// See GetNextReportValue
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="experiment"></param>
        /// <param name="onboard"></param>
        /// <param name="xmitScalar"></param>
        /// <returns></returns>
        public static float CalculateNextReport(this ScienceSubject subject, ScienceExperiment experiment, List<ScienceData> onboard, float xmitScalar = 1f)
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
        public static float GetNextReportValue(ScienceSubject subject, ScienceExperiment experiment, List<ScienceData> onboard, float xmitScalar = 1f)
        {
            // I dislike having to create a new ScienceData but it's the simplest way to get the game to 
            // calculate the next report value for us
            //
            // note: it won't apply any bonus if there isn't a crewed lab on the active vessel
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
