/******************************************************************************
                   Science Alert for Kerbal Space Program                    
 ******************************************************************************
    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *****************************************************************************/
#define GENERATE_ATLAS
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using ReeperCommon;
using ImprovedAddonLoader;
#if DEBUG
using System;
#endif

namespace ScienceAlert.Toolbar
{
    class AppLauncherInterface : MonoBehaviour, IToolbar
    {
        // --------------------------------------------------------------------
        //    Members
        // --------------------------------------------------------------------
        public event ToolbarClickHandler OnClick;

        private IDrawable drawable;
        private bool movedDrawable = false;
        private Vector2 drawablePosition = Vector2.zero;

        private ApplicationLauncherButton button;
        private PackedSprite sprite; // animations: Spin, Unlit
        private UIButton background; // prevent clickthrough ;)

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/
        void Start() { StartCoroutine(WaitOnAppLauncher()); }


        /// <summary>
        /// Waits for the Application Launcher before beginning initialization
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator WaitOnAppLauncher()
        {
            Log.Debug("Waiting on AppLauncher...");

            while (!ApplicationLauncher.Ready)
                yield return null;

            Log.Verbose("Retrieving animation sheet.");
            var sheet = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.sheet_app.png", true, false);
            if (sheet == null)
            {
                Log.Error("Failed to locate embedded app sheet texture!");

                // well ... without it we're sunk. Something is better than
                // nothing. We can't let the stock behaviour fail
                sheet = new Texture2D(512, 512, TextureFormat.ARGB32, false);
                sheet.SetPixels32(Enumerable.Repeat((Color32)Color.clear, 512 * 512).ToArray());
                sheet.Apply();
            }

            sprite = PackedSprite.Create("ScienceAlert.Button.Sprite", Vector3.zero);
            sprite.SetMaterial(new Material(Shader.Find("Sprite/Vertex Colored")) { mainTexture = sheet });
            sprite.renderer.sharedMaterial.mainTexture.filterMode = FilterMode.Point;
            sprite.Setup(38f, 38f);
            sprite.SetFramerate(24f);
            sprite.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);
            sprite.gameObject.layer = LayerMask.NameToLayer("EzGUI_UI");

            Log.Write("setup sprite");

            // normal state
            UVAnimation normal = new UVAnimation() { name = "Unlit", loopCycles = 0, framerate = 24f };
            normal.BuildUVAnim(sprite.PixelCoordToUVCoord(9 * 38, 8 * 38), sprite.PixelSpaceToUVSpace(38, 38), 1, 1, 1);

            // animated state
            UVAnimation anim = new UVAnimation() { name = "Spin", loopCycles = -1, framerate = 24f };
            anim.BuildWrappedUVAnim(new Vector2(0, 1f - sprite.PixelSpaceToUVSpace(38, 38).y), sprite.PixelSpaceToUVSpace(38, 38), 100);

            // add animations to button
            sprite.AddAnimation(normal);
            sprite.AddAnimation(anim);


            Log.Debug("Creating mod button...");
            button = ApplicationLauncher.Instance.AddModApplication(
                                                        OnToggleOn,
                                                        OnToggleOff,
                                                        OnHover,
                                                        OnHover,
                                                        OnEnable,
                                                        OnDisable,
                                                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                                                        sprite);

            sprite.PlayAnim("Unlit");
            if (sprite.plane != SpriteRoot.SPRITE_PLANE.XY) Log.Error("sprite plane isn't xy!");

            // create invisible background button
            background = GuiUtil.CreateButton("AppLauncher.ScienceAlert.Background", Vector2.zero);
            background.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);

            background.renderCamera = sprite.renderCamera;
            var bgTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
            bgTex.GenerateRandom();

            Material bgMat = new Material(Shader.Find("Sprite/Vertex Colored")) { mainTexture = bgTex };
            background.Setup(128f, 128f, bgMat);

            background.gameObject.SetActive(true);
            background.Hide(false);
            background.SetUVs(new Rect(0f, 0f, 1f, 1f));

            background.InitUVs();
            background.SetPixelToUV(background.renderer.sharedMaterial.mainTexture);
            background.UpdateCamera();

            background.AddValueChangedDelegate(delegate(IUIObject obj) { Log.Write("ezbutton click!"); });


            Transform t = button.transform;

            while (t.parent != null) t = t.parent;

            Log.Write("Dumping components");
            t.gameObject.PrintComponents();

            Log.Debug("Finished creating mod button");

            foreach (var c in Camera.allCameras)
                Log.Write("cam: {0}", c.name);


            // adjust background position
                //var screenPos = background.RenderCamera.WorldToScreenPoint(button

                // wasn't visible
                //background.transform.position = button.transform.position + new Vector3(0f, 100f, 0f);

                // visible, but skewed right and half off screen
                //background.transform.position = button.transform.position - new Vector3(0f, 100f, 0f);

                // top-left position is at upper-right of icon
                //background.transform.position = button.transform.position;

                // y looks right, but half scaled off right of screen
                // background button position: (424.5, 385.5, 1035.0)
                //background.transform.position = button.sprite.transform.position;

                // sort of right, but y is off by a few pixels and x is off by width
                // whole button does fit on screen though
                // background button position: (383.5, 344.5, 1045.0)
                // background.transform.position = button.GetAnchor();

                // gets local space center of sprite apparently
                // background button position: (19.0, -19.0, 0.0)
                //background.transform.position = button.sprite.GetCenterPoint();

                // y is right but x is off by about width in + dir
                // background button position: (399.5, 366.5, 1045.0)
                //background.transform.position = button.transform.position + button.sprite.GetCenterPoint();

