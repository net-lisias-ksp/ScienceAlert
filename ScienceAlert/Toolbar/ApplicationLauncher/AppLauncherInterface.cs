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
    /// <summary>
    /// Unfortunately there's some peculiar, potentially annoying behaviour
    /// with the ApplicationLauncher. Specifically, there are cases where
    /// mousing over a stock button would cause it to overlap with the
    /// Drawable. The goal of this object is to make the drawable behave
    /// nicely by:
    /// 
    /// - temporarily hiding the drawable if a stock button is hovered
    /// - temporarily hiding the drawable if the currency widget is visible
    /// - closing the drawable if a stock button is clicked
    /// 
    /// Note: this whole thing is essentially a bugfix for the issue 
    /// described at http://forum.kerbalspaceprogram.com/threads/86682-Appilcation-Launcher-and-Mods?p=1280014#post1280014
    /// </summary>
    class DrawableManners : MonoBehaviour
    {

        // --------------------------------------------------------------------
        //    Members
        // --------------------------------------------------------------------
        List<ApplicationLauncherButton> stockButtons = new List<ApplicationLauncherButton>();
        List<GameObject> widgets = new List<GameObject>();

        CurrencyWidgetsApp currency;
        MessageSystem messager;
        ResourceDisplay resources;

        AppLauncherInterface button;
        System.Collections.IEnumerator waitRoutine;
        bool mouseHover = false;


/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        void Start()
        {
            Log.Debug("DrawableManners start");

            button = GetComponent<AppLauncherInterface>();

            var appl = ApplicationLauncher.Instance.gameObject.transform;
            var holder = appl.Find("anchor/List");

            if (holder == null)
            {
                Log.Error("AppLauncherInterface.DrawableManners: stock buttons not found!");
            }
            else
            {
                stockButtons = holder.GetComponentsInChildren<ApplicationLauncherButton>().ToList();

                // nope foreach (var b in appl.GetComponentsInChildren<RUIToggleButton>())
                // nope foreach (var b in appl.GetComponentsInChildren<UIButton>())
                // nope foreach (var b in appl.GetComponentsInChildren<UIListItemContainer>())
                // nope foreach (var b in appl.GetComponentsInChildren<BTButton>())

                // Looking for ApplicationLauncherButtons would be the obvious route but for
                // some reason the currency widget doesn't seem to be one. We can get click
                // events this way instead
                foreach (var b in holder.GetComponentsInChildren<UIScrollList>()) // AH HA!!
                    b.AddValueChangedDelegate(StockButtonClick);
                
            }

            // the currency button is a little different
            currency = GameObject.FindObjectOfType<CurrencyWidgetsApp>();
            currency.gameObject.PrintComponents();
            
            StartCoroutine(WaitAndModifyCurrencyWidget());
            StartCoroutine(WaitAndModifyMessageWidget());
            StartCoroutine(WaitAndModifyResourceWidget());
        }



        void OnDestroy()
        {
            // remove hooks from stock buttons
            var holder = ApplicationLauncher.Instance.gameObject.transform.Find("anchor/List");

            foreach (var b in holder.GetComponentsInChildren<UIScrollList>())
            {
                try
                {
                    b.RemoveValueChangedDelegate(StockButtonClick);
                }
                catch (Exception e)
                {
                    Log.Error("AppLauncherInterface: Exception while remove stock app button delegates: {0}", e);
                }
            }
        }



        /// <summary>
        /// Close the drawable if the player clicks on a stock button
        /// </summary>
        /// <param name="o"></param>
        public void StockButtonClick(IUIObject o)
        {
            Log.Debug("stock button click");
            button.Drawable = null;
        }



        /// <summary>
        /// The CurrencyWidgetsApp button is a strange one. It acts little
        /// differently than the others so we need a specific solution to
        /// avoid overlapping the experiment drawable on it if it's visible.
        /// 
        /// To begin with, we need to wait for the NestedPrefabSpawner to run.
        /// After that we can add delegates to notify us
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator WaitAndModifyCurrencyWidget()
        {
            while (!currency.widgetSpawner.Spawned)
                yield return 0;

#if DEBUG
            currency.gameObject.PrintComponents();
#endif

            var t = currency.gameObject.transform;

            // we also need to know if the widget is actually being displayed
            widgets.Add(t.Find("anchor/FundsWidget/Frame").gameObject);
            widgets.Add(t.Find("anchor/RepWidget/Frame").gameObject);
            widgets.Add(t.Find("anchor/SciWidget/Frame").gameObject);
        }



        /// <summary>
        /// Wait for the messager to exist and then locate a child object
        /// we can poll to see if the panel is being displayed
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator WaitAndModifyMessageWidget()
        {
            Log.Debug("Waiting for message widget...");

            while (MessageSystem.Instance == null)
                yield return 0;

            messager = MessageSystem.Instance;
            
            widgets.Add(messager.gameObject.transform.Find("listArea/bg").gameObject);
            Log.Debug("AppLauncherInterface: Found message widget background");
        }



        /// <summary>
        /// Same idea as the messager widget
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator WaitAndModifyResourceWidget()
        {
            while (ResourceDisplay.Instance == null)
                yield return 0;

            resources = ResourceDisplay.Instance;
            widgets.Add(resources.transform.Find("header").gameObject);

#if DEBUG
            resources.gameObject.PrintComponents();
#endif
        }



        /// <summary>
        /// wait for user to move mouse off of the hover area
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator TemporarilyHideDrawable()
        {
            IDrawable drawable = button.Drawable;

            if (drawable == null) yield break;

            Log.Debug("DrawableManners: Temporarily hiding drawable");
            button.Drawable = null;

            while (mouseHover && button.Drawable == null)
                yield return 0;

            // check to see if a new drawable was added
            if (button.Drawable != null)
            {
                Log.Debug("DrawableManners: Did not restore drawable; is too old");
                yield break;
            }
            // restore drawable
            button.Drawable = drawable;
            Log.Debug("DrawableManners: Restored drawable");
        }



        /// <summary>
        /// Why update the coroutine manually? I like this way better than
        /// using a flag to avoid duplicate coroutines from being created
        /// </summary>
        void Update()
        {
            if (waitRoutine != null)
                if (!waitRoutine.MoveNext())
                    waitRoutine = null;
        }



        /// <summary>
        /// If the ScienceAlert button is pressed, it should supercede any
        /// open widgets
        /// </summary>
        public void CloseOpenWidgets()
        {
            resources.HideResourceList();
            messager.Hide();
            currency.widgetSpawner.gameObject.SetActive(false);

            // note: doesn't get currency widget button state =\
            foreach (var b in stockButtons)
                if (b.toggleButton.State == RUIToggleButton.ButtonState.TRUE)
                    b.toggleButton.SetFalse();
        }



        /// <summary>
        /// ExperimentManager will query this to decide whether the drawable
        /// should be visible or not. 
        /// </summary>
        public bool ShouldHide
        {
            get
            {
                return waitRoutine != null ||
                        stockButtons.Any(b => b.toggleButton.IsHovering || b.toggleButton.State == RUIToggleButton.ButtonState.TRUE)
                        || widgets.Any(go => go.activeInHierarchy);
            }
        }
    }


    class AppLauncherInterface : MonoBehaviour, IToolbar
    {

        // --------------------------------------------------------------------
        //    Members
        // --------------------------------------------------------------------
        public event ToolbarClickHandler OnClick;

        private IDrawable drawable;
        private bool movedDrawable = false;
        private Vector2 drawablePosition = Vector2.zero;

        private DrawableManners manners;

        public ApplicationLauncherButton button;
        private PackedSprite sprite; // animations: Spin, Unlit


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
            var sheet = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.sheet_app.png", false, false);
            
            if (sheet == null)
            {
                Log.Error("Failed to locate embedded app sheet texture!");

                // well ... without it we're sunk. Something is better than
                // nothing. We can't let the stock behaviour fail
                Log.Warning("Creating dummy sprite texture");

                sheet = new Texture2D(38, 38, TextureFormat.ARGB32, false);
                sheet.SetPixels32(Enumerable.Repeat((Color32)Color.clear, 38 * 38).ToArray());
                sheet.Apply();
            }

            Log.Verbose("Setting up sprite");
            sprite = PackedSprite.Create("ScienceAlert.Button.Sprite", Vector3.zero);
            sprite.SetMaterial(new Material(Shader.Find("Sprite/Vertex Colored")) { mainTexture = sheet });
            sprite.renderer.sharedMaterial.mainTexture.filterMode = FilterMode.Point;
            sprite.Setup(38f, 38f);
            sprite.SetFramerate(Settings.Instance.StarFlaskFrameRate);
            sprite.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);
            sprite.gameObject.layer = LayerMask.NameToLayer("EzGUI_UI");

            

            // normal state
            Log.Verbose("Setting up normal animation");
            UVAnimation normal = new UVAnimation() { name = "Unlit", loopCycles = 0, framerate = 24f };
            normal.BuildUVAnim(sprite.PixelCoordToUVCoord(9 * 38, 8 * 38), sprite.PixelSpaceToUVSpace(38, 38), 1, 1, 1);

            // animated state
            Log.Verbose("Setting up star flask animation");
            UVAnimation anim = new UVAnimation() { name = "Spin", loopCycles = -1, framerate = 24f };
            anim.BuildWrappedUVAnim(new Vector2(0, sprite.PixelCoordToUVCoord(0, 38).y), sprite.PixelSpaceToUVSpace(38, 38), 100);


            // add animations to button
            sprite.AddAnimation(normal);
            sprite.AddAnimation(anim);

            sprite.PlayAnim("Unlit");

            Log.Verbose("Creating mod button...");
            button = ApplicationLauncher.Instance.AddModApplication(
                                                        OnToggleOn,
                                                        OnToggleOff,
                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                                                        sprite);

            Log.Verbose("Creating DrawableManners...");
            manners = gameObject.AddComponent<DrawableManners>();

 
            Log.Verbose("AppLauncherInterface ready");
            ApplicationLauncher.Instance.gameObject.PrintComponents();
        }



        void OnDestroy()
        {
            if (button != null)
            {
                Log.Verbose("Removing ApplicationLauncherButton");
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }

            if (manners != null) Component.Destroy(manners);
        }



        Vector2 CalculateDrawablePosition(Vector2 size)
        {
            Log.Write("Drawable size = {0}", size);

            // Rect.xy == top left
            Rect rect = new Rect(0, 0, size.x, size.y);

            rect.x = Screen.width - size.x;
            rect.y = Screen.height - button.sprite.RenderCamera.WorldToScreenPoint(button.transform.position).y + 38f * 1.25f;
            rect = KSPUtil.ClampRectToScreen(rect);

            Log.Write("clamped: {0}", rect);

            var transformedY = button.sprite.RenderCamera.ScreenToWorldPoint(new Vector3(rect.x, Screen.height - rect.y /* inverted remember */)).y;
            var transformedX = button.sprite.RenderCamera.ScreenToWorldPoint(new Vector3(rect.x, 0f, 0f)).x;
 
            return new Vector2(rect.x, rect.y);
        }


        #region GUI events



        public void OnGUI()
        {
            if (drawable == null) return;
            if (!button.gameObject.activeSelf) return;
            if (manners.ShouldHide) return;

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
                Log.Debug("Dummy render to position drawable");

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


        /// <summary>
        /// close open stock widgets and let our listeners know about
        /// the click
        /// </summary>
        public void OnToggleOn()
        {
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

            manners.CloseOpenWidgets();

            OnClick(new ClickInfo() { button = button });
        }



        public void OnToggleOff()
        {
            Drawable = null;
        }

        #endregion


        #region animation related

        /// <summary>
        /// Plays spinning star flask animation
        /// </summary>
        public void PlayAnimation()
        {
            if (Settings.Instance.FlaskAnimationEnabled)
            {
                sprite.PlayAnim("Spin");
            }
            else SetLit();
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
                sprite.PauseAnim();
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

                    if (button.State == RUIToggleButton.ButtonState.TRUE && value == null)
                        button.SetFalse(false);

                    if (button.State == RUIToggleButton.ButtonState.FALSE && value != null)
                        button.SetTrue(false);
                }
            }
        }

        #endregion
    }
}
