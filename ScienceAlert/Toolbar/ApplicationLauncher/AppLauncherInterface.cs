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
    /// </summary>
    class DrawableManners : MonoBehaviour
    {
        // goals:
        //      temporarily hide drawable on hover
        //      permanently hide drawable on click

        class ButtonWrapper
        {
            public ApplicationLauncherButton button;
            public RUIToggleButton.OnHover onHover;
            public RUIToggleButton.OnTrue onTrue;
            public RUIToggleButton.OnClickBtn onClick;

            public void HoverStub()
            {
                Log.Write("hoverstub");
                onHover();
            }

            public void TrueStub()
            {
                Log.Write("truestub");
                onTrue();
            }

            public void ClickStub(RUIToggleButton button)
            {
                Log.Write("clickstub");
                onClick(button);
            }
        }

        // --------------------------------------------------------------------
        //    Members
        // --------------------------------------------------------------------
        //List<ButtonWrapper> stockButtons = new List<ButtonWrapper>();
        List<ApplicationLauncherButton> stockButtons = new List<ApplicationLauncherButton>();

        CurrencyWidgetsApp currency;
        AppLauncherInterface button;

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
                
                //for (int i = 0; i < appl.childCount; ++i)
                //    if (appl.GetChild(i).GetComponent<ApplicationLauncherButton>() != null)
                //    {
                //        buttons.Add(appl.GetChild(i).GetComponent<ApplicationLauncherButton>());
                //        break;
                //    }

                //foreach (var b in buttons)
                //{
                //    var wrapped = new ButtonWrapper()
                //    {
                //        button = b,
                //        onHover = b.toggleButton.onHover,
                //        onTrue = b.toggleButton.onTrue,
                //        onClick = b.toggleButton.onClickBtn
                //    };

                //    b.toggleButton.onTrue = wrapped.TrueStub;
                //    b.toggleButton.onHover = wrapped.HoverStub;

                //    b.toggleButton.onClickBtn = wrapped.ClickStub;
                   
                //    stockButtons.Add(wrapped);
                //}

                // nope foreach (var b in appl.GetComponentsInChildren<RUIToggleButton>())
                // nope foreach (var b in appl.GetComponentsInChildren<UIButton>())
                // nope foreach (var b in appl.GetComponentsInChildren<UIListItemContainer>())
                // nope foreach (var b in appl.GetComponentsInChildren<BTButton>())
                foreach (var b in appl.GetComponentsInChildren<UIScrollList>()) // AH HA!!
                {
                    //b.AddValueChangedDelegate(delegate(IUIObject o) { Log.Write("victory"); });
                    //b.AddInputDelegate(delegate(ref POINTER_INFO ptr) { Log.Write("ptr delegtate"); });
                    b.AddValueChangedDelegate(StockButtonClick);
                }
            }

            // the currency button is a little different
            currency = GameObject.FindObjectOfType<CurrencyWidgetsApp>();
            currency.gameObject.PrintComponents();

            if (currency == null)
            {
                Log.Error("AppLauncherInterface: CurrencyWidgetsApp is null!");
            }
            else StartCoroutine(WaitAndModifyCurrencyWidget());
        }


        void OnDestroy()
        {
            
        }


        public void StockButtonClick(IUIObject o)
        {
            Log.Write("stock button click");
            button.Drawable = null;
        }


        System.Collections.IEnumerator WaitAndModifyCurrencyWidget()
        {
            while (!currency.widgetSpawner.Spawned)
                yield return 0;

            var button = currency.gameObject.transform.Find("anchor/WidgetHoverArea").GetComponent<UIButton>();
            button.AddInputDelegate(delegate(ref POINTER_INFO ptr) { Log.Write("widget hover area"); });
            button.AddValueChangedDelegate(delegate(IUIObject obj) { Log.Write("widgethoverarea changed"); });

            var button2 = currency.gameObject.transform.Find("anchor/hoverComponent").GetComponent<UIButton>();
            button2.AddInputDelegate(delegate(ref POINTER_INFO ptr) { Log.Write("hoverComponent area"); });
            button2.AddValueChangedDelegate(delegate(IUIObject obj) { Log.Write("hoverComponent changed"); });
        }

        void Update()
        {
            // seems to be always active. todo
            //if (currency.gameObject.transform.Find("anchor/WidgetHoverArea").gameObject.renderer.enabled)
            //    Log.Write("WidgetHoverarea active!");
        }



        public void OnButton(IUIObject obj)
        {
            Log.Write("DrawableManners: OnButton");
        }



        public bool ShouldHide
        {
            get
            {
                return false;
            }
        }



        //public bool ShouldClose
        //{
        //    get
        //    {
        //        return false;
        //    }
        //}
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

        private ApplicationLauncherButton button;
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
            sprite.SetFramerate(Settings.Instance.StarFlaskFrameRate);
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

            sprite.PlayAnim("Unlit");
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
            gameObject.AddComponent<DrawableManners>();

            ApplicationLauncher.Instance.gameObject.PrintComponents();

            //// having the drawable window overlapping stock buttons is extremely
            //// annoying. We'll temporarily hide the drawable whenever one is
            //// displaying. Of course, that means we need to actually get the 
            //// stock button list
            //var appButtonList = ApplicationLauncher.Instance.gameObject.transform.Find("anchor/List");
            //if (appButtonList == null)
            //{
            //    Log.Error("AppLauncherInterface: ApplicationLauncher Application list not found!");
            //}
            //else
            //{
            //    Log.Write("normal buttons");
            //    foreach (var b in ApplicationLauncher.Instance.gameObject.transform.Find("anchor/List").GetComponentsInChildren<ApplicationLauncherButton>().ToList())
            //    {
            //        //appButtonList.GetComponent<UIScrollList>().AddValueChangedDelegate(StockButtonClick);
            //        //appButtonList.GetComponent<UIScrollList>().AddInputDelegate(TestInput);
            //        b.container.AddInputDelegate(TestInput);
                   
            //    }
            //}

            //Log.Write("missing button");

            //for (int i = 0; i < ApplicationLauncher.Instance.gameObject.transform.childCount; ++i)
            //{
            //    var c = ApplicationLauncher.Instance.gameObject.transform.GetChild(i).GetComponent<ApplicationLauncherButton>();
            //    if (c != null)
            //    {
            //        c.container.AddInputDelegate(TestInput);
            //        break;
            //    }
            //}
            //foreach (var b in ApplicationLauncher.Instance.gameObject.GetComponentsInChildren<ApplicationLauncherButton>().ToList())
            //{

            //    //appButtonList.GetComponent<UIScrollList>().AddValueChangedDelegate(StockButtonClick);
            //    //appButtonList.GetComponent<UIScrollList>().AddInputDelegate(TestInput);
                

            //}

            //var obj = GameObject.FindObjectOfType<CurrencyWidget>();
            //if (obj != null) Log.Write("found currency widget: {0}", obj.name);

            ////obj.gameObject.PrintComponents();

            //var obj2 = GameObject.FindObjectOfType<CurrencyWidgetsApp>();
            //if (obj2 != null) Log.Write("found curr widget app: {0}", obj2.name);

            //obj2.gameObject.PrintComponents();

        }

        void OnDestroy()
        {
            if (button != null)
            {
                Log.Verbose("Removing ApplicationLauncherButton");
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }

            //var appButtonList = ApplicationLauncher.Instance.gameObject.transform.Find("anchor/List");
            //if (appButtonList != null)
            //    appButtonList.GetComponent<UIScrollList>().RemoveValueChangedDelegate(StockButtonClick);

            if (gameObject.GetComponent<DrawableManners>()) GameObject.Destroy(gameObject.GetComponent<DrawableManners>());
        }


        Vector2 CalculateDrawablePosition(Vector2 size)
        {
            // Rect.xy == top left
            Rect rect = new Rect(0, 0, size.x, size.y);

            rect.x = Screen.width - size.x;
            rect.y = Screen.height - button.sprite.RenderCamera.WorldToScreenPoint(button.transform.position).y + 38f * 1.25f;
            rect = KSPUtil.ClampRectToScreen(rect);

            var transformedY = button.sprite.RenderCamera.ScreenToWorldPoint(new Vector3(rect.x, Screen.height - rect.y /* inverted remember */)).y;// +38f * 1.25f;
            var transformedX = button.sprite.RenderCamera.ScreenToWorldPoint(new Vector3(rect.x, 0f, 0f)).x;
 
            return new Vector2(rect.x, rect.y);
        }


        #region GUI events

        public void TestInput(ref POINTER_INFO ptr)
        {
            Log.Write("TestInput");
            Log.Write("target = {0}", ptr.targetObj.name);

            //if ((UIListItemContainer)ptr.targetObj == button.container) Log.Write("That's us!");
           
            if (ptr.evt == POINTER_INFO.INPUT_EVENT.MOVE || ptr.evt == POINTER_INFO.INPUT_EVENT.PRESS)
                Drawable = null;
        }

        public void StockButtonClick(IUIObject obj)
        {
            Drawable = null;    
        }

        public void OnGUI()
        {
            if (drawable == null) return;
            if (!button.gameObject.activeSelf) return;

            // don't draw anything if one of the stock buttons is lit. We'll
            // just have to hope other mod buttons handle things reasonably since
            // I can't predict their behaviour
            //if (appButtons.Any(b => b.State == RUIToggleButton.ButtonState.TRUE || b.toggleButton.IsHovering)) return;

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
                }
            }
        }

        #endregion
    }
}
