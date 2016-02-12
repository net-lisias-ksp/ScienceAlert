using System;
using System.Collections;
using ReeperCommon.Containers;
using ScienceAlert.Gui;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.Core.Gui
{
    [MediatedBy(typeof(ApplicationLauncherMediator))]
// ReSharper disable once ClassNeverInstantiated.Global
    public class ApplicationLauncherView : View
    {
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

        [Inject(global::ScienceAlert.GuiKeys.ApplicationLauncherSpriteSheet)] public Texture2D SpriteSheet { get; set; }
        [Inject] public ICoroutineRunner CoroutineRunner { get; set; }
        [Inject] public IGuiConfiguration GuiConfiguration { get; set; }

        internal readonly Signal<bool> Toggle = new Signal<bool>();
        internal readonly Signal ButtonCreated = new Signal();

        private ApplicationLauncherButton _button;
        private PackedSprite _sprite;

        private const string EzGuiLayerName = "EzGUI_UI";
        private const string ShaderName = "Sprite/Vertex Colored";
        private const int ButtonWidth = 38;
        private const int ButtonHeight = 38;


        public enum Animations
        {
            Unlit,
            Lit,
            Spinning
        }

        protected override void Start()
        {
            base.Start();
            CoroutineRunner.StartCoroutine(SetupButton());
        }


        protected override void OnDestroy()
        {
            ApplicationLauncher.Instance.Do(l => _button.Do(l.RemoveModApplication));

            base.OnDestroy();
        }


        private IEnumerator SetupButton()
        {
            while (ApplicationLauncher.Instance == null)
                yield return 0;


            _sprite = CreateSprite(CreateMaterial());

            _button = ApplicationLauncher.Instance.AddModApplication(
                                                        OnTrue,
                                                        OnFalse,

                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                                                        _sprite);

            yield return new WaitForEndOfFrame();   // the button won't respect toggle state immediately for some reason,
                                                    // so a slight delay is necessary while it finishes doing whatever internal setup

            SetAnimationState(Animations.Unlit);
            SetAnimationState(Animations.Spinning); // temp to make sure it's working

            ButtonCreated.Dispatch();
        }


        private Material CreateMaterial()
        {
            return Shader.Find(ShaderName)
                .With(s => new Material(s))
                .Do(m => m.mainTexture = SpriteSheet)
                .IfNull(() => { throw new ShaderNotFoundException(ShaderName); });
        }


        private PackedSprite CreateSprite(Material material)
        {
            if (material == null) throw new ArgumentNullException("material");

            var sprite = PackedSprite.Create("ScienceAlert.Button.Sprite", Vector3.zero);

            sprite.SetMaterial(material);
            sprite.renderer.sharedMaterial.mainTexture.filterMode = FilterMode.Point;
            sprite.Setup(ButtonWidth, ButtonHeight);
            sprite.SetFramerate(GuiConfiguration.Framerate);
            sprite.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);
            sprite.gameObject.layer = LayerMask.NameToLayer(EzGuiLayerName);

            // normal state
            var normal = new UVAnimation { name = Animations.Unlit.ToString(), loopCycles = 0, framerate = GuiConfiguration.Framerate };
            normal.BuildUVAnim(sprite.PixelCoordToUVCoord(9 * ButtonWidth, 8 * ButtonHeight), sprite.PixelSpaceToUVSpace(ButtonWidth, ButtonHeight), 1, 1, 1);

            // animated state
            var anim = new UVAnimation { name = Animations.Spinning.ToString(), loopCycles = -1, framerate = GuiConfiguration.Framerate };
            anim.BuildWrappedUVAnim(new Vector2(0, sprite.PixelCoordToUVCoord(0, ButtonHeight).y), sprite.PixelSpaceToUVSpace(ButtonWidth, ButtonHeight), 100);

            // lit but not animated state
            var lit = new UVAnimation {name = Animations.Lit.ToString(), loopCycles = 0, framerate = GuiConfiguration.Framerate};
            lit.BuildWrappedUVAnim(new Vector2(0, sprite.PixelCoordToUVCoord(0, ButtonHeight).y),
                sprite.PixelSpaceToUVSpace(ButtonWidth, ButtonHeight), 1);


            // add animations to button
            sprite.AddAnimation(normal);
            sprite.AddAnimation(anim);
            sprite.AddAnimation(lit);

            return sprite;
        }


        private void OnFalse()
        {
            Toggle.Dispatch(false);
        }


        private void OnTrue()
        {
            Toggle.Dispatch(true);
        }


        public void SetToggleState(bool tf)
        {
            if (tf)
                _button.Do(b => b.SetTrue(false));
            else _button.Do(b => b.SetFalse(false));
        }


        public void SetAnimationState(Animations anim)
        {
            _sprite.Do(spr => spr.PlayAnim(anim.ToString()));
        }
    }
}
