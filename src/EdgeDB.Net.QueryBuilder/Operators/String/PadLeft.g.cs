using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class StringPadLeft : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "str_pad_start({0}, {1}, {2?})";
    }
}
