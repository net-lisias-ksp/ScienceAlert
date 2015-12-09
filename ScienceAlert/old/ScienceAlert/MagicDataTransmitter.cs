///******************************************************************************
//                   Science Alert for Kerbal Space Program                    
// ******************************************************************************
//    Copyright (C) 2014 Allen Mrazek (amrazek@hotmail.com)

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// *****************************************************************************/
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using ReeperCommon;

//namespace ScienceAlert
//{

//    /// <summary>
//    /// When the user decides to transmit data, the game selects "the best"
//    /// antenna to use and moves the ScienceData out of its experiment or
//    /// container into the transmitter.  This poses us two problems:
//    ///   1) There's no way for us to get at the data to be transmitted, so
//    ///      we don't know which info is being transmitted (or when it was, no event)
//    ///   2) Since the data was taken out of storage, we temporarily lose
//    ///      track of it until the transmitter has finished submitting it
//    ///      
//    /// As a result, while we're examining a given experiment to see if 
//    /// it should trigger a response we might find no info in R&D and no
//    /// info in storage resulting in the plugin erroneously concluding that
//    /// the experiment is fresh in the interim time while the results are
//    /// being transmitted by an antenna.
//    /// 
//    /// This method involves tricking the game into thinking there's a fake
//    /// transmitter on the vessel. Whenever it goes looking for a transmitter
//    /// to use, our magic version will score highest and be chosen. It will
//    /// receive the data to be transmitted and delegate that data to real
//    /// transmitters itself.
//    /// 
//    /// There are a couple of edge cases to be solved still, however. For
//    /// instance, a player can manually select a transmitter, click "transmit"
//    /// and bypass this transmitter. I haven't fixed this yet because I still
//    /// need to determine whether anyone actually uses this method of transmission.
//    /// 
//    /// Note: Why not use MM to edit the transmitter module itself with our own
//    /// version? It will contaminate the player's save. Also I'm experimenting 
//    /// with adding PartModules on-the-fly
//    /// </summary>
//    using ScienceDataList = List<ScienceData>;

//    public class MagicDataTransmitter : PartModule, IScienceDataTransmitter
//    {
//        // keep track of which (REAL) transmitter is transmitting what
//        private Dictionary<IScienceDataTransmitter, KeyValuePair<ScienceDataList, Callback>> realTransmitters =
//            new Dictionary<IScienceDataTransmitter, KeyValuePair<ScienceDataList, Callback>>();

//        // If all transmitters are busy, we'll need a mechanism to wait for one
//        // to become ready. It seems that sending data to a busy transmitter
//        // will result in it getting stalled, remaining open but
//        // not sending data
//        private Dictionary<IScienceDataTransmitter,
//            Queue<
//                KeyValuePair<ScienceDataList, Callback>>
//            > toBeTransmitted =
//                new Dictionary<IScienceDataTransmitter, Queue<KeyValuePair<ScienceDataList, Callback>>>();

//        internal StorageCache cacheOwner;


///******************************************************************************
// *                    Implementation Details
// ******************************************************************************/

//        /// <summary>
//        /// Called just as the PartModule is added to a Part.  We can
//        /// count on doing our initialization here because ScienceAlert
//        /// will destroy older copies of MagicDataTransmitters by default.  This
//        /// is done mainly so we don't have to keep track of the MagicDataTransmitter
//        /// instance and can restrict any updates to here.
//        /// </summary>
//        public void Start()
//        {
//            Log.Debug("MagicDataTransmitter started");

//            // locate all available real transmitters
//            var transmitters = FlightGlobals.ActiveVessel.FindPartModulesImplementing<IScienceDataTransmitter>();

//            // remove the fake ones ..
//            transmitters.RemoveAll(tx => tx is MagicDataTransmitter);

//            foreach (var tx in transmitters)
//            {
//                realTransmitters.Add(tx, new KeyValuePair<ScienceDataList, Callback>());
//                toBeTransmitted.Add(tx, new Queue<KeyValuePair<ScienceDataList, Callback>>());
//            }

//            Log.Debug("MagicDataTransmitter has found {0} useable transmitters", transmitters.Count);
//        }



//        private void BeginTransmissionWithRealTransmitter(IScienceDataTransmitter transmitter, ScienceDataList science,
//            Callback callback)
//        {
//            Log.Debug("Beginning real transmission of " + science.Count + " science reports on transmitter " +
//                      ((PartModule) transmitter).part.flightID);

//            if (callback != null)
//                transmitter.TransmitData(science, () => TransmissionComplete(transmitter, callback));
//            else transmitter.TransmitData(science);
//        }


//        public void Update()
//        {
//            // keep an eye on queued transmission data and send it as
//            // as soon as its assigned transmitter becomes available
//            var txs = toBeTransmitted.CoreKeys;

//            try
//            {
//                foreach (var tx in txs)
//                    if (toBeTransmitted[tx].Count > 0)
//                        if (!tx.IsBusy() && tx.CanTransmit())
//                        {
//                            var nextData = toBeTransmitted[tx].Dequeue();

//                            Log.Debug("Dispatching " + nextData.Key.Count + " science data entries to transmitter");

//                            realTransmitters[tx] = nextData;

//                            BeginTransmissionWithRealTransmitter(tx, nextData.Key, nextData.Value);
//                        }
//            }
//            catch (KeyNotFoundException)
//            {
//                // I'm not sure exactly what causes this to occur
//                // It may be a result of a mod collision or perhaps an event
//                // managed to slip through while the vessel is exploding. Last
//                // time I observed this personally was as the vessel was rolling
//                // down a hill shedding pieces furiously
//                //
//                // most likely, the transmitter was destroyed. Might be related
//                // to a bug I found with RebuildObservers

