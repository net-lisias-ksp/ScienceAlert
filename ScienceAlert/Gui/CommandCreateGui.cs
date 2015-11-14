using System;
using strange.extensions.command.impl;
using strange.extensions.context.api;
using strange.extensions.injector;
using UnityEngine;

namespace ScienceAlert.Gui
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
            MapBindings();
            CreateViews();
        }


        private void CreateViews()
        {
            _contextView.AddComponent<ApplicationLauncherView>();
            _contextView.AddComponent<AlertPanelView>();
        }


        private void MapBindings()
        {
            injectionBinder.Bind<SignalButtonCreated>().ToSingleton().CrossContext();
            injectionBinder.Bind<SignalButtonToggled>().ToSingleton().CrossContext();
        }
    }
}
