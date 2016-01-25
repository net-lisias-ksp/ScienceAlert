using System;
using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments
{
    public class ExperimentSensor
    {
        public readonly ScienceExperiment Experiment;
        private readonly IQueryScienceSubject _scienceSubjectQuery;
        private readonly IExperimentReportValueCalculator _reportCalculator;

        private IScienceSubject _currentSubject;
 
        public ExperimentSensor(
            ScienceExperiment experiment, 
            IQueryScienceSubject scienceSubjectQuery,
            IExperimentReportValueCalculator reportCalculator)
        {
            if (experiment == null) throw new ArgumentNullException("experiment");
            if (scienceSubjectQuery == null) throw new ArgumentNullException("scienceSubjectQuery");
            if (reportCalculator == null) throw new ArgumentNullException("reportCalculator");

            Experiment = experiment;
            _scienceSubjectQuery = scienceSubjectQuery;
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

            HasChanged =
                SetValueAndCheckForChange(CollectionValue, CalculateCollectionValue(), v => CollectionValue = v) ||
                SetValueAndCheckForChange(TransmissionValue, CalculateTransmissionValue(), v => TransmissionValue = v) ||
                SetValueAndCheckForChange(LabValue, CalculateLabValue(), v => LabValue = v) ||
                SetValueAndCheckForChange(Onboard, false, b => Onboard = b) ||
                SetValueAndCheckForChange(Available, false, b => Available = b);
        }


        private static bool SetValueAndCheckForChange<T>(T currentValue, T newValue, Action<T> setAction) where T: struct
        {
            setAction(newValue);
            return currentValue.Equals(newValue);
        }


        private float CalculateCollectionValue()
        {
            return _reportCalculator.CalculateCollectionValue(Experiment, _currentSubject);
        }

        private float CalculateTransmissionValue()
        {
            return _reportCalculator.CalculateTransmissionValue(Experiment, _currentSubject);
        }


        private float CalculateLabValue()
        {
            return _reportCalculator.CalculateLabValue(Experiment, _currentSubject);
        }


        private IScienceSubject GetCurrentScienceSubject()
        {
            return _scienceSubjectQuery.GetSubject(Experiment);
        }
    }
}
