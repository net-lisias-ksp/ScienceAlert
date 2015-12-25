namespace ScienceAlert.Game
{
    public interface IGameFactory
    {
        IVessel Create(Vessel vessel);
        IModuleScienceExperiment Create(ModuleScienceExperiment mse);
        IPart Create(Part part);
    }
}
