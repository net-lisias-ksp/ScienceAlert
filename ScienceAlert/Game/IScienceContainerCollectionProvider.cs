using System.Collections.ObjectModel;

namespace ScienceAlert.Game
{
    public interface IScienceContainerCollectionProvider
    {
        ReadOnlyCollection<IScienceDataContainer> Containers { get; }
    }
}
