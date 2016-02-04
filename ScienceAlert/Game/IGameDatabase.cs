namespace ScienceAlert.Game
{
    public interface IGameDatabase
    {
        IUrlConfig[] GetConfigs(string nodeName);
    }
}
