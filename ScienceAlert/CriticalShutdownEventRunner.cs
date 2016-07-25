using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using ReeperCommon.Utilities;
using UnityEngine;

namespace ScienceAlert
{
    // Used in case something has blown up. All contexts will be untrustworthy (so dispatching signals is likely to fail), so
    // this alternative mechanism is used for code that absolutely must run when shutting down if something broke
    class CriticalShutdownEventRunner : ICriticalShutdownEvent
    {
        private static readonly List<Action> Subscribers = new List<Action>();
        private static Coroutine _shutdownRoutine;

        private class ShutdownEventSubscription : IDisposable
        {
            private readonly ICriticalShutdownEvent _owner;
            private readonly Action _subscriber;

            public ShutdownEventSubscription([NotNull] ICriticalShutdownEvent owner, [NotNull] Action subscriber)
            {
                if (owner == null) throw new ArgumentNullException("owner");
                if (subscriber == null) throw new ArgumentNullException("subscriber");
                _owner = owner;
                _subscriber = subscriber;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }


            private void Dispose(bool disposing)
            {
                if (disposing)
                    _owner.Unsubscribe(_subscriber);
            }
        }



        public IDisposable Subscribe([NotNull] Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            Subscribers.Add(action);

            return new ShutdownEventSubscription(this, action);
        }


        public void Unsubscribe([NotNull] Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            Subscribers.Remove(action);
        }


        public void Dispatch()
        {
            if (_shutdownRoutine != null) return;
            _shutdownRoutine = CoroutineHoster.Instance.StartCoroutine(Shutdown());
        }


        private static IEnumerator Shutdown()
        {
            yield return new WaitForEndOfFrame();

            foreach (var item in Subscribers)
            {
                try
                {
                    item();
                }
                catch (Exception e)
                {
                    Log.Error("Exception thrown inside shutdown handler: " + e);
                }
            }

            Subscribers.Clear();
            _shutdownRoutine = null;
        }
    }
}
