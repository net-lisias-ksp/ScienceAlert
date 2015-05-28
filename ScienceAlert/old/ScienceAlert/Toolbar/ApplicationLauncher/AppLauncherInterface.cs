///******************************************************************************
//                   Science Alert for Kerbal Space Program                    
// ******************************************************************************
//    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// *****************************************************************************/
//using UnityEngine;
//using System.Linq;
//using System.Collections.Generic;
//using ReeperCommon;
//using System;

//namespace ScienceAlert.Toolbar
//{

//    /// <summary>
//    /// A concrete object hidden behind IToolbar which handles (almost) all
//    /// interactions with the ApplicationLauncher. The other important bit
//    /// is DrawableManners which is a workaround for some oversights/bugs
//    /// in the current implementation of AppLauncher
//    /// </summary>
//    class AppLauncherInterface : MonoBehaviour, IToolbar
//    {
//        // --------------------------------------------------------------------
//        //    Members
//        // --------------------------------------------------------------------
//        public event ToolbarClickHandler OnClick;

//        private IDrawable drawable;
//        private Vector2 drawablePosition = new Vector2(Screen.width, 0);

//        public ApplicationLauncherButton button;
//        private PackedSprite sprite; // animations: Spin, Unlit

        

///******************************************************************************
// *                    Implementation Details
// ******************************************************************************/
//        void Awake() { StartCoroutine(WaitOnAppLauncher()); }


//        /// <summary>
//        /// Waits for the Application Launcher before beginning initialization
//        /// </summary>
//        /// <returns></returns>
//        System.Collections.IEnumerator WaitOnAppLauncher()
//        {
//            Log.Debug("Waiting on AppLauncher...");

//            while (!ApplicationLauncher.Ready)
//                yield return null;

//            Log.Verbose("Retrieving animation sheet.");
//            var sheet = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.sheet_app.png", false, false);
            
//            if (sheet == null)
//            {
//                Log.Error("Failed to locate embedded app sheet texture!");

//                // well ... without it we're sunk. Something is better than
//                // nothing. We can't let the stock behaviour fail
//                Log.Warning("Creating dummy sprite texture");

//                sheet = new Texture2D(38, 38, TextureFormat.ARGB32, false);
//                sheet.SetPixels32(Enumerable.Repeat((Color32)Color.clear, 38 * 38).ToArray());
//                sheet.Apply();
//            }

//            Log.Verbose("Setting up sprite");
//            sprite = PackedSprite.Create("ScienceAlert.Button.Sprite", Vector3.zero);
//            sprite.SetMaterial(new Material(Shader.Find("Sprite/Vertex Colored")) { mainTexture = sheet });
//            sprite.renderer.sharedMaterial.mainTexture.filterMode = FilterMode.Point;
//            sprite.Setup(38f, 38f);
//            sprite.SetFramerate(Settings.Instance.StarFlaskFrameRate);
//            sprite.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);
//            sprite.gameObject.layer = LayerMask.NameToLayer("EzGUI_UI");

            

//            // normal state
//            Log.Verbose("Setting up normal animation");
//            UVAnimation normal = new UVAnimation() { name = "Unlit", loopCycles = 0, framerate = Settings.Instance.StarFlaskFrameRate };
//            normal.BuildUVAnim(sprite.PixelCoordToUVCoord(9 * 38, 8 * 38), sprite.PixelSpaceToUVSpace(38, 38), 1, 1, 1);

//            // animated state
//            Log.Verbose("Setting up star flask animation");
//            UVAnimation anim = new UVAnimation() { name = "Spin", loopCycles = -1, framerate = Settings.Instance.StarFlaskFrameRate };
//            anim.BuildWrappedUVAnim(new Vector2(0, sprite.PixelCoordToUVCoord(0, 38).y), sprite.PixelSpaceToUVSpace(38, 38), 100);


//            // add animations to button
//            sprite.AddAnimation(normal);
//            sprite.AddAnimation(anim);

//            sprite.PlayAnim("Unlit");

//            Log.Verbose("Creating mod button...");
//            button = ApplicationLauncher.Instance.AddModApplication(
//                                                        OnToggle,
//                                                        OnToggle,
   
//                                                        () => { },
//                                                        () => { },
//                                                        () => { },
//                                                        () => { },
//                                                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
//                                                        sprite);

//            GameEvents.onGUIApplicationLauncherUnreadifying.Add(AppLauncherUnreadifying);

//            Log.Debug("AppLauncherInterface: Button transform = {0}", button.transform.position.ToString());
 
//            Log.Verbose("AppLauncherInterface ready");
//        }



//        void OnDestroy()
//        {
//            if (button != null)
//            {
//                Log.Verbose("Removing ApplicationLauncherButton");
//                ApplicationLauncher.Instance.RemoveModApplication(button);
//            }

//            GameEvents.onGUIApplicationLauncherUnreadifying.Remove(AppLauncherUnreadifying);
//        }



//        Vector2 CalculateDrawablePosition(Vector2 size)
//        {
//            // Rect.xy == top left
//            Rect rect = new Rect(0, 0, size.x, size.y);

//            rect.x = Screen.width - size.x;
//            rect.y = Screen.height - button.sprite.RenderCamera.WorldToScreenPoint(button.transform.position).y + 38f * 1.25f;
//            rect = KSPUtil.ClampRectToScreen(rect);

