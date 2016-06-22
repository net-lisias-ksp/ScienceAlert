using System;
using JetBrains.Annotations;
using strange.framework.api;

namespace ScienceAlert.Core
{
    // Adds a little bit to the builder composites: instead of referring to
    // IObjectFromConfigNodeBuilder<Blah, Blah, Blah>, we can create a quick shorthand interface that
    // implements it and then treat it the same way. For instance, IRuleBuilder instead of 
    // IOBjectFromConfigNodeBuilder<ISensorRule, ConfigNode, IInjectionBinder> or whatever the specific implementation is
    class CommandCreateBuilderCompositeWithShorthand<TInterfaceOfCompositeAndBuilders, TComposite, TShorthand> :
        CommandCreateBuilderComposite<TInterfaceOfCompositeAndBuilders, TComposite>
        where TInterfaceOfCompositeAndBuilders : class
        where TComposite : TInterfaceOfCompositeAndBuilders
        where TShorthand : TComposite
    {
        private readonly IInstanceProvider _provider;

        public CommandCreateBuilderCompositeWithShorthand([NotNull] ITemporaryBindingFactory tempBindingFactory, [NotNull] ICriticalShutdownEvent failSignal,
            [NotNull] IInstanceProvider provider) : base(tempBindingFactory, failSignal)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            _provider = provider;
        }

        public override void Execute()
        {
            base.Execute();
            injectionBinder.Bind<TShorthand>().To(_provider.GetInstance<TInterfaceOfCompositeAndBuilders>()).CrossContext();
        }
    }
}
