using System.Collections.Generic;
using System.Reflection;
using strange.extensions.command.api;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.context.impl;
using UnityEngine;

namespace ScienceAlert.Core
{
    public class SignalContext : MVCSContext
    {
        protected SignalContext(MonoBehaviour view, ContextStartupFlags flags)
            : base(view, flags)
        {
        }


        protected override void mapBindings()
        {
            base.mapBindings();
            implicitBinder.ScanForAnnotatedClasses(
                new KeyValuePair<Assembly, string[]>(Assembly.GetExecutingAssembly(), new [] { "ScienceAlert" }));
        }


        protected override void addCoreComponents()
        {
            base.addCoreComponents();
            injectionBinder.Unbind<ICommandBinder>();
            injectionBinder.Bind<ICommandBinder>().To<SignalCommandBinder>().ToSingleton();
        }
    }
}
