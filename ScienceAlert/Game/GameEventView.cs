using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace ScienceAlert.Game
{
    [MediatedBy(typeof(GameEventMediator))]
// ReSharper disable once ClassNeverInstantiated.Global
    public class GameEventView : View
    {
        internal readonly Signal<Vessel> VesselModified = new Signal<Vessel>();
        internal readonly Signal<Vessel> VesselChanged = new Signal<Vessel>();
        internal readonly Signal<Vessel> VesselDestroyed = new Signal<Vessel>();


        protected override void Start()
        {
            base.Start();
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselDestroy.Add(OnVesselDestroy);
            GameEvents.onVesselWasModified.Add(OnVesselModified);
        }


        protected override void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
            base.OnDestroy();
        }


        private void OnVesselChange(Vessel data)
        {
            VesselChanged.Dispatch(data);
        }


        private void OnVesselDestroy(Vessel data)
        {
            VesselDestroyed.Dispatch(data);
        }


        private void OnVesselModified(Vessel data)
        {
            VesselModified.Dispatch(data);
        }
    }
}
