using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class RuleBuilderProvider : IRuleBuilderProvider
    {
        private readonly List<IRuleBuilder> _builders = new List<IRuleBuilder>();
 
        public RuleBuilderProvider(IEnumerable<IRuleBuilder> builders)
        {
            if (builders == null) throw new ArgumentNullException("builders");
            _builders = builders.ToList();
        }


        public void AddBuilder(IRuleBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            _builders.AddUnique(builder);
        }


        public Maybe<IRuleBuilder> GetBuilder(ConfigNode config)
        {
            return _builders.FirstOrDefault(b => b.CanHandle(config)).ToMaybe();
        }
    }
}
