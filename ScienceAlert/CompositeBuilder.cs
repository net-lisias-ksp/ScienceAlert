using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;

namespace ScienceAlert
{
    [DoNotAutoRegister]
    public class CompositeBuilder<TResultingObject, TParamType1, TParamType2, TParamType3> : 
        IConfigNodeObjectBuilder<TResultingObject, TParamType1, TParamType2, TParamType3>
    {
        private readonly List<IConfigNodeObjectBuilder<TResultingObject, TParamType1, TParamType2, TParamType3>> _builders;

        public CompositeBuilder(
            IEnumerable<IConfigNodeObjectBuilder<TResultingObject, TParamType1, TParamType2, TParamType3>> builders)
        {
            if (builders == null) throw new ArgumentNullException("builders");
            _builders = builders.ToList();
        }


        public TResultingObject Build(ConfigNode config, TParamType1 parameter, TParamType2 parameter2, TParamType3 parameter3)
        {
            var builder = GetBuilderFor(config);

            if (!builder.Any())
                throw new ArgumentException("No builder found that can handle " + (config != null ? config.ToSafeString() : "<null ConfigNode>"));

            return builder.Value.Build(config, parameter, parameter2, parameter3);
        }


        public bool CanHandle(ConfigNode config)
        {
            return GetBuilderFor(config).Any();
        }


        private Maybe<IConfigNodeObjectBuilder<TResultingObject, TParamType1, TParamType2, TParamType3>> GetBuilderFor(
            ConfigNode config)
        {
            return _builders.FirstOrDefault(builder => builder.CanHandle(config)).ToMaybe();
        }


        public void AddBuilder(IConfigNodeObjectBuilder<TResultingObject, TParamType1, TParamType2, TParamType3> builder)
        {
            if (builder == null) throw new ArgumentNullException("builder");

            _builders.AddUnique(builder);
        }
    }
}