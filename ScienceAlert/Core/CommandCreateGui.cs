using System;
using ScienceAlert.Gui;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector;
using UnityEngine;

namespace ScienceAlert.Core
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandCreateGui : Command
    {
        private readonly GameObject _contextView;


        public CommandCreateGui([Name(ContextKeys.CONTEXT_VIEW)] GameObject contextView)
        {
            if (contextView == null) throw new ArgumentNullException("contextView");
            _contextView = contextView;
        }


        public override void Execute()
        {
            _contextView.AddComponent<ApplicationLauncherView>();
        }
    }
}
