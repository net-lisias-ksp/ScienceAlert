using System.Linq;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using ScienceAlert.UI.OptionsWindow;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ScienceAlert.VesselContext.Gui
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class OptionsWindowListItemMediator : Mediator
    {
        [Inject] public OptionsListItemView View { get; set; }
        [Inject] public LocalConfiguration Configuration { get; set; }


        public override void OnRegister()
        {
            base.OnRegister();
            Log.Warning("OptionsWindowListItemMediator.OnRegister");

            if (Configuration == null) Log.Error("Configuration not set at this point");
            if (View.Experiment == null) Log.Error("View experiment not set");

            var maybeSetting =
                Configuration.With(
                    lc => lc.FirstOrDefault(es => es.Experiment.HasValue && View.Experiment.Equals(es.Experiment.Value.id)))
                    .ToMaybe();

            if (!maybeSetting.HasValue) // wat? somebody screwed up, we shouldn't be creating options for experiments that don't actually exist
            {
                Log.Warning("An options list item was created for '" +
                            View.Experiment.Return(i => i.ToString(), "<null>") + "' that has no associated experiment");
                return;
            }

            var setting = maybeSetting.Value;

            View.EnableAlertsSignal.AddListener(tf => setting.AlertsEnabled = tf);
            View.StopWarpOnAlertSignal.AddListener(tf => setting.StopWarpOnAlert = tf);
            View.RecoveryAlertSignal.AddListener(tf => setting.AlertOnRecoverable = tf);
            View.TransmissionAlertSignal.AddListener(tf => setting.AlertOnTransmittable = tf);
            View.LabAlertSignal.AddListener(tf => setting.AlertOnLabValue = tf);
            View.SoundOnAlertSignal.AddListener(tf => setting.SoundOnAlert = tf);
            View.AnimationOnAlertSignal.AddListener(tf => setting.AnimationOnAlert = tf);
            View.SubjectResearchThresholdSignal.AddListener(f => setting.SubjectResearchThreshold = f);
            View.ReportMinimumThresholdSignal.AddListener(f => setting.IgnoreThreshold = f);
        }


        public override void OnRemove()
        {
            base.OnRemove();
            Log.Warning("OptionsWindowListItemMediator.OnUnregister");

            // not strictly necessary as long as the mediator and view lifetime match (and they should), just a bit of defensive coding
            View.EnableAlertsSignal.RemoveAllListeners();
            View.StopWarpOnAlertSignal.RemoveAllListeners();
            View.RecoveryAlertSignal.RemoveAllListeners();
            View.TransmissionAlertSignal.RemoveAllListeners();
            View.LabAlertSignal.RemoveAllListeners();
            View.SoundOnAlertSignal.RemoveAllListeners();
            View.AnimationOnAlertSignal.RemoveAllListeners();
            View.SubjectResearchThresholdSignal.RemoveAllListeners();
            View.ReportMinimumThresholdSignal.RemoveAllListeners();
        }
    }
}
