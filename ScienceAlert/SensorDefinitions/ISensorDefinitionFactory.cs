namespace ScienceAlert.SensorDefinitions
{
    interface ISensorDefinitionFactory
    {
        SensorDefinition CreateDefault(ScienceExperiment experiment);
    }
}