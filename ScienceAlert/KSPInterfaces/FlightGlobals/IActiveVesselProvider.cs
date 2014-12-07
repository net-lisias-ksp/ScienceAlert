namespace ScienceAlert.KSPInterfaces.FlightGlobals
{
    public interface IActiveVesselProvider
    {
        Maybe<IVessel> GetActiveVessel();
    }
}
