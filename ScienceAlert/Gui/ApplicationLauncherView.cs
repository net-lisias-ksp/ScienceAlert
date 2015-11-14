using System;
using System.Collections;
using ReeperCommon.Containers;
using strange.extensions.injector;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.Gui
{
    public class ApplicationLauncherView : View
    {
        [Inject(Keys.ApplicationLauncherSpriteSheet)] public Texture2D SpriteSheet { get; set; }
        [Inject] public IRoutineRunner CoroutineRunner { get; set; }
        [Inject] public IGuiSettings GuiSettings { get; set; }

        internal readonly Signal<bool> Toggle = new Signal<bool>();
        internal readonly Signal ButtonCreated = new Signal();

        private ApplicationLauncherButton _button;

        private const string EzGuiLayerName = "EzGUI_UI";
        private const string ShaderName = "Sprite/Vertex Colored";
        private const int ButtonWidth = 38;
        private const int ButtonHeight = 38;

        private const string UnlitAnimationName = "Unlit";
        private const string SpinAnimationName = "Spin";

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


            var sprite = CreateSprite(CreateMaterial());

            _button = ApplicationLauncher.Instance.AddModApplication(
                                                        OnTrue,
                                                        OnFalse,

                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                                                        sprite);

            yield return new WaitForEndOfFrame();   // the button won't respect toggle state immediately for some reason,
                                                    // so a slight delay is necessary while it finishes doing whatever internal setup

            ButtonCreated.Dispatch();
        }


        private Material CreateMaterial()
        {
            return Shader.Find(ShaderName).With(s => new Material(s)).Do(m => m.mainTexture = SpriteSheet);
        }


        private PackedSprite CreateSprite(Material material)
        {
            if (material == null) throw new ArgumentNullException("material");

            var sprite = PackedSprite.Create("ScienceAlert.Button.Sprite", Vector3.zero);

            sprite.SetMaterial(material);
            sprite.renderer.sharedMaterial.mainTexture.filterMode = FilterMode.Point;
            sprite.Setup(ButtonWidth, ButtonHeight);
            sprite.SetFramerate(GuiSettings.Framerate);
            sprite.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);
            sprite.gameObject.layer = LayerMask.NameToLayer(EzGuiLayerName);

            // normal state
            var normal = new UVAnimation { name = UnlitAnimationName, loopCycles = 0, framerate = GuiSettings.Framerate };
            normal.BuildUVAnim(sprite.PixelCoordToUVCoord(9 * ButtonWidth, 8 * ButtonHeight), sprite.PixelSpaceToUVSpace(ButtonWidth, ButtonHeight), 1, 1, 1);

            // animated state
            var anim = new UVAnimation { name = SpinAnimationName, loopCycles = -1, framerate = GuiSettings.Framerate };
            anim.BuildWrappedUVAnim(new Vector2(0, sprite.PixelCoordToUVCoord(0, ButtonHeight).y), sprite.PixelSpaceToUVSpace(ButtonWidth, ButtonHeight), 100);


            // add animations to button
            sprite.AddAnimation(normal);
            sprite.AddAnimation(anim);

            sprite.PlayAnim(UnlitAnimationName);

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
    }
}
