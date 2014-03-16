using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ExperimentIndicator
{

    /// <summary>
    /// When the user decides to transmit data, the game selects "the best"
    /// antenna to use and moves the ScienceData out of its experiment or
    /// container into the transmitter.  This poses us two problems:
    ///   1) There's no way for us to get at the data to be transmitted, so
    ///      we don't know which info is being transmitted (or when it was, no event)
    ///   2) Since the data was taken out of storage, we temporarily lose
    ///      track of it until the transmitter has finished submitting it
    ///      
    /// As a result, while we're examining a given experiment to see if 
    /// it should trigger a response we might find no info in R&D and no
    /// info in storage resulting in the plugin erroneously concluding that
    /// the experiment is fresh in the interim time while the results are
    /// being transmitted by an antenna.
    /// 
    /// So, how do we solve this?  One way is to implement our very own
    /// transmitter which includes the functions we need to examine data
    /// being transmitted and then using something like ModuleManager to edit
    /// all ModuleDataTransmitters into our custom version.  This would probably
    /// work, but it has some disadvantages.  The main one is that it will make
    /// the user's save dependent on this plugin existing the moment their 
    /// persistent file is saved.  This is REALLY bad for an info plugin; if
    /// they delete it and try to load their file, ALL OF THEIR ANTENNA-USING 
    /// VESSELS WILL BE DELETED!
    /// 
    /// We might be able to hijack the modules ingame.  I experimented with this
    /// a bit and it was messy.
    /// 
    /// We might be able to tap into the Actions/Events for a given part and 
    /// insert some stub functions which let us know when something is being
    /// submitted, and then perhaps suppress experiment detection while its
    /// active.  Not a real great solution either.
    /// 
    /// We could keep a short memory.  A band-aid solution at best.  What happens
    /// when the craft is out of power trying to transmit the first packet?
    /// 
    /// But there's a really simple way we can accomplish everything we want
    /// WITHOUT imposing any save-breaking changes: we create a fake transmitter.
    /// Recall that on craft with multiple transmitters, the game says something
    /// along the lines of "searching for the best one".  So what if we made a fake
    /// transmitter and rigged it to stand miles above any normal transmitter on
    /// the craft?  Then any time the user wanted to submit data, it'd go right to
    /// our fake transmitter which would in turn pass the actual submission to
    /// real transmitters.  Simple!  The only tricky bit will be installing the
    /// module at runtime and making sure it doesn't pollute the user's save file.
    /// This turned out to be considerably easier than I thought.
    /// </summary>
    using ScienceDataList = List<ScienceData>;

    public class MagicDataTransmitter : PartModule, IScienceDataTransmitter
    {
        // keep track of which (REAL) transmitter is transmitting what
        private Dictionary<IScienceDataTransmitter, ScienceDataList> realTransmitters = new Dictionary<IScienceDataTransmitter, ScienceDataList>();


        /// <summary>
        /// Called just as the PartModule is added to a Part.  We can
        /// count on doing our initialization here because ExperimentIndicator
        /// will destroy older copies of MagicDataTransmitters by default.  This
        /// is done mainly so we don't have to keep track of the MagicDataTransmitter
        /// instance and can restrict any updates to here.
        /// </summary>
        public void Start()
        {
            Log.Debug("MagicDataTransmitter started");

            // locate all available real transmitters
            var transmitters = FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataTransmitter>();

            // remove the fake ones ..
            transmitters.RemoveAll(tx => tx is MagicDataTransmitter);

            foreach (var tx in transmitters)
                realTransmitters.Add(tx, new ScienceDataList());

            Log.Debug("MagicDataTransmitter has found {0} useable transmitters", transmitters.Count);
        }



        public override void OnSave(ConfigNode node)
        {
#if !DEBUG // permit poisoning in debug games
            node.ClearData(); // don't save anything about MagicDataTransmitter or
                              // the save file will be poisoned
#endif
        }

        public override void OnLoad(ConfigNode node)
        {
            // empty
        }

        void IScienceDataTransmitter.TransmitData(ScienceDataList data)
        {
            Log.Debug("MagicTransmitter: received {0} ScienceData entries", data.Count);

            // locate the best actual transmitter to send this data through
            // lower scores seem to be better
            var potentials = new List<IScienceDataTransmitter>();

            foreach (var kvp in realTransmitters)
                potentials.Add(kvp.Key);

            if (potentials.Count > 0)
            {
                potentials.OrderBy(potential => ScienceUtil.GetTransmitterScore(potential));

                if (!potentials.First().IsBusy())
                    realTransmitters[potentials.First()].Clear();

                realTransmitters[potentials.First()].AddRange(data);
                potentials.First().TransmitData(data);

                Log.Debug("Sent data to transmitter {0}, total queued {1}", potentials.First().ToString(), QueuedData.Count);
            }
            else Log.Error("MagicDataTransmitter: Did not find any real transmitters");
        }

        bool IScienceDataTransmitter.IsBusy()
        {
            return false;  // magic transmitter is always ready
        }

        bool IScienceDataTransmitter.CanTransmit()
        {
            return realTransmitters.Any(pair => pair.Key.CanTransmit());
        }

        float IScienceDataTransmitter.DataRate
        {
            get
            {
                return float.MaxValue;
            }
        }

        double IScienceDataTransmitter.DataResourceCost
        {
            get
            {
                return 0d;
            }
        } 

        
        public ScienceDataList     QueuedData
        {
            get
            {
                ScienceDataList list = new ScienceDataList();

                foreach (var kvp in realTransmitters)
                {
                    if (!kvp.Key.IsBusy())
                        kvp.Value.Clear(); // it's not doing anything, therefore nothing is queued

                    list.AddRange(kvp.Value);
                }

                return list;
            }
        }

        public override string ToString()
        {
            return string.Format("MagicDataTransmitter attached to {0}; {1} entries in queue", FlightGlobals.ActiveVessel.rootPart.ConstructID, QueuedData.Count);
        }
    }
}
