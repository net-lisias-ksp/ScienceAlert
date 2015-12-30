using System;
using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Extensions;
using ReeperCommon.Logging;
using ReeperCommon.Repositories;
using strange.extensions.command.impl;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ScienceAlert.Core.Gui
{
// ReSharper disable once ClassNeverInstantiated.Global
    public class CommandConfigureGuiSkinsAndTextures : Command
    {
        private readonly IResourceRepository _resources;

        private const float TitleBarButtonSize = 16f;

        public CommandConfigureGuiSkinsAndTextures(IResourceRepository resources)
        {
            if (resources == null) throw new ArgumentNullException("resources");

            _resources = resources;
        }


        public override void Execute()
        {
            ConfigureSkins();
            ConfigureTextures();

            Log.Debug("Finished configuring GUI skins and textures");
        }


        private void ConfigureSkins()
        {
            Log.Verbose("Configuring GUI skins");

            injectionBinder.Bind<GUISkin>().ToValue(HighLogic.Skin).CrossContext();

            ConfigureCompactSkin();
            ConfigurePopupSkin();
            ConfigureAlertViewToggle();
        }


        private void ConfigureCompactSkin()
        {
            // we need to trim off some fat on this skin to make the UI use space a bit more efficiently...
            var customSkin = (Object.Instantiate(HighLogic.Skin) as GUISkin).IfNull(() => { throw new SkinNotCreatedException(HighLogic.Skin); });

            customSkin.Do(s =>
            {
                s.window.padding = s.window.margin = new RectOffset();

                s.scrollView.padding = new RectOffset(1, 1, 1, 1);
                s.scrollView.margin = new RectOffset(1, 1, 1, 1);

                s.button.padding = new RectOffset(1, 1, 1, 1);
                s.button.margin = new RectOffset(1, 1, 1, 1);


                //s.button.fixedWidth = largestButtonDimensions.x;
                //s.button.fixedHeight = largestButtonDimensions.y;
                //s.button.fontSize = 12;
                //s.button.fontStyle = FontStyle.Normal;
                s.button.contentOffset = new Vector2(0f, 3f);
                s.button.fontSize = 14;
                s.button.stretchWidth = true;
                s.button.stretchHeight = true;

                s.toggle.padding = new RectOffset(1, 1, 1, 1);
                s.toggle.margin = new RectOffset(1, 1, 1, 1);
                s.toggle.contentOffset = Vector2.zero;
                s.toggle.overflow = new RectOffset();

                s.label.padding = s.label.margin = new RectOffset(1, 1, 1, 1);
                s.label.contentOffset = new Vector2(3f, 3f);
                s.label.fontStyle = FontStyle.Bold;
            });
            
            injectionBinder.Bind<GUISkin>().ToValue(customSkin).ToName(GuiKeys.CompactSkin).CrossContext();
            injectionBinder.Bind<GUIStyle>().ToValue(ConfigureTitleBarButtonStyle()).ToName(GuiKeys.WindowTitleBarButtonStyle).CrossContext();
        }


        private void ConfigurePopupSkin()
        {
            var customSkin = (Object.Instantiate(injectionBinder.GetInstance<GUISkin>()) as GUISkin).IfNull(
                () => { throw new SkinNotCreatedException("PopupSkin"); });

            customSkin.label.fontStyle = FontStyle.Normal;
            customSkin.label.fontSize = 12;
            customSkin.label.normal.textColor = Color.white;

            injectionBinder.Bind<GUISkin>().ToValue(customSkin).ToName(GuiKeys.PopupSkin).CrossContext();
        }

        /// <summary>
        /// The indicators on the alert panel are going to be toggle buttons. We need to customize them a bit with
        /// the proper textures and cut down on wasted space
        /// </summary>
        private void ConfigureAlertViewToggle()
        {
            var skin = injectionBinder.GetInstance<GUISkin>(GuiKeys.CompactSkin);

            var toggle = new GUIStyle(skin.toggle);
            var unlit = GetTexture("Resources/toggle_frame");
            var lit = Object.Instantiate(unlit) as Texture2D;

            unlit.ChangeLightness(0.25f); // darken a bit
            unlit.Apply();

            var largestButtonDimensions = CalculateButtonDimensions(skin.button);

            toggle.fixedWidth = toggle.fixedHeight = largestButtonDimensions.y;
            toggle.normal.background = unlit;
            toggle.active.background = unlit;
            toggle.hover.background = unlit;

            toggle.onNormal.background = lit;
            toggle.onHover.background = lit;
            toggle.onActive.background = lit;

            injectionBinder.Bind<GUIStyle>().To(toggle).ToName(GuiKeys.LitToggleStyle).CrossContext();
        }


        private Vector2 CalculateButtonDimensions(GUIStyle buttonStyle)
        {
            // todo: actual longest experiment name
            return buttonStyle.CalcSize(new GUIContent("Experiment.........................."));
        }


        private void ConfigureTextures()
        {
            Log.Verbose("Configuring GUI textures");

            BindTexture("Resources/sheet_app", GuiKeys.ApplicationLauncherSpriteSheet);
            BindTexture("Resources/btnClose", GuiKeys.CloseButtonTexture);
            BindTexture("Resources/btnLock", GuiKeys.LockButtonTexture);
            BindTexture("Resources/btnUnlock", GuiKeys.UnlockButtonTexture);
            BindTexture("Resources/btnScale", GuiKeys.RescaleCursorTexture);
            BindTexture("Resources/cursor", GuiKeys.ResizeCursorTexture);
            BindTexture("Squad/Contracts/Icons/dish", GuiKeys.DishTexture);
            BindTexture("Squad/Contracts/Icons/report", GuiKeys.ClipboardTexture);
            BindTexture("Squad/Contracts/Icons/sample", GuiKeys.SampleTexture);
        }


        private static GUIStyle ConfigureTitleBarButtonStyle()
        {
            var style = new GUIStyle(HighLogic.Skin.button) { border = new RectOffset(), padding = new RectOffset() };
            style.fixedHeight = style.fixedWidth = TitleBarButtonSize;
            style.margin = new RectOffset();

            return style;
        }


        private Texture2D GetTexture(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("url must contain a value", "url");

            return _resources.GetTexture(url).SingleOrDefault().IfNull(
                () => { throw new TextureNotFoundException(url); });
        }


        private void BindTexture(string url, object name)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Must contain a value", "url");

            GetTexture(url).Do(t => injectionBinder.Bind<Texture2D>().ToValue(t).ToName(name).CrossContext());
        }
    }
}
