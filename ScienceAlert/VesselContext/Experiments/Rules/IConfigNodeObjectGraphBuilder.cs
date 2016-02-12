namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public interface IConfigNodeObjectGraphBuilder<TResultingObject>
    {
        TResultingObject Build(IConfigNodeObjectGraphBuilder<TResultingObject> builder, ConfigNode config);
        TResultingObject Build(ConfigNode config);
        bool CanHandle(ConfigNode config);
    }
}
