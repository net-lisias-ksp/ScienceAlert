namespace ScienceAlert.SensorDefinitions
{
    public interface ISensorDefinitionFactory
    {
        SensorDefinition CreateDefault(ScienceExperiment experiment);
    }
}