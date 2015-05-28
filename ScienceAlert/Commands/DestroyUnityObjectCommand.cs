using System;
using ScienceAlert.Annotations;
using UnityEngine;

namespace ScienceAlert.Commands
{
    public class DestroyUnityObjectCommand<T> : ICommand where T: UnityEngine.Object
    {
        private readonly T _target;

        public DestroyUnityObjectCommand([NotNull] T target)
        {
            if (target == null) throw new ArgumentNullException("target");
            _target = target;
        }


        public void Execute()
        {
            if (_target == null) // Unity overloads this operator 
                throw new InvalidOperationException("Can't destroy target; target has already been destroyed");

            UnityEngine.Object.Destroy(_target);
        }
    }
}
