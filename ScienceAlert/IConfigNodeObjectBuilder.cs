namespace ScienceAlert
{
    public interface IConfigNodeObjectBuilder<out TResultingObject>
    {
        TResultingObject Build(ConfigNode config);
        bool CanHandle(ConfigNode config);
    }

    public interface IConfigNodeObjectBuilder<out TResultingObject, in TParamType1, in TParamType2>
    {
        TResultingObject Build(ConfigNode config, TParamType1 parameter, TParamType2 parameter2);
        bool CanHandle(ConfigNode config);
    }
}
