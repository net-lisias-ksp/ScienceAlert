namespace ScienceAlert.Core
{
    public enum CoreContextKeys
    {
        CoreContextView, // GameObject
        VesselContextView, // GameObject

        CoreContextShutdownEventSubscription, // IDisposable

        GameData, // IDirectory

        CareerScienceGainMultiplier, // float
        HomeWorld, // ICelestialBody


        SoundConfig, // ConfigNode

        AlertSound, // AudioClip
    }
}
