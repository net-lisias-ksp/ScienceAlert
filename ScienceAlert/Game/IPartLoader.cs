using ReeperCommon.Containers;

namespace ScienceAlert.Game
{
    public interface IPartLoader
    {
        Maybe<IPart> GetPartByName(string name);
    }
}
