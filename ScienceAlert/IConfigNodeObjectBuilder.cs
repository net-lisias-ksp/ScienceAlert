namespace ScienceAlert
{
    interface IConfigNodeObjectBuilder<out TResultingObject>
    {
        TResultingObject Build(ConfigNode config);
        bool CanHandle(ConfigNode config);
    }

    interface IConfigNodeObjectBuilder<out TResultingObject, in TParamType1, in TParamType2, in TParamType3>
    {
        TResultingObject Build(ConfigNode config, TParamType1 parameter, TParamType2 parameter2, TParamType3 parameter3);
        bool CanHandle(ConfigNode config);
    }
}
