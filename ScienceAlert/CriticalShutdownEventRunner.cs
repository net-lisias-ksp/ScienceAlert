using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using ReeperCommon.Logging;
using ReeperCommon.Utilities;
using UnityEngine;

namespace ScienceAlert
{
    class CriticalShutdownEventRunner : ICriticalShutdownEvent
    {
        private static readonly List<Action> _subscribers = new List<Action>();
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

            _subscribers.Add(action);

            return new ShutdownEventSubscription(this, action);
        }


        public void Unsubscribe([NotNull] Action action)
        {
            if (action == null) throw new ArgumentNullException("action");

            _subscribers.Remove(action);
        }


        public void Dispatch()
        {
            if (_shutdownRoutine != null) return;
            _shutdownRoutine = CoroutineHoster.Instance.StartCoroutine(Shutdown());
        }


        private static IEnumerator Shutdown()
        {
            yield return new WaitForEndOfFrame();

            foreach (var item in _subscribers)
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

            _subscribers.Clear();
            _shutdownRoutine = null;
        }
    }
}
