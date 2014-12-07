using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScienceAlert
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object source)
        {
            return ReferenceEquals(source, null);
        }

        public static bool IsSameAs(this object source, object other)
        {
            return ReferenceEquals(source, other);
        }
    }

    public struct Maybe<T> : IEnumerable<T>
    {
        private IEnumerable<T> _values;

        public static Maybe<T> None { get { return new Maybe<T>(); } }

        public static Maybe<T> With(T value)
        {
            var maybe = new Maybe<T>();
            maybe._values = new T[] { value };
            return maybe;
        }

        public IEnumerator<T> GetEnumerator()
        {
            LazyInitialize();

            return _values.GetEnumerator();
        }

        private void LazyInitialize()
        {
            if (_values.IsNull())
                _values = new T[] { };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
