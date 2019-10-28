using System.Linq.Expressions;

namespace QueryProvider.Translation
{
    internal class TranslateResult
    {
        internal string QueryString;
        internal LambdaExpression Projector;
    }
}
