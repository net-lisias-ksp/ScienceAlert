using System;
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
        internal readonly Signal<GameEvents.FromToAction<Part, Part>> CrewOnEva = new Signal<GameEvents.FromToAction<Part, Part>>();
        internal readonly Signal<GameEvents.HostedFromToAction<ProtoCrewMember, Part>> CrewTransferred = new Signal<GameEvents.HostedFromToAction<ProtoCrewMember, Part>>();
        internal readonly Signal<GameScenes> GameSceneLoadRequested = new Signal<GameScenes>();
        internal readonly Signal<float, ScienceSubject, ProtoVessel, bool> ScienceReceived = new Signal<float, ScienceSubject, ProtoVessel, bool>();
        internal readonly Signal ApplicationQuit = new Signal();

        internal readonly Signal GameUpdateTick = new Signal();

        protected override void Start()
        {
            base.Start();
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselDestroy.Add(OnVesselDestroy);
            GameEvents.onVesselWasModified.Add(OnVesselModified);
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            GameEvents.OnScienceRecieved.Add(OnScienceReceived);
            GameEvents.onCrewOnEva.Add(OnCrewOnEva);
            GameEvents.onCrewTransferred.Add(OnCrewTransferred);
        }


        protected override void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
            GameEvents.OnScienceRecieved.Remove(OnScienceReceived);
            GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
            GameEvents.onCrewTransferred.Remove(OnCrewTransferred);
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


        private void OnCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> data)
        {
            throw new NotImplementedException();
        }

        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            throw new NotImplementedException();
        }


        private void OnGameSceneLoadRequested(GameScenes data)
        {
            GameSceneLoadRequested.Dispatch(data);
        }


// ReSharper disable once UnusedMember.Local
        private void OnApplicationQuit()
        {
            ApplicationQuit.Dispatch();
        }


        private void OnScienceReceived(float data0, ScienceSubject data1, ProtoVessel data2, bool data3)
        {
            ScienceReceived.Dispatch(data0, data1, data2, data3);
        }


// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            GameUpdateTick.Dispatch();
        }
    }
}
