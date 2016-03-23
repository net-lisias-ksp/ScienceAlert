using System.Collections.Generic;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class CompositeRuleBuilder : CompositeBuilder<IExperimentRule, IRuleBuilder, ITemporaryBindingFactory>,
        IRuleBuilder
    {
        public CompositeRuleBuilder(IEnumerable<IConfigNodeObjectBuilder<IExperimentRule, IRuleBuilder, ITemporaryBindingFactory>> builders) : base(builders)
        {
        }
    }
}