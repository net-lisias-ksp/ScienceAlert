using ScienceAlert.Annotations;

namespace ScienceAlert
{
    public delegate void SensorTriggeredDelegate();

    public interface ISensor
    {
        event SensorTriggeredDelegate OnTriggered;    
        void Poll();
    }
}
