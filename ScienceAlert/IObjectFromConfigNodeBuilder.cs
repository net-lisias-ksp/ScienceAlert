namespace ScienceAlert
{
    public interface IObjectFromConfigNodeBuilder<TProduced, TParam, TParam2>
    {
        TProduced Build(TParam param1, TParam2 param2, IObjectFromConfigNodeBuilder<TProduced, TParam, TParam2> rootBuilder = null);
        bool CanHandle(TParam param1, TParam2 param2, IObjectFromConfigNodeBuilder<TProduced, TParam, TParam2> rootBuilder = null);
    }
}
