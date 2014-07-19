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
        private PackedSprite animationSprite;

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

            Log.Write("Testing app launcher textures");

            Func<int, int, string> GetFrame = delegate(int frame, int desiredLen)
            {
                string str = frame.ToString();

                while (str.Length < desiredLen)
                    str = "0" + str;

                return str;
            };

            #region atlas creation

#if (DEBUG && GENERATE_ATLAS)
            //Log.Write("Generating atlas");

            //ConfigNode node;
            //var textures = new List<Texture2D>();

            //Func<int, int, string> GetFrame = delegate(int frame, int desiredLen)
            //{
            //    string str = frame.ToString();

            //    while (str.Length < desiredLen)
            //        str = "0" + str;

            //    return str;
            //};

            //for (int i = 0; i < 100; ++i)
            //{
            //    var intermediate = GameDatabase.Instance.GetTexture(string.Format("/flask{0}", GetFrame(i + 1, 4)), false);
            //    if (intermediate == null) Log.Error("failed to find frame " + string.Format("flask{0}", GetFrame(i + 1, 4)));

            //    textures.Add(intermediate);
            //}
            //var atlasTexture = GuiUtil.CreateAtlas(512, 512, out node, textures);
            //atlasTexture.SaveToDisk("finished_atlas.png");
#endif
            #endregion

            // looks like we need to use a PackedSprite if we want
            // button animations
            //var normalTex = GameDatabase.Instance.GetTexture("ScienceAlert/normal", false);
            //if (normalTex == null) Log.Error("normal is null");

            var sheet = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.sheet_app.png", true, false);
            if (sheet == null) Log.Error("Failed to locate embedded app sheet texture!");

            sheet.CreateReadable().SaveToDisk("extracted.png");

            //Log.Write("loaded normaltex");
            //normalTex.SaveToDisk("saved.png");

            animationSprite = PackedSprite.Create("ScienceAlert.Button.Animation", Vector3.zero);
            animationSprite.SetMaterial(new Material(Shader.Find("Sprite/Vertex Colored")) { mainTexture = sheet });
            

            Log.Write("setup sprite");

            // normal state
                // todo

            // animated state
            animationSprite.Setup(38f, 38f);
            animationSprite.SetFramerate(24f);
            animationSprite.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);

            UVAnimation anim = new UVAnimation();
            SPRITE_FRAME[] frames = new SPRITE_FRAME[100];
            UvAtlas atlas = UvAtlas.LoadFromResource("ScienceAlert.Resources.atlas.txt", sheet.width, sheet.height);
            
            Log.Write("zero at {0}", animationSprite.PixelCoordToUVCoord(0, 0));
            Log.Write("dimensions: {0}", animationSprite.PixelSpaceToUVSpace(38, 38));


            for (int frame = 0; frame < 100; ++frame)
            {
                frames[frame] = new SPRITE_FRAME(0);
                frames[frame].uvs = atlas.GetUV(string.Format("flask{0}", GetFrame(frame + 1, 4)));

                Log.Write("frame {0} = {1}", frame, frames[frame].uvs);
            }

            anim.framerate = 24f;
            anim.loopCycles = -1;
            anim.name = "Spin";
            anim.SetAnim(frames);

            animationSprite.AddAnimation(anim);
            animationSprite.renderCamera = Camera.allCameras.ToList().Find(c => (c.cullingMask & (1 << LayerMask.NameToLayer("EzGUI_UI"))) != 0);
            animationSprite.gameObject.layer = LayerMask.NameToLayer("EzGUI_UI");
  

            Log.Debug("Creating mod button...");
            button = ApplicationLauncher.Instance.AddModApplication(
                                                        OnToggle,
                                                        OnToggle,
                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        () => { },
                                                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                                                        animationSprite);


            Log.Write("playing anim");
            //button.PlayAnim();
            animationSprite.PlayAnim("Spin");

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
            
        }

        public void StopAnimation()
        {
            
        }

        public void SetUnlit()
        {
            
        }

        public void SetLit()
        {

        }


        #region properties

        public bool Important { get { return false; } set { } }

        public bool IsAnimating
        {
            get
            {
                return false;
            }
        }

        public bool IsNormal
        {
            get
            {
                return false;
            }
        }

        public bool IsLit
        {
            get
            {
                return false;
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
