using System;
using System.Linq;
using JetBrains.Annotations;

namespace ScienceAlert
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CompositeObjectFromConfigNodeBuilder<TProduced, TParam, TParam2> : IObjectFromConfigNodeBuilder<TProduced, TParam, TParam2>
    {
        private readonly IObjectFromConfigNodeBuilder<TProduced, TParam, TParam2>[] _builders;

        public CompositeObjectFromConfigNodeBuilder([NotNull] IObjectFromConfigNodeBuilder<TProduced, TParam, TParam2>[] builders)
        {
            if (builders == null) throw new ArgumentNullException("builders");
            _builders = builders;
        }


        public TProduced Build(TParam param1, TParam2 param2, IObjectFromConfigNodeBuilder<TProduced, TParam, TParam2> rootBuilder)
        {
            foreach (var builder in _builders)
                if (builder.CanHandle(param1, param2, this))
                    return builder.Build(param1, param2, this);
            throw new ArgumentException("No builder can handle these parameters");
        }


        public bool CanHandle(TParam param1, TParam2 param2, IObjectFromConfigNodeBuilder<TProduced, TParam, TParam2> rootBuilder)
        {
            return _builders.Any(b => b.CanHandle(param1, param2, this));
        }
    }
}