using System;
using strange.extensions.mediation.impl;

namespace ScienceAlert.UI
{
    [Serializable]
    class ManualRegistrationView : View
    {
        public override bool autoRegisterWithContext
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }
    }
}
