using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class SetsDistinct : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "distinct {0}";
    }
}