//                /*
//                 * KeyNotFoundException: The given key was not present in the dictionary.
//                      at System.Collections.Generic.Dictionary`2[IScienceDataTransmitter,System.Collections.Generic.List`1[ScienceData]].get_Item (IScienceDataTransmitter key) [0x00000] in <filename unknown>:0 
//                      at ScienceAlert.MagicDataTransmitter.Update () [0x00000] in <filename unknown>:0 
//                 **/
//                Log.Error("MagicDataTransmitter appears to be out of date. Any queued data might have been lost.");

//                toBeTransmitted.Clear();
//                realTransmitters.Clear();

//                cacheOwner.ScheduleRebuild();  
//            }
//        }



//        public override void OnSave(ConfigNode node)
//        {
//            node.ClearData();
//        }



//        public override void OnLoad(ConfigNode node)
//        {
//            // empty
//        }


//        /// <summary>
//        /// A suitable transmitter has been selected to submit some data. We'll queue
//        /// it for transmission in the next Update().
//        /// </summary>
//        /// <param name="data"></param>
//        /// <param name="transmitter"></param>
//        private void QueueTransmission(ScienceDataList data, IScienceDataTransmitter transmitter, Callback callback)
//        {

//            if (data.Count == 0)
//            {
//                Log.Warning("Empty science list cannot be queued for transmission");
//                return;
//            }
//            Log.Debug("Queued " + data.Count + " science reports for transmission");

//#if DEBUG
//            if (!realTransmitters.ContainsKey(transmitter))
//                Log.Error("MagicDataTransmitter.DoTransmit - Given transmitter isn't in real transmitter list!");
//#endif

//            toBeTransmitted[transmitter].Enqueue(new KeyValuePair<ScienceDataList, Callback>(
//                new ScienceDataList(data), callback)); // note: we copy the list, else somebody else might try messing with it
//                                                       // before transmission is complete (I'm looking at you, SCANsat)
//        }



//        /// <summary>
//        /// Locate a suitable transmitter and queue this data up for it
//        /// </summary>
//        /// <param name="data"></param>
//        void IScienceDataTransmitter.TransmitData(ScienceDataList data)
//        {
//            TransmitData(data, null);
//        }

//        public void TransmitData(ScienceDataList dataQueue, Callback callback)
//        {
//            Log.Debug("MagicTransmitter: received {0} ScienceData entries", dataQueue.Count);
//            if (callback == null) Log.Debug(" with no callback");
//            else Log.Debug("With callback");

//            // locate the best actual transmitter to send this data through
//            // lower scores seem to be better
//            var potentials = new List<IScienceDataTransmitter>();

//            foreach (var kvp in realTransmitters)
//                potentials.Add(kvp.Key);

//            if (potentials.Count > 0)
//            {
//                potentials = potentials.OrderBy(potential => ScienceUtil.GetTransmitterScore(potential)).ToList();

//                QueueTransmission(dataQueue, potentials.First(), callback);
//            }
//            else Log.Error("MagicDataTransmitter: Did not find any real transmitters");
//        }

//        bool IScienceDataTransmitter.IsBusy()
//        {
//            return false;  // magic transmitter is always ready
//        }

//        bool IScienceDataTransmitter.CanTransmit()
//        {
//            return realTransmitters.Any(pair => pair.Key.CanTransmit());
//        }

//        float IScienceDataTransmitter.DataRate
//        {
//            get
//            {
//                return float.MaxValue;
//            }
//        }

//        double IScienceDataTransmitter.DataResourceCost
//        {
//            get
//            {
//                return 0d;
//            }
//        }


//        private void TransmissionComplete(IScienceDataTransmitter transmitter, Callback original)
//        {
//            Log.Debug("Received TransmissionComplete callback from " + transmitter.GetType().Name);

//            if (original != null) original();
//        }
        
//        public ScienceDataList     QueuedData
//        {
//            get
//            {
//                ScienceDataList list = new ScienceDataList();
//                bool badFlag = false; // used to exterminate a KeyNotFound exception that
//                                      // seems to slip through now and then

//                try
//                {
//                    foreach (var kvp in realTransmitters)
//                    {
//                        if (kvp.Key == null)
//                        {
//                            // something happened to cause our transmitter list to
//                            // be wrong somehow
//                            Log.Error("MagicDataTransmitter: Encountered a bad transmitter value.");
//                            if (kvp.Key == null) Log.Error("IScienceDataTransmitter key was null");
//                            if (kvp.Value.Key == null) Log.Error("IScienceDataTransmitter KeyValuePair was null");

//                            badFlag = true;
//                            continue;
//                        }

//                        if (!kvp.Key.IsBusy() && kvp.Value.Key != null)
//                            kvp.Value.Key.Clear();
                        
//                        if (kvp.Value.Key != null)
//                            list.AddRange(kvp.Value.Key);

//                        list.AddRange(toBeTransmitted[kvp.Key].SelectMany(transmitterData => transmitterData.Key));
//                    }

//                } catch (Exception e)
//                {
//                    badFlag = true;
//                    Log.Error("Exception occurred in MagicDataTransmitter.QueuedData: {0}", e);
//                }

//                if (badFlag)
//                {
//                    Log.Warning("Resetting MagicDataTransmitter due to bad transmitter key or value");
//                    cacheOwner.ScheduleRebuild();
//                }

//                return list;
//            }
//        }

//        public override string ToString()
//        {
//            return string.Format("MagicDataTransmitter attached to {0}; {1} entries in queue", FlightGlobals.ActiveVessel.rootPart.name, QueuedData.Count);
//        }
//    }
//}
