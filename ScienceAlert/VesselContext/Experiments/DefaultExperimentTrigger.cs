using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using strange.extensions.promise.api;

namespace ScienceAlert.VesselContext.Experiments
{
    public class DefaultExperimentTrigger : IExperimentTrigger
    {
        public IPromise Deploy()
        {
            throw new NotImplementedException();
        }

        public bool IsBusy
        {
            get { throw new NotImplementedException(); }
        }
    }
}
