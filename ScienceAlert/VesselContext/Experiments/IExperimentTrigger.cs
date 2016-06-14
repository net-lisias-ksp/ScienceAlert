using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using strange.extensions.promise.api;

namespace ScienceAlert.VesselContext.Experiments
{
    public interface IExperimentTrigger
    {
        IPromise Deploy();
        bool IsBusy { get; }
    }
}
