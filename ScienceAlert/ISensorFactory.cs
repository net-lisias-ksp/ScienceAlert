using System;

namespace ScienceAlert
{
    public interface ISensorFactory
    {
        ISensor Create(ScienceExperiment experiment, Action<ISensor> onTriggerAction);
    }
}
