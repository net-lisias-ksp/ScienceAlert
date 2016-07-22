using ReeperCommon.Logging;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable UnusedMember.Global
#pragma warning disable 169

namespace ScienceAlert.UI.OptionsWindow
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class OptionsListItem : MonoBehaviour
    {
        [SerializeField] private Text _headerText;

        private void Start()
        {
            
        }





        // UnityAction
        public void OnEnableAlertsToggled(bool tf)
        {
            Log.Warning("toggled alerts " + tf);
        }


        // UnityAction
        public void OnStopWarpOnAlertToggled(bool tf)
        {
            Log.Warning("toggled stop warp on alert " + tf);
        }


        // UnityAction
        public void OnRecoveryAlertToggled(bool tf)
        {
            Log.Warning("toggled recovery alerts " + tf);
        }


        // UnityAction
        public void OnTransmissionAlertToggled(bool tf)
        {
            Log.Warning("toggled transmission alerts " + tf);
        }


        // UnityAction
        public void OnLabAlertToggled(bool tf)
        {
            Log.Warning("toggled lab alerts " + tf);
        }


        // UnityAction
        public void OnSoundOnAlertToggled(bool tf)
        {
            Log.Warning("toggled sound on alert " + tf);
        }


        // UnityAction
        public void OnAnimationOnAlertToggled(bool tf)
        {
            Log.Warning("toggled animation on alert " + tf);
        }


        // UnityAction
        public void OnSubjectResearchThresholdChanged(float newValue)
        {
            Log.Warning("Subject research threshold changed to " + newValue);
        }

        // UnityAction
        public void OnReportScienceValueIgnoreThresholdChanged(float newValue)
        {
            Log.Warning("Ignore threshold changed to " + newValue);
        }
    }
}
