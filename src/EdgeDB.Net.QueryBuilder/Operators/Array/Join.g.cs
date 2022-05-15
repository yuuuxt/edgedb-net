using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class ArrayJoin : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "array_join({0}, {1})";
    }
}
