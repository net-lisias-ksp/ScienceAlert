namespace ScienceAlert.VesselContext.Experiments.ValuePredictors
{
    public interface ITransmissionValuePredictor
    {
        float PredictTransmissionValue(ScienceExperiment experiment);
    }
}
