namespace ScienceAlert.VesselContext
{
    struct SensorStatusChange
    {
        public SensorState NewState;
        public SensorState OldState;

        public SensorStatusChange(SensorState newState, SensorState oldState) : this()
        {
            NewState = newState;
            OldState = oldState;
        }
    }
}
