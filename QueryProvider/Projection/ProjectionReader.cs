using System;
using System.Collections;
using System.Collections.Generic;

namespace QueryProvider.Projection
{
    internal class ProjectionReader<TIn, TOut> : IEnumerable<TOut>, IEnumerable
    {
        private IEnumerable<TIn> _data;
        private IEnumerable<TOut> _projected;
        private Func<TIn, TOut> _projector;

        internal ProjectionReader(IEnumerable<TIn> data, Func<TIn, TOut> projector)
        {
            _data = data;
            _projector = projector;
            _projected = Project();
        }

        public IEnumerator<TOut> GetEnumerator() => _projected.GetEnumerator();

        private IEnumerable<TOut> Project()
        {
            foreach(var item in _data)
            {
                yield return _projector(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
