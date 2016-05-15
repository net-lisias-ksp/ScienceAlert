using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using ReeperCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace ScienceAlert.UI
{
    [RequireComponent(typeof(RectTransform), typeof(RawImage)), DisallowMultipleComponent]
    class TestCaptureTheory : MonoBehaviour
    {
        private RawImage _staticImage;
        private RenderTexture _renderTexture;
        private bool _frozen = false;

        public RectTransform Target;
        public GameObjectStateBlock StateBlock = new GameObjectStateBlock();

        private void Awake()
        {
            _staticImage = GetComponent<RawImage>();
        }


        public void Freeze()
        {
  
            if (_frozen) return;

            if (Target == null)
            {
                Log.Error("Missing target RectTransform");
                return;
            }
            print("Freeze begun");

            var canvas = Target.GetComponentInParent<Canvas>();

            if (canvas == null)
            {
                Log.Warning("No canvas found");
                return;
            }

            var cam = GetCanvasCamera(canvas);

            if (!cam.Any())
            {
                Log.Warning("No camera found");
                return;
            }

            _renderTexture = new RenderTexture(Screen.width, Screen.height, 8);
            var oldTarget = cam.Value.targetTexture;

            cam.Value.targetTexture = _renderTexture;
            cam.Value.Render();
            cam.Value.targetTexture = oldTarget;

            _staticImage.texture = _renderTexture;
            Target.gameObject.SetActive(false);
            _frozen = true;
            _staticImage.enabled = true;

            print("Frozen!");
        }


        public void Unfreeze()
        {
            if (!_frozen) return;

            print("Unfreezing");

            _staticImage.texture = null;
            _renderTexture.Do(rt => rt.Release());
            _staticImage.enabled = false;
        }


        private static Maybe<Camera> GetCanvasCamera(Canvas parentCanvas)
        {
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return Maybe<Camera>.None;
            return parentCanvas.worldCamera.ToMaybe();
        }
    }
}
