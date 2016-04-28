using System;
using UnityEngine;

namespace ScienceAlert.UI.OptionsWindow
{
    [DisallowMultipleComponent, Serializable]
    // ReSharper disable once UnusedMember.Global
    class ExpandableButtonController : MonoBehaviour
    {
        [SerializeField]
#pragma warning disable 649
        private Animator _animator;
#pragma warning restore 649

        private readonly int _openId = Animator.StringToHash("Open");
        private bool _isOpen = false;


        // ReSharper disable once UnusedMember.Global
        public void TogglePanel()
        {
            _isOpen = !_isOpen;
            _animator.SetBool(_openId, _isOpen);
        }
    }
}
