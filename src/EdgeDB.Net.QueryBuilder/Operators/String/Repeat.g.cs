using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringRepeat : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "str_repeat({0}, {1})";
    }
}
