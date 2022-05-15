using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TemporalAdd : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => System.Linq.Expressions.ExpressionType.Add;
        public string EdgeQLOperator => "{0} + {1}";
    }
}
