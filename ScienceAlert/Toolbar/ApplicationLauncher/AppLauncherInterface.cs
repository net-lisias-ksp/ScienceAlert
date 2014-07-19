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
using UnityEngine;
using System.Linq;
using ReeperCommon;
using ImprovedAddonLoader;

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

            Log.Debug("Generating button texture...");
            var tex = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            tex.GenerateRandom();
            tex.SaveToDisk("random.png");
            Log.Debug("Generated debug texture and saved to disk.");

            // looks like we need to use a PackedSprite if we want
            // button animations
            var normalTex = GameDatabase.Instance.GetTexture("ScienceAlert/normal", false);
            if (normalTex == null) Log.Error("normal is null");

            Log.Write("loaded normaltex");
            normalTex.SaveToDisk("saved.png");

            animationSprite = PackedSprite.Create("ScienceAlert.Button.Animation", Vector3.zero);
            animationSprite.SetMaterial(new Material(Shader.Find("KSP/Unlit" /*Sprite/Vertex Colored"*/)) { mainTexture = normalTex });
            

            Log.Write("setup sprite");

            // normal state
            //var normal = new CSpriteFrame();
            //normal.uvs = new Rect(0f, 0f, 1f, 1f);

            //Log.Write("set anim");
            //UVAnimation animate = new UVAnimation();
            //animate.framerate = 24f;
            ////animate.BuildUVAnim(animationSprite.PixelSpaceToUVSpace(Vector2.zero), animationSprite.PixelSpaceToUVSpace(new Vector2(38f, 38f)), 1, 1, 1);
            //animate.loopCycles = -1;
            //animate.SetAnim(new SPRITE_FRAME[] { normal.ToStruct() });

            //Log.Write("38,38 into uv space: {0}", animationSprite.PixelSpaceToUVSpace(new Vector2(38f, 38f)).ToString());

            //animationSprite.AddAnimation(animate);
            

            ////animationSprite.animations[0].SetAnim(new SPRITE_FRAME[] { normal.ToStruct() });

            //Log.Write("init uvs");
            //animationSprite.InitUVs();


            animationSprite.Setup(38f, 38f);
            animationSprite.SetFramerate(24f);
            animationSprite.SetAnchor(SpriteRoot.ANCHOR_METHOD.UPPER_LEFT);

            UVAnimation anim = new UVAnimation();
            SPRITE_FRAME[] frames = new SPRITE_FRAME[1];

            // Adjust each frame's properties here
            // There might be a way to get each frame information but I haven't found it yet
            //frames[0] = new SPRITE_FRAME(0);
            //frames[0].uvs = new Rect(0.25f, 0.25f, .75f, .75f);

            //anim.SetAnim(frames);
            anim.BuildUVAnim(Vector2.zero, new Vector2(0.05f, 0.05f), 20, 1, 20);
            anim.framerate = 24f;
            anim.loopCycles = -1;
            

            animationSprite.AddAnimation(anim);
            animationSprite.renderCamera = Camera.allCameras.ToList().Find(c => (c.cullingMask & (1 << LayerMask.NameToLayer("EzGUI_UI"))) != 0);
            animationSprite.gameObject.layer = LayerMask.NameToLayer("EzGUI_UI");
            animationSprite.gameObject.SetActive(true);

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
                                                        //normalTex);

            //button.Setup(animationSprite);
            //button.SetTexture(tex);
            //button.Setup(tex);

            Log.Write("playing anim");
            button.PlayAnim();

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
