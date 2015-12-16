//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace ScienceAlert.Experiments
//{

//    /// <summary>
//    /// ExperimentSensor is only interested in reporting current values: it doesn't care what state
//    /// anything is in at all. SensorMonitor will worry about what to do with this data
//    /// </summary>
//    public class ExperimentSensor : ISensor
//    {
//        private readonly ScienceExperiment _experiment;

//        public ExperimentSensor(ScienceExperiment experiment)
//        {
//            if (experiment == null) throw new ArgumentNullException("experiment");

//            _experiment = experiment;
//            CollectionValue = TransmissionValue = LabDataValue = 0f;
//            TransmissionMultiplier = 0f;
//        }


//        public void Poll()
//        {
//            var vessel = FlightGlobals.ActiveVessel;
//            var vesselSituation = ScienceUtil.GetExperimentSituation(vessel);
//            var vesselBiome = string.IsNullOrEmpty(vessel.landedAt) ? ScienceUtil.GetExperimentBiome(vessel.mainBody, vessel.latitude, vessel.longitude) : Vessel.GetLandedAtString(vessel.landedAt);

//            var subject = GetScienceSubject(_experiment, vesselSituation, vessel.mainBody, vesselBiome);
//            var existingReports = GetExistingReports(subject);

//            CollectionValue = CalculateCollectionValue(subject, existingReports);
//            TransmissionValue = CalculateTransmissionValue(subject, existingReports);
//            LabDataValue = CalculateLabDataValue(subject);
//        }


//        private float CalculateCollectionValue(ScienceSubject subject, List<ScienceData> existingReports)
//        {
//            return GetNextReportValue(subject, _experiment, existingReports);
//        }


//        private float CalculateTransmissionValue(ScienceSubject subject, List<ScienceData> existingReports)
//        {
//            return GetNextReportValue(subject, _experiment, existingReports, TransmissionMultiplier);
//        }


//        private float CalculateLabDataValue(ScienceSubject subject)
//        {
//            var lab = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleScienceLab>()
//                .FirstOrDefault();
//            return lab == null
//                ? 0f
//                : Mathf.Round(GetScienceLabMultiplier(lab, subject) *
//                              ResearchAndDevelopment.GetReferenceDataValue(_experiment.baseValue * _experiment.dataScale,
//                                  subject) * HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier);
//        }


//        private static ScienceSubject GetScienceSubject(ScienceExperiment experiment, ExperimentSituations situation, CelestialBody body, string biome)
//        {
//            if (experiment == null) throw new ArgumentNullException("experiment");
//            if (body == null) throw new ArgumentNullException("body");


//            var subj = new ScienceSubject(experiment, situation, body, biome);
//            var subj1 = subj;
//            var existingSubj = ResearchAndDevelopment.GetSubjects().FirstOrDefault(ss => ss.id == subj1.id);

//            subj = existingSubj ?? subj;

//            return subj;
//        }


//        private static List<ScienceData> GetExistingReports(ScienceSubject subject)
//        {
//            return FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataContainer>()
//                .SelectMany(container => container.GetData())
//                .Where(data => data.subjectID == subject.id)
//                .ToList();
//        }

//        private static float GetNextReportValue(ScienceSubject subject, ScienceExperiment experiment, List<ScienceData> onboard, float xmitScalar = 1f)
//        {
//            var careerMultiplier = HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

//            if (!onboard.Any())
//                return ResearchAndDevelopment.GetScienceValue(experiment.baseValue * experiment.dataScale, subject, xmitScalar) * careerMultiplier;

//            var experimentValue = ResearchAndDevelopment.GetNextScienceValue(experiment.baseValue * experiment.dataScale, subject, xmitScalar) * careerMultiplier;

//            if (onboard.Count == 1)
//                return experimentValue;

//            return experimentValue / Mathf.Pow(4f, onboard.Count - 1);
//        }


//        private static float GetScienceLabMultiplier(ModuleScienceLab lab, ScienceSubject subject)
//        {
//            var multiplier = 1f;

//            // lab can't process data twice
//            if (lab.ExperimentData.Any(storedId => storedId == subject.id)) return 0f;

//            if (lab.vessel.Landed)
//                multiplier *= 1f + lab.SurfaceBonus;

//            if (subject.id.Contains(lab.vessel.mainBody.bodyName))
//                multiplier *= 1f + lab.ContextBonus;

//            if ((lab.vessel.Landed || lab.vessel.Splashed) &&
//                ReferenceEquals(FlightGlobals.GetHomeBody(), lab.vessel.mainBody))
//                multiplier *= lab.homeworldMultiplier; // lack of addition intended

//            return multiplier;
//        }


//        public float CollectionValue { get; private set; }
//        public float TransmissionValue { get; private set; }
//        public float LabDataValue { get; private set; }

//        public float TransmissionMultiplier { get; set; }
//    }
//}
