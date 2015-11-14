using System;
using System.Linq;
using System.Reflection;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperCommon.Repositories;
using ScienceAlert.Core;
using strange.extensions.command.impl;
using UnityEngine;

namespace ScienceAlert.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandConfigureGuiSkinsAndTextures : Command
    {
        private readonly IResourceRepository _resources;
        private readonly ILog _log;

        private const float TitleBarButtonSize = 16f;

        public CommandConfigureGuiSkinsAndTextures(IResourceRepository resources, ILog log)
        {
            if (resources == null) throw new ArgumentNullException("resources");
            if (log == null) throw new ArgumentNullException("log");

            _resources = resources;
            _log = log;
        }


        public override void Execute()
        {
            ConfigureSkins();
            ConfigureTextures();

            _log.Debug("Finished configuring GUI skins and textures");
        }


        private void ConfigureSkins()
        {
            _log.Verbose("Configuring GUI skins");
            injectionBinder.Bind<GUISkin>().ToValue(HighLogic.Skin).CrossContext();
            injectionBinder.Bind<GUIStyle>().ToValue(ConfigureTitleBarButtonStyle()).ToName(Keys.WindowTitleBarButtonStyle).CrossContext();
        }


        private void ConfigureTextures()
        {
            _log.Verbose("Configuring GUI textures");

            Assembly.GetExecutingAssembly()
                .GetManifestResourceNames()
                .ToList()
                .ForEach(n => _log.Normal("Resource: " + n));

            BindTexture("Resources/sheet_app", Keys.ApplicationLauncherSpriteSheet);
            BindTexture("Resources/btnClose", Keys.CloseButtonTexture);
            BindTexture("Resources/btnLock", Keys.LockButtonTexture);
            BindTexture("Resources/btnUnlock", Keys.UnlockButtonTexture);
            BindTexture("Resources/btnScale", Keys.RescaleCursorTexture);
            BindTexture("Resources/cursor", Keys.ResizeCursorTexture);
        }


        private static GUIStyle ConfigureTitleBarButtonStyle()
        {
            var style = new GUIStyle(HighLogic.Skin.button) { border = new RectOffset(), padding = new RectOffset() };
            style.fixedHeight = style.fixedWidth = TitleBarButtonSize;
            style.margin = new RectOffset();

            return style;
        }


        private void BindTexture(string url, object name)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Must contain a value", "url");

            _resources.GetTexture(url).SingleOrDefault()
                .IfNull(() =>
                {
                    throw new TextureNotFoundException(url);
                }).Do(t => injectionBinder.Bind<Texture2D>().ToValue(t).ToName(name).CrossContext());
        }
    }
}
