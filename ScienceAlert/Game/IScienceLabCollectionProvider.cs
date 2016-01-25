using System.Collections.ObjectModel;

namespace ScienceAlert.Game
{
    public interface IScienceLabCollectionProvider
    {
        ReadOnlyCollection<IScienceLab> Labs { get; }
    }
}
