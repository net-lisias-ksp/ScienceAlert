using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649

namespace ScienceAlert.UI.TooltipWindow
{
    [DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TooltipWindowView : ManualRegistrationView
    {
        [SerializeField] private RectTransform _movingPanel;
        [SerializeField] private Text _tooltipText;

        private RectTransform _transform;
        private Camera _camera;
        private Canvas _canvas;

        public bool Visible
        {
            get { return gameObject.activeInHierarchy; }
            set
            {
                gameObject.SetActive(value);
                if (value)
                    transform.SetAsLastSibling();
            }
        }

        protected override void Start()
        {
            base.Start();
            _transform = GetComponent<RectTransform>();
            FindAssociatedCamera();
        }


        // ReSharper disable once UnusedMember.Local
        private void OnTransformParentChanged()
        {
            FindAssociatedCamera();
        }


        private void FindAssociatedCamera()
        {
            _camera = null;
            _canvas = GetComponentInParent<Canvas>().IfNull(() => Log.Error("No associated Canvas found"));

            if (!_canvas) return;

            // if render mode is overlay, we don't need a camera to pass to ScreenPointToLocalPointInRectangle
            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) return;

            _camera = _canvas.worldCamera.IfNull(() => Log.Error("No worldCamera found on Canvas")).IfNull(() => _canvas = null);
        }

        

        public void SetTooltip(string text)
        {
            _tooltipText.text = text;
        }


        // ReSharper disable once UnusedMember.Local
        private void LateUpdate()
        {
            if (!_canvas) return;

            FollowMouse();
        }


        private void FollowMouse()
        {
            Vector2 localPoint;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_transform, Input.mousePosition, _camera,
                    out localPoint))
                return;

            _movingPanel.anchoredPosition = _transform.TransformPoint(localPoint);
        }
    }
}
