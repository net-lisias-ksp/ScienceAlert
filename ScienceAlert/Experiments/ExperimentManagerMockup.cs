using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReeperCommon;
using UnityEngine;

namespace ScienceAlert.Experiments
{
    using ScienceList = List<ScienceExperiment>;


    public delegate void ExperimentAvailableDelegate(ScienceExperiment experiment, float calculatedValue);

    

    public class ExperimentManagerMockup : MonoBehaviour
    {
        // events
        public event ExperimentAvailableDelegate OnExperimentAvailable = delegate { };

        // other members
        ScienceList availableList = new ScienceList();

/******************************************************************************
 *                    Implementation Details
 ******************************************************************************/

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }


        #region static methods and properties

        public static int AvailableCount
        {
            get
            {
                return Instance.availableList.Count;
            }
        }
        

        #endregion

#region properties

        public static ExperimentManagerMockup Instance 
        {
            get
            {
                if (_instance == null)
                {
                    Log.Warning("ExperimentManager not found; creating one"); // this should never occur. We'll try and recover if we can

                    GameObject go = new GameObject("ScienceAlert.ExperimentManager");
                    return go.AddComponent<ExperimentManagerMockup>();
                } else return _instance;
            }

            private set
            {
                _instance = value;
            }
        }

        private static ExperimentManagerMockup _instance = null;
        #endregion
    }
}
