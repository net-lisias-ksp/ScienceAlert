using System.Collections.ObjectModel;

namespace ScienceAlert.Game
{
    public interface IScienceExperimentModuleCollectionProvider
    {
        ReadOnlyCollection<IModuleScienceExperiment> ScienceExperimentModules { get; }
    }
}
