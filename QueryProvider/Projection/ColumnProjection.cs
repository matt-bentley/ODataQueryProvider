using System.Linq.Expressions;

namespace QueryProvider.Projection
{
    internal class ColumnProjection
    {
        internal string Columns;
        internal LambdaExpression Selector;
    }
}