                // about half a size too much on on both dir. Y is too far up, x not far enough left
                // (361.5, 404.5, 1045.0)
                //background.transform.position = button.transform.position - button.sprite.GetCenterPoint();

                // correct! top-left of button. It really makes no sense when you think about it though
                //background.transform.position = button.transform.position - button.sprite.GetCenterPoint() + new Vector3(-38f * 0.5f, -38f * 0.5f);
            background.transform.position = button.transform.position - new Vector3(38f, 38f * 1.0f);

                // perfect!
                //background.transform.position = button.transform.position - new Vector3(38f, 38f, 0f);
                //background.transform.position = background.transform.position - new Vector3(0f, 38f, 0f);
            //background.transform.position = background.transform.position - new Vector3(0f, 38f, 0f);
                Log.Write("background button position: {0}", background.transform.position);
                Log.Write("offset = {0}", button.sprite.offset);
                Log.Write("center = {0}", button.sprite.GetCenterPoint());
        }

        void OnDestroy()
        {
            if (button != null)
            {
                Log.Verbose("Removing ApplicationLauncherButton");
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }
        }


        Vector2 CalculateDrawablePosition(Vector2 size)
        {
            // Rect.xy == top left
            Rect rect = new Rect(0, 0, size.x, size.y);

            rect.x = Screen.width - size.x;
            rect.y = Screen.height - button.sprite.RenderCamera.WorldToScreenPoint(button.transform.position).y + 38f * 1.25f;
            var transformedY = button.sprite.RenderCamera.ScreenToWorldPoint(new Vector3(rect.x, Screen.height - rect.y /* inverted remember */)).y;// +38f * 1.25f;
            var transformedX = button.sprite.RenderCamera.ScreenToWorldPoint(new Vector3(rect.x, 0f, 0f)).x;


            rect = KSPUtil.ClampRectToScreen(rect);

 
            background.transform.position = new Vector3(transformedX, transformedY, button.transform.position.z);


            return new Vector2(rect.x, rect.y);
        }


        #region GUI events

        public void OnGUI()
        {

            

            if (drawable == null) return;

            // if drawables were switched, there could be one frame
            // where the new drawable uses the old position which is
            // ugly (jumps). As long as we know when the drawable changed, we 
            // can do a temp render to get the new dimensions
            //
            // We do this here rather than when drawable switches because
            // if the drawable implementor uses any method that requires
            // unity to be in OnGUI, it'd break
            if (!movedDrawable)
            {
                var old = RenderTexture.active;
                RenderTexture.active = RenderTexture.GetTemporary(Screen.width, Screen.height);
                movedDrawable = true;
                OnGUI();
                RenderTexture.ReleaseTemporary(RenderTexture.active);
                RenderTexture.active = old;
            }

            var dimensions = drawable.Draw(drawablePosition);
            drawablePosition = CalculateDrawablePosition(dimensions);
        }


        public void OnToggleOff()
        {
            Log.Write("ToggleOff");
        }

        public void OnHover()
        {
            Log.Write("Hover");
        }

        public void OnEnable()
        {
            Log.Write("Enable");
        }

        public void OnDisable()
        {
            Log.Write("Disable");
        }

        public void OnToggleOn()
        {
            Log.Write("ToggleOn");

            int button = 0;

            if (Input.GetMouseButtonUp(0))
            {
                Log.Debug("AppLauncher: left click");
            }
            else if (Input.GetMouseButtonUp(1))
            {
                Log.Debug("AppLauncher: right click");
                button = 1;
            }
            else if (Input.GetMouseButtonUp(2))
            {
                Log.Debug("AppLauncher: middle click");
                button = 2;
            }

            this.button.SetFalse(false);

            OnClick(new ClickInfo() { button = button });
        }

        #endregion


        #region animation related

        /// <summary>
        /// Plays spinning star flask animation
        /// </summary>
        public void PlayAnimation()
        {
            sprite.PlayAnim("Spin");
        }



        /// <summary>
        /// Pause the star flask animation (but stay lit)
        /// </summary>
        public void StopAnimation()
        {
            sprite.PauseAnim();
        }



        /// <summary>
        /// Normal flask texture
        /// </summary>
        public void SetUnlit()
        {
            sprite.PlayAnim("Unlit");
        }



        /// <summary>
        /// Set to star flask texture, but without animating
        /// </summary>
        public void SetLit()
        {
            if (sprite.GetCurAnim().name != "Spin")
            {
                sprite.SetFrame("Spin", 0);
            }
            else sprite.PauseAnim();
        }

        #endregion


        #region properties

        // Unused for AppLauncher (for now)
        public bool Important { get { return false; } set { } }



        /// <summary>
        /// Returns true if the star flask spin animation is playing
        /// </summary>
        public bool IsAnimating
        {
            get
            {
                return sprite.IsAnimating() && sprite.GetCurAnim().name == "Spin";
            }
        }



        /// <summary>
        /// Normal flask texture, no star
        /// </summary>
        public bool IsNormal
        {
            get
            {
                return sprite.GetCurAnim().name == "Unlit";
            }
        }



        /// <summary>
        /// Star flask texture, no animation
        /// </summary>
        public bool IsLit
        {
            get
            {
                return sprite.GetCurAnim().name == "Spin" && !sprite.IsAnimating();
            }
        }



        /// <summary>
        /// Windows will set this to something when they want to be drawn
        /// </summary>
        public IDrawable Drawable
        {
            get
            {
                return drawable;
            }

            set
            {
                if (value != drawable)
                {
                    drawable = value;
                    movedDrawable = false;
                }
            }
        }

        #endregion
    }
}
