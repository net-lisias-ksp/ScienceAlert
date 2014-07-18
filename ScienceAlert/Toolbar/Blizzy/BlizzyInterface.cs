using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ReeperCommon;

namespace ScienceAlert.Toolbar
{
    /// <summary>
    /// Uses Blizzy's Toolbar Wrapper to communicate with Toolbar without
    /// creating any hard dependencies, and does any related setup/teardown
    /// work.
    /// </summary>
    class BlizzyInterface : MonoBehaviour, IToolbar
    {
        private const string NormalFlaskTexture = "ScienceAlert/textures/flask";
        private List<string> StarFlaskTextures = new List<string>();

        private float FrameRate = 24f;
        private int FrameCount = 100;
        private int CurrentFrame = 0;

        IButton button;
        new System.Collections.IEnumerator animation;


//-----------------------------------------------------------------------------
// Begin implementation
//-----------------------------------------------------------------------------

        /// <summary>
        /// Create Toolbar and associated textures; setup event
        /// </summary>
        void Start()
        {
            Log.Verbose("Initializing BlizzyInterface...");

            SliceAtlasTexture();

            button = ToolbarManager.Instance.add("ScienceAlert", "PopupOpen");
            button.Text = "Science Alert";
            button.ToolTip = "Left-click to view alert experiments; Right-click for settings";
            button.OnClick += ce => ToolbarButton.TriggerOnClick(new ClickInfo() { button = ce.MouseButton });

            FrameRate = Settings.Instance.StarFlaskFrameRate;

            Log.Verbose("Finished BlizzyInterface initialization");
        }



        /// <summary>
        /// Slices up the embedded flask tileset texture and stores fake
        /// urls in GameDatabase so toolbar can find them
        /// </summary>
        private void SliceAtlasTexture()
        {
            Func<int, int, string> GetFrame = delegate(int frame, int desiredLen)
            {
                string str = frame.ToString();

                while (str.Length < desiredLen)
                    str = "0" + str;

                return str;
            };


            // load textures
            try
            {
                if (!GameDatabase.Instance.ExistsTexture(NormalFlaskTexture))
                {
                    // load normal flask texture
                    Log.Debug("Loading normal flask texture");

                    #region normal flask texture
                    Texture2D nflask = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.flask.png", true);

                    if (nflask == null)
                    {
                        Log.Error("Failed to create normal flask texture!");
                    }
                    else
                    {
                        GameDatabase.TextureInfo ti = new GameDatabase.TextureInfo(nflask, false, true, true);
                        ti.name = NormalFlaskTexture;
                        GameDatabase.Instance.databaseTexture.Add(ti);
                        Log.Debug("Created normal flask texture {0}", ti.name);
                    }

                    #endregion
                    #region sprite sheet textures


                    // load sprite sheet
                    Log.Debug("Loading sprite sheet textures...");

                    Texture2D sheet = ResourceUtil.GetEmbeddedTexture("ScienceAlert.Resources.sheet.png", false, false);

                    if (sheet == null)
                    {
                        Log.Error("Failed to create sprite sheet texture!");
                    }
                    else
                    {
                        var rt = RenderTexture.GetTemporary(sheet.width, sheet.height);
                        var oldRt = RenderTexture.active;
                        int invertHeight = ((FrameCount - 1) / (sheet.width / 24)) * 24;

                        Graphics.Blit(sheet, rt);
                        RenderTexture.active = rt;

                        for (int i = 0; i < FrameCount; ++i)
                        {
                            StarFlaskTextures.Add(NormalFlaskTexture + GetFrame(i + 1, 4));
                            Texture2D sliced = new Texture2D(24, 24, TextureFormat.ARGB32, false);

                            sliced.ReadPixels(new Rect((i % (sheet.width / 24)) * 24, /*invertHeight -*/ (i / (sheet.width / 24)) * 24, 24, 24), 0, 0);
                            sliced.Apply();

                            GameDatabase.TextureInfo ti = new GameDatabase.TextureInfo(sliced, false, false, false);
                            ti.name = StarFlaskTextures.Last();

                            GameDatabase.Instance.databaseTexture.Add(ti);
                            Log.Debug("Added sheet texture {0}", ti.name);
                        }

                        RenderTexture.active = oldRt;
                        RenderTexture.ReleaseTemporary(rt);
                    }

                    Log.Debug("Finished loading sprite sheet textures.");
                    #endregion
                }
                else
                { // textures already loaded
                    for (int i = 0; i < FrameCount; ++i)
                        StarFlaskTextures.Add(NormalFlaskTexture + GetFrame(i + 1, 4));
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to load textures: {0}", e);
            }
        }



        /// <summary>
        /// Normal cleanup
        /// </summary>
        void OnDestroy()
        {
            Log.Verbose("Destroying BlizzyInterface");
            button.Destroy();
        }


        /// <summary>
        /// Begins playing the "star flask" animation, used when a new 
        /// experiment has become available.
        /// </summary>
        public void PlayAnimation()
        {
            if (animation == null) animation = DoAnimation();
        }



        /// <summary>
        /// Stops playing animation (but leaves the current frame state)
        /// </summary>
        public void StopAnimation()
        {
            animation = null;
        }



        /// <summary>
        /// Switch to normal flask texture
        /// </summary>
        public void SetUnlit()
        {
            animation = null;
            button.TexturePath = NormalFlaskTexture;
        }


        public IDrawable Drawable
        {
            get
            {
                return button.Drawable;
            }

            set
            {
                button.Drawable = value;
            }
        }

        public bool Important
        {
            get
            {
                return button.Important;
            }

            set
            {
                button.Important = value;
            }
        }


        void Update()
        {
            if (animation != null) animation.MoveNext();
        }





        /// <summary>
        /// Is called by Update whenever animation exists to
        /// update animation frame.
        /// 
        /// Note: I didn't make this into an actual coroutine
        /// because StopCoroutine seems to sometimes throw
        /// exceptions
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator DoAnimation()
        {
            float elapsed = 0f;

            while (true)
            {
                while (elapsed < 1f / FrameRate)
                {
                    elapsed += Time.deltaTime;
                    yield return new WaitForSeconds(1f / FrameRate);
                }

                elapsed -= 1f / FrameRate;

                CurrentFrame = (CurrentFrame + 1) % FrameCount;
                button.TexturePath = StarFlaskTextures[CurrentFrame];
            }
        }
    }
}
