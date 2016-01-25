using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ScienceAlert.Game
{
    public interface IScienceExperimentModuleCollectionProvider
    {
        ReadOnlyCollection<IModuleScienceExperiment> ScienceExperimentModules { get; }
    }
}
