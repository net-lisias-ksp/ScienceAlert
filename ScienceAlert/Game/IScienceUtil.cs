namespace ScienceAlert.Game
{
    public interface IScienceUtil
    {
        bool RequiredUsageInternalAvailable(IVessel vessel, IPart part, ExperimentUsageReqs reqs);
    }
}
