using System.Linq.Expressions;

namespace EdgeDB.Operators
{
    internal class TypesIs : IEdgeQLOperator
    {
        public ExpressionType? ExpressionType => null;
        public string EdgeQLOperator => "{0} is {1}";
    }
}
