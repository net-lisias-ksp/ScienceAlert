namespace ScienceAlert.VesselContext.Experiments.Rules
{
    // just a marker to make finding the right builder a little easier when injecting
    public interface IRuleBuilder : IConfigNodeObjectBuilder<IExperimentRule, IRuleBuilder, ITemporaryBindingFactory>
    {
    }
}