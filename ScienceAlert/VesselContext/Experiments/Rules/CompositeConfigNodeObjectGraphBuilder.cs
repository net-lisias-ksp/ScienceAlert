using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon.Containers;

namespace ScienceAlert.VesselContext.Experiments.Rules
{
    public class CompositeConfigNodeObjectGraphBuilder<TTypeProduced> : IConfigNodeObjectGraphBuilder<TTypeProduced>
    {
        private readonly IConfigNodeObjectGraphBuilder<TTypeProduced>[] _builders;

        public CompositeConfigNodeObjectGraphBuilder(params IConfigNodeObjectGraphBuilder<TTypeProduced>[] builders)
        {
            if (builders == null) throw new ArgumentNullException("builders");
            _builders = builders;
        }


        public CompositeConfigNodeObjectGraphBuilder(IEnumerable<IConfigNodeObjectGraphBuilder<TTypeProduced>> builders) : this(builders.ToArray())
        {
            
        }


        private Maybe<IConfigNodeObjectGraphBuilder<TTypeProduced>> GetGraphBuilderFor(ConfigNode config)
        {
            foreach (var builder in _builders)
                if (builder.CanHandle(config))
                    return Maybe<IConfigNodeObjectGraphBuilder<TTypeProduced>>.With(builder);

            return Maybe<IConfigNodeObjectGraphBuilder<TTypeProduced>>.None;
        }


        public TTypeProduced Build(ConfigNode config)
        {
            return Build(this, config);
        }

        public TTypeProduced Build(IConfigNodeObjectGraphBuilder<TTypeProduced> builder, ConfigNode config)
        {
            var graphBuilder = GetGraphBuilderFor(config);

            if (!graphBuilder.Any())
                throw new ArgumentException("No builder can handle " + config.name);

            return graphBuilder.Value.Build(builder, config);
        }


        public bool CanHandle(ConfigNode config)
        {
            return GetGraphBuilderFor(config).Any();
        }
    }
}
