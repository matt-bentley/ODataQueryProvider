using QueryProvider.Helpers;
using QueryProvider.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QueryProvider.Translation
{
    internal class ODataQueryTranslator : ExpressionVisitor
    {
        private StringBuilder _sb;
        private static readonly Dictionary<ExpressionType, string> _expressionTypes;
        private const string ODATA_DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ssZ";
        private ColumnProjection _projection;

        static ODataQueryTranslator()
        {
            _expressionTypes = GetExpressionTypes();
        }

        internal TranslateResult Translate(Expression expression)
        {
            _sb = new StringBuilder();
            this.Visit(expression);
            return new TranslateResult
            {
                QueryString = this._sb.ToString(),
                Projector = _projection == null ? null : _projection.Selector
            };
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                this.Visit(m.Arguments[0]);
                if (m.Method.Name == "Where")
                {
                    var expression = LeafEvaluator.PartialEval(m.Arguments[1]);
                    AddPrefix("filter");
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(expression);
                    this.Visit(lambda.Body);
                    return m;
                }
                else if (m.Method.Name == "Take")
                {
                    AddPrefix("top");
                    this.Visit(m.Arguments[1]);
                    return m;
                }
                else if (m.Method.Name == "Skip")
                {
                    AddPrefix("skip");
                    this.Visit(m.Arguments[1]);
                    return m;
                }
                else if (m.Method.Name == "OrderBy")
                {
                    AddPrefix("orderby");
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    this.Visit(lambda.Body);
                    return m;
                }
                else if (m.Method.Name == "OrderByDescending")
                {
                    AddPrefix("orderby");
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    this.Visit(lambda.Body);
                    _sb.Append(" desc");
                    return m;
                }
                else if (m.Method.Name == "Select")
                {
                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                    ColumnProjection projection = new ColumnProjector().ProjectColumns(lambda);
                    AddPrefix("select");
                    _sb.Append(projection.Columns);
                    this._projection = projection;
                    return m;
                }
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        private void AddPrefix(string name)
        {
            if (_sb.Length > 0)
            {
                _sb.Append('&');
            }
            else
            {
                _sb.Append('?');
            }
            _sb.Append('$');
            _sb.Append(name);
            _sb.Append('=');
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    _sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;

                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }

            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            VisitFilterBinary(b);

            return b;
        }

        private void VisitFilterBinary(BinaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.AndAlso || expression.NodeType == ExpressionType.OrElse)
            {
                // this is a multi expression
                string op = expression.NodeType == ExpressionType.AndAlso ? "and" : "or";
                _sb.Append('(');
                VisitFilterBinary((BinaryExpression)expression.Left);
                _sb.Append(' ');
                _sb.Append(op);
                _sb.Append(' ');
                VisitFilterBinary((BinaryExpression)expression.Right);
                _sb.Append(')');
            }
            else
            {
                VisitFilterProperty(expression);
            }
        }

        private void VisitFilterProperty(BinaryExpression expression)
        {
            MemberExpression compareMember;
            Expression compareValueExpression;
            compareMember = expression.Left as MemberExpression;

            if (compareMember == null)
            {
                compareValueExpression = expression.Left;
                compareMember = expression.Right as MemberExpression;
                if (compareMember == null)
                {
                    throw new NotSupportedException($"Expression '{expression.ToString()}' could not be processed");
                }
            }
            else
            {
                compareValueExpression = expression.Right;
            }

            PropertyInfo propInfo = compareMember.Member as PropertyInfo;
            _sb.Append(propInfo.Name);
            _sb.Append(' ');
            VisitFilterOperator(expression.NodeType);
            _sb.Append(' ');
            this.Visit(compareValueExpression);
        }

        private void VisitFilterOperator(ExpressionType expressionType)
        {
            string op;
            if (_expressionTypes.TryGetValue(expressionType, out op))
            {
                _sb.Append(op);
            }
            else
            {
                throw new NotSupportedException($"Operator: {expressionType.ToString()} is not supported");
            }
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q != null)
            {
                // this is a reference to the original IQueryable, it isn't used
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _sb.Append(((bool)c.Value) ? 1 : 0);
                        break;

                    case TypeCode.DateTime:
                        var date = (DateTime)c.Value;
                        _sb.Append(date.ToString(ODATA_DATETIME_FORMAT));
                        break;

                    case TypeCode.String:
                        _sb.Append("'");
                        _sb.Append(c.Value);
                        _sb.Append("'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));

                    default:
                        _sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }
        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                _sb.Append(m.Member.Name);
                return m;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        private static Dictionary<ExpressionType, string> GetExpressionTypes()
        {
            var expressionTypes = new Dictionary<ExpressionType, string>();
            expressionTypes.Add(ExpressionType.Equal, "eq");
            expressionTypes.Add(ExpressionType.NotEqual, "ne");
            expressionTypes.Add(ExpressionType.GreaterThan, "gt");
            expressionTypes.Add(ExpressionType.LessThan, "lt");
            expressionTypes.Add(ExpressionType.LessThanOrEqual, "le");
            return expressionTypes;
        }
    }
}
