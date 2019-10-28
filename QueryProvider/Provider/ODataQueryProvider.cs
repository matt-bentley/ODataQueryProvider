using QueryProvider.Helpers;
using QueryProvider.Interfaces;
using QueryProvider.Projection;
using QueryProvider.Translation;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryProvider.Provider
{
    internal class ODataQueryProvider<T> : Abstract.QueryProvider
    {
        private readonly IODataClient<T> _client;

        internal ODataQueryProvider(IODataClient<T> client)
        {
            _client = client;
        }

        public override string GetQueryText(Expression expression)
        {
            return this.Translate(expression).QueryString;
        }

        public override object Execute(Expression expression)
        {
            var result = this.Translate(expression);
            var data = _client.GetData(result.QueryString);
            if (result.Projector != null)
            {
                Type elementType = ExpressionTypeSystem.GetElementType(expression.Type);
                var projector = result.Projector.Compile();
                return Activator.CreateInstance(
                    typeof(ProjectionReader<,>).MakeGenericType(typeof(T), elementType),
                    BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new object[] { data, projector },
                    null
                    );
            }
            else
            {
                return data;
            }
        }

        private TranslateResult Translate(Expression expression)
        {
            return new ODataQueryTranslator().Translate(expression);
        }
    }
}
