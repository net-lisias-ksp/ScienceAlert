namespace ScienceAlert
{
    public enum CrossContextKeys
    {
        CoreContextView, // GameObject
        VesselContextView, // GameObject

        GameData, // IDirectory

        CareerScienceGainMultiplier, // float
        HomeWorld, // ICelestialBody

        AlertSound, // PlayableSound
        MinSoundDelay, // float

        DefaultOnboardRule, // ConfigNode
        DefaultAvailabilityRule, // ConfigNode
        DefaultConditionRule, // ConfigNode

        SensorRuleTypes, // ReadOnlyCollection<Type>
    }
}
