using System;
using System.Collections.Generic;
using System.Linq;
using ReeperCommon;
using ScienceAlert.KSPInterfaces.FlightGlobals;

namespace ScienceAlert.Experiments.Science
{
    /// <summary>
    /// The purpose of this object is to reduce redundancy between 
    /// ExperimentObservers.  ScienceData isn't sorted by ScienceExperiment
    /// but rather by id, so it makes more sense to have one object track
    /// it than have every observer do so.
    /// 
    /// StoredVesselScience will need to be hooked up to the proper events
    /// in order to work
    /// </summary>
    public class StoredVesselScience : IStoredVesselScience
    {
        private List<IScienceDataContainer> storage;                      // containers for science data
        private MagicDataTransmitter _magicTransmitter;                    // MagicDataTransmitter keeps an eye on any queued data for the vessel
        private readonly IActiveVesselProvider _vesselProvider;
        private IVessel _monitoredVessel;


/******************************************************************************
 *                          Begin Implementation
 *****************************************************************************/
        public StoredVesselScience(IActiveVesselProvider vesselProvider)
        {
            if (vesselProvider.IsNull())
                throw new ArgumentNullException("vesselProvider");

            _vesselProvider = vesselProvider;

            ScanVesselForScienceContainers();
        }



        public void OnDestroy()
        {
            RemoveMagicTransmitter(false);
        }



        #region events


        /// <summary>
        /// Player is swapping vessels
        /// </summary>
        /// <param name="v"></param>
        public void OnVesselChange(IVessel v)
        {
            Log.Debug("StorageCache.OnVesselChange to {0}", v.vesselName);

            RemoveMagicTransmitter();
            _monitoredVessel = v;
            ScanVesselForScienceContainers();
        }



        public void OnVesselWasModified(IVessel v)
        {
            Log.Debug("StorageCache.OnVesselModified");

            if (_monitoredVessel != v)
            {
                OnVesselChange(v);
            }
            else ScanVesselForScienceContainers();
        }



        public void OnVesselDestroy(IVessel v)
        {
            if (_monitoredVessel == v)
            {
                Log.Debug("StorageCache.OnVesselDestroyed");

                storage = new List<IScienceDataContainer>();
                _magicTransmitter = null;
                _monitoredVessel = null;
            }
        }

        public IEnumerable<ScienceData> ScienceData
        {
            get
            {
                var found = new List<ScienceData>();

                storage.ForEach(container => found.AddRange(container.GetData()));

                if (_magicTransmitter == null) return found;

                found.AddRange(_magicTransmitter.QueuedData);

                return found;
            }
        }

        #endregion





        /// <summary>
        /// Rebuild storage cache.  This object is shared between
        /// all ExperimentObservers and allows them to determine what data
        /// is onboard the craft, being transmitted or has been submitted to
        /// R&D
        /// </summary>
        public void ScanVesselForScienceContainers()
        {
            IsBusy = true;

            Log.Debug("StorageCache: Rebuilding ...");
            storage.Clear();

            if (_vesselProvider.GetActiveVessel().SingleOrDefault() != _monitoredVessel)
            {
                // this could be an indication that we're not monitoring
                // the active vessel properly
                // note to self: this does occur from time to time but I have yet to nail down
                // the specifics of why this is
                Log.Error("StorageCache: Active vessel is not monitored vessel.");


                RemoveMagicTransmitter();
                _monitoredVessel = _vesselProvider.GetActiveVessel().SingleOrDefault();
                
            }

            if (_monitoredVessel == null) return;

            // ScienceContainers are straightforward ...
            storage = _monitoredVessel.GetScienceContainers().ToList();

            Log.Debug("StorageCache: located {0} IScienceDataContainers", storage.Count);


            // now we must deal with IScienceDataTransmitters, which are not 
            // quite as simple due to the MagicDataTransmitter we're faking

                // remove any existing magic transmitters
                //   note: we include non-root parts here because it's possible
                //         for two vessels to have merged, due to docking for instance.
                // 
                //          We could theoretically only remove MagicTransmitters from
                //          the root parts of docked vessels, but there may be cases
                //          where vessels that couldn't normally dock do (some kind of plugin perhaps)
                //          so I've opted for a general solution here
                RemoveMagicTransmitter(false);   

            
                // count the number of "real" transmitters onboard
                List<IScienceDataTransmitter> transmitters = FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataTransmitter>();
                transmitters.RemoveAll(tx => tx is MagicDataTransmitter);

            
                if (transmitters.Count > 0)
                {
                    // as long as at least one transmitter is "real", the
                    // vessel's root part should have a MagicDataTransmitter
                    if (transmitters.Any(tx => !(tx is MagicDataTransmitter)))
                    {
                        _magicTransmitter = _monitoredVessel.rootPart.AddModule("MagicDataTransmitter") as MagicDataTransmitter;
                        _magicTransmitter.cacheOwner = this;

                        Log.Debug("Added MagicDataTransmitter to root part {0}", FlightGlobals.ActiveVessel.rootPart.ConstructID);

                    }
                } else
                {
                    Log.Debug("Vessel {0} has no transmitters; no magic transmitter added", _monitoredVessel.vesselName);
                }

            IsBusy = false;
            Log.Debug("Rebuilt StorageCache");
        }



        /// <summary>
        /// Remove the magic transmitter (if it exists) from the given Vessel.
        /// Mainly used so that only the active vessel has a magic transmitter.  
        /// 
        /// There's a chance that in specific circumstances, having an unmonitored
        /// magic transmitter might cause problems.
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="rootOnly"></param>
        private void RemoveMagicTransmitter(bool rootOnly = true)
        {
            if (_monitoredVessel != null)
                if (_monitoredVessel.rootPart != null)
                {
                    try
                    {
                        if (_monitoredVessel.rootPart.Modules.Contains("MagicDataTransmitter"))
                            _monitoredVessel.rootPart.RemoveModule(_monitoredVessel.rootPart.Modules.OfType<MagicDataTransmitter>().Single());

                        if (!rootOnly)
                            foreach (var part in _monitoredVessel.Parts)
                                if (part.Modules.Contains("MagicDataTransmitter"))
                                    part.RemoveModule(part.Modules.OfType<MagicDataTransmitter>().First());
                    } catch (Exception e)
                    {
                        Log.Warning("RemoveMagicTransmitter: caught exception {0}", e);
                    }
                }

            _magicTransmitter = null;
        }



#if DEBUG
        public void DumpContainerData()
        {
            Log.Debug("Dumping container data...");

            if (storage == null)
                Log.Error("storage is null");

            foreach (var container in storage)
            {
                if (container == null)
                    Log.Error("container is null");

                if (container.GetData() == null)
                {
                    Log.Error("container.GetData() is null");
                }
                else
                {
                    foreach (var data in container.GetData())
                    {
                        if (data != null)
                        {
                            Log.Debug("  Data: {0}, title {1}", data.subjectID, data.title);
                        }
                        else Log.Error("data in container.getData() is null");
                    }
                }
            }

            Log.Debug("Dumping transmitter data ...");
            if (_magicTransmitter == null)
                Log.Error("magicTransmitter is null!");

            foreach (var data in _magicTransmitter.QueuedData)
                Log.Debug("  Data: {0}, title {1}", data.subjectID, data.title);

            Log.Debug("Finished dumping container data!");
        }
        
#endif

        public int StorageContainerCount
        {
            get
            {
                return storage.Count;
            }
        }


        public bool IsBusy
        {
            get;
            private set;
        }
    }
}
