using System;
using System.Collections;
using strange.extensions.context.api;
using strange.extensions.implicitBind;
using strange.extensions.injector;
using strange.extensions.injector.api;
using UnityEngine;

namespace ScienceAlert
{
// ReSharper disable once UnusedMember.Global
    public class CoroutineRunner : ICoroutineRunner
    {
        private readonly GameObject _gameObject;
        private MonoBehaviour _monoBehaviour;


        // ReSharper disable once ClassNeverInstantiated.Local
        private class Runner : MonoBehaviour
        {

        }


        public CoroutineRunner([Name(ContextKeys.CONTEXT_VIEW)] GameObject gameObject)
        {
            if (gameObject == null) throw new ArgumentNullException("gameObject");

            _gameObject = gameObject;
        }


        [PostConstruct]
        // ReSharper disable once UnusedMember.Global
        public void Initialize()
        {
            _monoBehaviour = _gameObject.AddComponent<Runner>();
        }


        public Coroutine StartCoroutine(IEnumerator coroutine)
        {
            if (coroutine == null) throw new ArgumentNullException("coroutine");

            return _monoBehaviour.StartCoroutine(coroutine);
        }

        public void StopCoroutine(Coroutine routine)
        {
            _monoBehaviour.StopCoroutine(routine);
        }
    }
}
