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
            while (!ApplicationLauncher.Ready)
                yield return null;


            var sheet = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.sheet_app.png", true, false);
            if (sheet == null) Log.Error("Failed to locate embedded app sheet texture!");

            sprite = PackedSprite.Create("ScienceAlert.Button.Animation", Vector3.zero);
            sprite.SetMaterial(new Material(Shader.Find("Sprite/Vertex Colored")) { mainTexture = sheet });
            sprite.Setup(38f, 38f);
            sprite.SetFramerate(24f);
            sprite.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);
            //sprite.renderCamera = Camera.allCameras.ToList().Find(c => (c.cullingMask & (1 << LayerMask.NameToLayer("EzGUI_UI"))) != 0);
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
                                                        OnToggle,
                                                        OnToggle,
                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                                                        sprite);

            sprite.PlayAnim("Unlit");

            Log.Debug("Finished creating mod button");
        }

        void OnDestroy()
        {
            if (button != null)
            {
                Log.Verbose("Removing ApplicationLauncherButton");
                ApplicationLauncher.Instance.RemoveModApplication(button);
            }
        }

        #region GUI events

        public void OnToggle()
        {
            Log.Write("AppLauncherInterface: OnToggle");
        }

        #endregion




        public void PlayAnimation()
        {
            sprite.PlayAnim("Spin");
        }

        public void StopAnimation()
        {
            sprite.PauseAnim();
        }

        public void SetUnlit()
        {
            sprite.PlayAnim("Unlit");
        }

        public void SetLit()
        {
            if (sprite.GetCurAnim().name != "Spin")
            {
                sprite.SetFrame("Spin", 0);
            }
            else sprite.PauseAnim();
        }


        #region properties

        public bool Important { get { return false; } set { } }

        public bool IsAnimating
        {
            get
            {
                return sprite.IsAnimating();
            }
        }

        public bool IsNormal
        {
            get
            {
                return sprite.GetCurAnim().name == "Unlit";
            }
        }

        public bool IsLit
        {
            get
            {
                return sprite.GetCurAnim().name == "Spin" && !sprite.IsAnimating();
            }
        }

        public IDrawable Drawable
        {
            get
            {
                return null;
            }

            set
            {
                
            }
        }

        #endregion
    }
}
