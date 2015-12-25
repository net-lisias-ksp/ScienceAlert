namespace ScienceAlert.VesselContext.Experiments.ValuePredictors
{
    public interface ILabDataPredictor
    {
        float PredictLabData(ScienceExperiment experiment);
    }
}