//            var transformedY = button.sprite.RenderCamera.ScreenToWorldPoint(new Vector3(rect.x, Screen.height - rect.y /* inverted remember */)).y;
//            var transformedX = button.sprite.RenderCamera.ScreenToWorldPoint(new Vector3(rect.x, 0f, 0f)).x;
 
//            return new Vector2(rect.x, rect.y);
//        }

//        #region GameEvents

//        private void AppLauncherUnreadifying(GameScenes scene)
//        {
//            Log.Debug("AppLauncherInterface: AppLauncherUnreadifying");

//            if (button != null && ApplicationLauncher.Instance != null)
//            {
//                Log.Verbose("Removing ApplicationLauncherButton");
//                ApplicationLauncher.Instance.RemoveModApplication(button);
//            }
//        }

//        #endregion

//        #region GUI events



//        public void OnGUI()
//        {
//            if (drawable == null) return;
//            if (!button.gameObject.activeSelf) return;

//            var dimensions = drawable.Draw(drawablePosition);
//            drawablePosition = CalculateDrawablePosition(dimensions);
//        }


//        // This is the basis of getting middle-mouse click to work on a UIButton. As you can see it's not very pretty
//        //public void Update()
//        //{
//        //    // if (Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, layerMask))
//        //    var camSettings = UIManager.instance.uiCameras[0];

//        //    RaycastHit hitInfo;

//        //    if (Physics.Raycast(camSettings.camera.ScreenPointToRay(Input.mousePosition), out hitInfo, camSettings.rayDepth, camSettings.mask))
//        //    {
//        //        Log.Warning("Raycasthit: {0}", hitInfo.collider.gameObject.name);
//        //    }
//        //}



//        /// <summary>
//        /// close open stock widgets and let our listeners know about
//        /// the click
//        /// </summary>
//        public void OnToggle()
//        {
//            Log.Debug("OnToggle");

//            int button = 0;

//            if (Input.GetMouseButtonUp(0))
//            {
//                Log.Debug("AppLauncher: left click");
//            }
//            else if (Input.GetMouseButtonUp(1))
//            {
//                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
//                {
//                    Log.Debug("AppLauncher: right click + alt");
//                    button = 2;
//                }
//                else
//                {
//                    Log.Debug("AppLauncher: right click");
//                    button = 1;
//                }
//            }
//            else if (Input.GetMouseButtonUp(2)) // note: middle mouse isn't registered as a tap by ezgui so this will never be called
//                                                // solution is to do the raycast ourselves which does work but it's messy
//            {
//                Log.Debug("AppLauncher: middle click");
//                button = 2;
//            }

 
//            OnClick(new ClickInfo() { button = button, used = false });
//        }



//        #endregion


//        #region animation related

//        /// <summary>
//        /// Plays spinning star flask animation
//        /// </summary>
//        public void PlayAnimation()
//        {
//            if (Settings.Instance.FlaskAnimationEnabled)
//            {
//                sprite.PlayAnim("Spin");
//            }
//            else SetLit();
//        }



//        /// <summary>
//        /// Pause the star flask animation (but stay lit)
//        /// </summary>
//        public void StopAnimation()
//        {
//            sprite.PauseAnim();
//        }



//        /// <summary>
//        /// Normal flask texture
//        /// </summary>
//        public void SetUnlit()
//        {
//            sprite.PlayAnim("Unlit");
//        }



//        /// <summary>
//        /// Set to star flask texture, but without animating
//        /// </summary>
//        public void SetLit()
//        {
//            if (sprite.GetCurAnim().name != "Spin")
//            {
//                sprite.SetFrame("Spin", 0);
//                sprite.PauseAnim();
//            }
//            else sprite.PauseAnim();
//        }

//        #endregion


//        #region properties

//        // Unused for AppLauncher (for now)
//        public bool Important { get { return false; } set { } }



//        /// <summary>
//        /// Returns true if the star flask spin animation is playing
//        /// </summary>
//        public bool IsAnimating
//        {
//            get
//            {
//                return sprite.IsAnimating() && sprite.GetCurAnim().name == "Spin";
//            }
//        }



//        /// <summary>
//        /// Normal flask texture, no star
//        /// </summary>
//        public bool IsNormal
//        {
//            get
//            {
//                return sprite.GetCurAnim().name == "Unlit";
//            }
//        }



//        /// <summary>
//        /// Star flask texture, no animation
//        /// </summary>
//        public bool IsLit
//        {
//            get
//            {
//                return sprite.GetCurAnim().name == "Spin" && !sprite.IsAnimating();
//            }
//        }



//        /// <summary>
//        /// Windows will set this to something when they want to be drawn
//        /// </summary>
//        public IDrawable Drawable
//        {
//            get
//            {
//                return drawable;
//            }

//            set
//            {
//                if (value != drawable)
//                {
//                    drawable = value;

//                    if (button.State == RUIToggleButton.ButtonState.TRUE && value == null)
//                        button.SetFalse(false);

//                    if (button.State == RUIToggleButton.ButtonState.FALSE && value != null)
//                        button.SetTrue(false);
//                }
//            }
//        }

//        #endregion
//    }
//}
