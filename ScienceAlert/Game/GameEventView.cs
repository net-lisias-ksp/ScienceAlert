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

        internal readonly Signal<GameEvents.FromToAction<CelestialBody, CelestialBody>> DominantBodyChanged =
            new Signal<GameEvents.FromToAction<CelestialBody, CelestialBody>>();

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

            GameEvents.onVesselCrewWasModified.Add(OnVesselCrewWasModified);

            GameEvents.onDominantBodyChange.Add(OnDominantBodyChanged);
        }


        protected override void OnDestroy()
        {
            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
            GameEvents.onVesselWasModified.Remove(OnVesselModified);
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
            GameEvents.OnScienceRecieved.Remove(OnScienceReceived);

            GameEvents.onVesselCrewWasModified.Remove(OnVesselCrewWasModified);

            GameEvents.onDominantBodyChange.Remove(OnDominantBodyChanged);
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


        private void OnVesselCrewWasModified(Vessel data)
        {
            if (IsActiveVessel(data))
                ActiveVesselCrewModified.Dispatch();
        }


        private void OnDominantBodyChanged(GameEvents.FromToAction<CelestialBody, CelestialBody> data)
        {
            DominantBodyChanged.Dispatch(data);
        }


// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            GameUpdateTick.Dispatch();
        }
    }
}
