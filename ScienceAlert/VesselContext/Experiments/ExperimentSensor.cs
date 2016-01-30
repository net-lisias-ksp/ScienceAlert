using System;
using System.Linq;
using ScienceAlert.Game;
using UnityEngine;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensor
    {
        public readonly ScienceExperiment Experiment;
        private readonly IScienceSubjectProvider _scienceSubjectProvider;
        private readonly IExperimentReportValueCalculator _reportCalculator;

        private IScienceSubject _currentSubject;
        

        public ExperimentSensor(
            ScienceExperiment experiment, 
            IScienceSubjectProvider scienceSubjectProvider,
            IExperimentReportValueCalculator reportCalculator)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (scienceSubjectProvider == null) throw new ArgumentNullException("scienceSubjectProvider");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");

            Experiment = experiment;
            _scienceSubjectProvider = scienceSubjectProvider;
            _reportCalculator = reportCalculator;

        }


        public bool HasChanged { get; private set; }
        public float CollectionValue { get; private set; }
        public float TransmissionValue { get; private set; }
        public float LabValue { get; private set; }
        public bool Onboard { get; private set; }
        public bool Available { get; private set; }


        public void ClearChangedFlag()
        {
            HasChanged = false;
        }


        public void UpdateSensorValues()
        {
            _currentSubject = GetCurrentScienceSubject();

            var oldCollection = CollectionValue;
            var oldTransmission = TransmissionValue;
            var oldLab = LabValue;
            var oldOnboard = Onboard;
            var oldAvailable = Available;

            UpdateCollectionValue();
            UpdateTransmissionValue();
            UpdateLabValue();
            UpdateOnboardValue();
            UpdateAvailabilityValue();

            HasChanged = !Mathf.Approximately(oldCollection, CollectionValue) ||
                         !Mathf.Approximately(oldTransmission, TransmissionValue) ||
                         !Mathf.Approximately(oldLab, LabValue) || oldOnboard != Onboard || oldAvailable != Available;
        }


        private void UpdateCollectionValue()
        {
            CollectionValue = _reportCalculator.CalculateCollectionValue(Experiment, _currentSubject);
        }


        private void UpdateTransmissionValue()
        {
            TransmissionValue = _reportCalculator.CalculateTransmissionValue(Experiment, _currentSubject);
        }


        private void UpdateLabValue()
        {
            LabValue = _reportCalculator.CalculateLabValue(Experiment, _currentSubject);
        }


        private void UpdateOnboardValue()
        {
            Onboard = false; // todo
        }


        private void UpdateAvailabilityValue()
        {
            Available = false; // todo
        }


        private IScienceSubject GetCurrentScienceSubject()
        {
            return _scienceSubjectProvider.GetSubject(Experiment);
        }
    }
}
