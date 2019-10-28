using System.Linq.Expressions;
using System.Text;

namespace QueryProvider.Projection
{
    internal class ColumnProjector : ExpressionVisitor
    {
        StringBuilder _sb;    

        internal ColumnProjection ProjectColumns(LambdaExpression expression)
        {
            _sb = new StringBuilder();
            Expression selector = this.Visit(expression.Body);
            return new ColumnProjection()
            {
                Columns = _sb.ToString(),
                Selector = expression
            };
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                if (_sb.Length > 0)
                {
                    _sb.Append(", ");
                }
                _sb.Append(m.Member.Name);
                return m;
            }
            else
            {
                return base.VisitMember(m);
            }
        }
    }
}
