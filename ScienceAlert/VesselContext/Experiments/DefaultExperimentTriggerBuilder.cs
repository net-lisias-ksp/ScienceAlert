using System;

namespace ScienceAlert.VesselContext.Experiments
{
    /// <summary>
    /// Naive builder that will attempt to satisfy all dependencies using the injection framework
    /// </summary>
    /// <typeparam name="TProduced"></typeparam>
    /// <typeparam name="TParameter"></typeparam>
    public abstract class DefaultExperimentTriggerBuilder<TProduced, TParameter, TParameter2> : IObjectFromConfigNodeBuilder<TProduced, TParameter, TParameter2>
    {
        public TProduced Build(TParameter param1, TParameter2 param2, IObjectFromConfigNodeBuilder<TProduced, TParameter, TParameter2> rootBuilder = null)
        {
            throw new NotImplementedException();
        }

        public bool CanHandle(TParameter param1, TParameter2 param2, IObjectFromConfigNodeBuilder<TProduced, TParameter, TParameter2> rootBuilder = null)
        {
            throw new NotImplementedException();
        }
    }
}
