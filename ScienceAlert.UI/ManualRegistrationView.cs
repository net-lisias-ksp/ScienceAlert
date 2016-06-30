using System;
using System.Collections;
using ReeperCommon.Containers;
using ReeperCommon.Logging;
using strange.extensions.mediation.impl;
using strange.extensions.signal.impl;
using UnityEngine;

namespace ScienceAlert.UI
{
    /// <summary>
    /// Normally, Views search for an associated context by looking at their GameObject hierarchy and searching upwards. Because
    /// these views will be integrated into the game's UI hierarchy and so won't be able to find any contexts that way, they'll 
    /// need to be wired up manually to the correct one when they're created. 
    /// 
    /// This little subclass simply disables the auto bubble logic that's normally present
    /// </summary>
    [Serializable]
    public class ManualRegistrationView : View
    {
        public override bool autoRegisterWithContext
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }
    }
}
