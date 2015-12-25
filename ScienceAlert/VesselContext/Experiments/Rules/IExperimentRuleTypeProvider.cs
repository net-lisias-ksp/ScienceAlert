using System;
using System.Collections.Generic;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public interface IExperimentRuleTypeProvider
    {
        IEnumerable<Type> Get();
    }
}
