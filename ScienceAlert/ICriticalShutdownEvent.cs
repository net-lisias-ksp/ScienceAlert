using System;

namespace ScienceAlert
{
    interface ICriticalShutdownEvent
    {
        IDisposable Subscribe(Action action);
        void Unsubscribe(Action action);
        void Dispatch();
    }
}