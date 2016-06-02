using System;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;

namespace ScienceAlert.Game
{
    [MediatedBy(typeof(GameEventMediator))]
// ReSharper disable once ClassNeverInstantiated.Global
    public class GameEventView : View
    {
        internal readonly Signal ActiveVesselModified = new Signal();
        internal readonly Signal ActiveVesselChanged = new Signal();
        internal readonly Signal ActiveVesselDestroyed = new Signal();
        internal readonly Signal ActiveVesselCrewModified = new Signal();

        internal readonly Signal<Vessel> VesselModified = new Signal<Vessel>();
        internal readonly Signal<GameEvents.FromToAction<Part, Part>> CrewBoardVessel = new Signal<GameEvents.FromToAction<Part, Part>>();
        internal readonly Signal<GameEvents.FromToAction<Part, Part>> CrewOnEva = new Signal<GameEvents.FromToAction<Part, Part>>();
        internal readonly Signal<GameEvents.HostedFromToAction<ProtoCrewMember, Part>> CrewTransferred = new Signal<GameEvents.HostedFromToAction<ProtoCrewMember, Part>>();
        internal readonly Signal<EventReport> CrewKilled = new Signal<EventReport>();

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

            GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
            GameEvents.onCrewKilled.Add(OnCrewKilled);
            GameEvents.onCrewTransferred.Add(OnCrewTransferred);
            GameEvents.onCrewOnEva.Add(OnCrewOnEva);
        }


        protected override void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
            GameEvents.OnScienceRecieved.Remove(OnScienceReceived);

            GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
            GameEvents.onCrewKilled.Remove(OnCrewKilled);
            GameEvents.onCrewTransferred.Remove(OnCrewTransferred);
            GameEvents.onCrewOnEva.Remove(OnCrewOnEva);

            base.OnDestroy();
        }


        private static bool IsActiveVessel(Vessel v)
        {
            return v != null && ReferenceEquals(FlightGlobals.ActiveVessel, v);
        }


        private void OnVesselChange(Vessel data)
        {
            ActiveVesselChanged.Dispatch();
        }


        private void OnVesselDestroy(Vessel data)
        {
            if (IsActiveVessel(data))
            {
                Log.Debug("OnVesselDestroy.Active");
                ActiveVesselDestroyed.Dispatch();
            }
            else Log.Debug("OnVesselDestroy.Nonactive");
        }


        private void OnVesselModified(Vessel data)
        {
            if (IsActiveVessel(data))
                ActiveVesselModified.Dispatch();

            VesselModified.Dispatch(data);
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


        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            if (IsActiveVessel(data.from.With(f => f.vessel)) || IsActiveVessel(data.to.With(t => t.vessel)))
                ActiveVesselCrewModified.Dispatch();

            CrewBoardVessel.Dispatch(data);
        }


        private void OnCrewKilled(EventReport data)
        {
            if (IsActiveVessel(data.origin.With(o => o.vessel)))
                ActiveVesselCrewModified.Dispatch();

            CrewKilled.Dispatch(data);
        }


        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            if (IsActiveVessel(data.from.With(f => f.vessel)) || IsActiveVessel(data.to.With(t => t.vessel)))
                ActiveVesselCrewModified.Dispatch();

            CrewOnEva.Dispatch(data);
        }


        private void OnCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> data)
        {
            if (IsActiveVessel(data.from.With(f => f.vessel)) || IsActiveVessel(data.to.With(t => t.vessel)))
                ActiveVesselCrewModified.Dispatch();

            CrewTransferred.Dispatch(data);
        }


// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            GameUpdateTick.Dispatch();
        }
    }
}
