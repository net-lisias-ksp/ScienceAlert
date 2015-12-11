using System;
using System.Collections.Generic;

namespace ScienceAlert.Rules
{
    public interface IExperimentRuleTypeProvider
    {
        IEnumerable<Type> Get();
    }
}
