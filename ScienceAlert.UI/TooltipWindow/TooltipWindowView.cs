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
            _camera = GetComponent<Canvas>().worldCamera;
        }


        public void SetTooltip(string text)
        {
            _tooltipText.text = text;
        }


        // ReSharper disable once UnusedMember.Local
        private void LateUpdate()
        {
            FollowMouse();
        }


        private void FollowMouse()
        {
            Vector2 localPoint;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_transform, Input.mousePosition, _camera,
                    out localPoint))
                return;

            _movingPanel.anchoredPosition = localPoint;
        }
    }
}
