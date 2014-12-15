namespace ScienceAlert.ReeperCommon.Window
{
    public interface IWindowLogic
    {
        void SetWindow(BasicWindow window);
        void OnWindowAssigned();
        void OnWindowUnassigned();

        void OnGUI();
        void Update();
    }
}
