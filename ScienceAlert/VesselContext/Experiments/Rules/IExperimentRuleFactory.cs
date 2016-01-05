namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public interface IExperimentRuleFactory
    {
        IExperimentRule Create(ScienceExperiment experiment, RuleDefinition definition);
    }
}
