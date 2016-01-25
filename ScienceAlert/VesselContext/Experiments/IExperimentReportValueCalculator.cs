using ScienceAlert.Game;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentReportValueCalculator
    {
        float CalculateCollectionValue(ScienceExperiment experiment, IScienceSubject subject);

        float CalculateTransmissionValue(ScienceExperiment experiment, IScienceSubject subject);

        float CalculateLabValue(ScienceExperiment experiment, IScienceSubject subject);
    }
}
