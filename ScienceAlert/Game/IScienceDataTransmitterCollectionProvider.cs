using System.Collections.ObjectModel;

namespace ScienceAlert.Game
{
    public interface IScienceDataTransmitterCollectionProvider
    {
        ReadOnlyCollection<IScienceDataTransmitter> Transmitters { get; }
    }
}
