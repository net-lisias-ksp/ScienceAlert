using System.Collections;
using UnityEngine;

namespace ScienceAlert
{
    public interface IRoutineRunner
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
        void StopCoroutine(Coroutine routine);
    }
}
