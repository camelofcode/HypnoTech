using System;
using System.Linq.Expressions;

namespace HypnoTech.Cms.Extensions
{
    public static class ExpressionExtensions
    {
        public static MemberExpression GetMemberExpression(this Expression expression)
        {
            MemberExpression memberExpression;
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    memberExpression = (MemberExpression)expression;
                    break;
                case ExpressionType.Convert:
                    var convertExpression = (UnaryExpression)expression;
                    memberExpression = (MemberExpression)convertExpression.Operand;
                    break;
                default:
                    throw new Exception("Invalid lambda expression");
            }

            return memberExpression;
        }
    }
}