namespace ScienceAlert.VesselContext.Experiments.ValuePredictors
{
    public interface ICollectionValuePredictor
    {
        float PredictCollectionValue(ScienceExperiment experiment);
    }
}
