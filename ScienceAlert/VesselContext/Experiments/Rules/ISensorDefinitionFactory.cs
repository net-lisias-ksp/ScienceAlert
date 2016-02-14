namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public interface ISensorDefinitionFactory
    {
        SensorDefinition Create(ScienceExperiment experiment);
    }
}
