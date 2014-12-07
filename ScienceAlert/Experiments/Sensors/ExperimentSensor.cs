using System.Collections.Generic;
using System.Linq;
using ReeperCommon;
using ScienceAlert.Experiments.Science;

namespace ScienceAlert.Experiments.Sensors
{
    public class ExperimentSensor : IExperimentSensor
    {

        // members
        private BiomeFilter filter;
        StoredVesselScience cache;                  

        private List<ModuleScienceExperiment> recoveryModules;                         // cache of all ModuleScienceExperiments on the Vessel, sorted
                                                                    // least-to-greatest by xmitScalar
        private List<ModuleScienceExperiment> transmitModules;                         // same as above, but in reverse order



        public ExperimentSensor(ScienceExperiment experiment, ProfileData.SensorSettings settings, StoredVesselScience cache, IEnumerable<ModuleScienceExperiment> modules)
        {
            Settings = settings;
            Experiment = experiment;

            this.cache = cache;
            this.filter = API.ScienceAlert.BiomeFilter;

            // sort onboard modules according to their efficiency at transmission. We want
            // to use the best transmission
            transmitModules = modules.OrderByDescending(mse => mse.xmitDataScalar).ToList();
            recoveryModules = new List<ModuleScienceExperiment>(transmitModules);
            recoveryModules.Reverse();
        }



        public void UpdateState(CelestialBody body, ExperimentSituations situation)
        {
            // get subject representing this experiment + biome + body + situation combo
            var subject = UpdateSubject(body, situation);

            if (subject.scienceCap < 0.01f || !IsRunnable)
            {
                RecoveryValue = TransmissionValue = 0f;
                Status = SensorState.NoAlert;
                return;
            }


            // science already collected and stored aboard the vessel will affect the next report's 
            // value so we need to find any similar samples and consider them in our projections of
            // the next report's value
            var stored = cache.FindStoredData(subject.id);

            UpdateScienceCollectionValues(subject, stored);

            Status = SensorState.NoAlert;

            if (RecoveryValue > 0.01f)
                SetRecoveryStatus(subject, stored);

            if (TransmissionValue > 0.01f)
                SetTransmissionStatus(subject, stored);
        }



        private ScienceSubject UpdateSubject(CelestialBody body, ExperimentSituations situation)
        {
            var subject = ResearchAndDevelopment.GetExperimentSubject(Experiment, situation, FlightGlobals.ActiveVessel.mainBody, Experiment.BiomeIsRelevantWhile(situation) ? filter.CurrentBiome : string.Empty);
            Subject = subject.id;
            return subject;
        }


        private void UpdateScienceCollectionValues(ScienceSubject subject, List<ScienceData> onboardSamples)
        {
            // calculate next recovery value
            RecoveryValue = subject.CalculateNextReport(Experiment, onboardSamples, 1f);

            // calculate next transmission value, using next best available module (it's possible for two
            // experiment modules on a Vessel to have different xmit scalars, though unlikely)
            TransmissionValue = subject.CalculateNextReport(Experiment, onboardSamples, GetNextModule(false).xmitDataScalar);
        }


        private void SetRecoveryStatus(ScienceSubject subject, List<ScienceData> onboardSamples)
        {
            float recoveryTotal = API.CalculateScienceTotal(Experiment, subject, onboardSamples);

            switch (Settings.Filter)
            {
                // "only alert me if I have no data whatsoever on this experiment"
                case ProfileData.SensorSettings.FilterMethod.Unresearched:
                    if (recoveryTotal < 0.001f)
                        Status |= SensorState.RecoveryAlert;
                    break;

                // "alert me if this subject has less than 50% of its science gathered"
                case ProfileData.SensorSettings.FilterMethod.LessThanFiftyPercent:
                    if (recoveryTotal / subject.scienceCap < 0.5f)
                        Status |= SensorState.RecoveryAlert;
                    break;

                // "alert me if this subject has less than 90% of its science gathered"
                case ProfileData.SensorSettings.FilterMethod.LessThanNinetyPercent:
                    if (recoveryTotal / subject.science < 0.9f)
                        Status |= SensorState.RecoveryAlert;
                    break;

                // "alert me if I can gain ANY science"
                case ProfileData.SensorSettings.FilterMethod.NotMaxed:
                    Status |= SensorState.RecoveryAlert;
                    break;
            }
            
        }


        private void SetTransmissionStatus(ScienceSubject subject, List<ScienceData> onboardSamples)
        {
            float transmitTotal = API.CalculateScienceTotal(Experiment, subject, onboardSamples,
                GetNextModule(false).xmitDataScalar);

 
            // todo: compare against settings
        }


        public bool DeployThisExperiment()
        {
            throw new System.NotImplementedException();
        }

        public void SetScienceModules(ModuleScienceExperiment moduleList)
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// Returns the next module best suited for either recovery or transmission
        /// </summary>
        /// <param name="forRecovery">True if this module will be used for recovery and so should be the worst xmitDataScalar</param>
        /// <returns></returns>
        protected virtual ModuleScienceExperiment GetNextModule(bool forRecovery = true)
        {
            return forRecovery
                ? recoveryModules.First(mse => !mse.Deployed && !mse.Inoperable)
                : transmitModules.First(mse => !mse.Deployed && !mse.Inoperable);
        }

#region properties


        /// <summary>
        /// Returns true if there's at least one module available for this experiment to run
        /// </summary>
        protected virtual bool IsRunnable
        {
            get
            {
                return recoveryModules.Any(mse => !mse.Deployed && !mse.Inoperable);
            }
        }


        public float RecoveryValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public SensorState Status { get; private set; }
        public string Subject { get; private set; }
        public ScienceExperiment Experiment { get; private set; }
        public ProfileData.SensorSettings Settings { get; private set; }

#endregion
    }
}
