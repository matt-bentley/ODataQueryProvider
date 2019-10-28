﻿using QueryProvider.Interfaces;
using QueryProvider.Provider;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryProvider
{
    public class ODataQuery<T> : IQueryable<T>
    {
        private readonly ODataQueryProvider<T> _provider;
        private readonly Expression _expression;

        public ODataQuery(IODataClient<T> client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            this._provider = new ODataQueryProvider<T>(client);
            this._expression = Expression.Constant(this);
        }

        internal ODataQuery(ODataQueryProvider<T> provider, Expression expression)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            if (expression == null)
                throw new ArgumentNullException("expression");

            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
                throw new ArgumentOutOfRangeException("expression");

            this._provider = provider;
            this._expression = expression;
        }

        Expression IQueryable.Expression
        {
            get { return this._expression; }
        }

        Type IQueryable.ElementType
        {
            get { return typeof(T); }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this._provider; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this._provider.Execute(this._expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this._provider.Execute(this._expression)).GetEnumerator();
        }

        public override string ToString()
        {
            return this._provider.GetQueryText(this._expression);
        }
    }
}
