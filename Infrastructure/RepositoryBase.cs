using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public abstract class RepositoryBase
    {
        public static void SetPropertyValue(Expression expression, object value)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            object propertyOwner = null;

            if (expression is MemberExpression)
            {
                var memberExpression = (MemberExpression)expression;
                var propertyInfo = (PropertyInfo)memberExpression.Member;

                if (memberExpression.Expression == null) // Occurs when it's a static property
                {
                    propertyInfo.SetValue(expression, value);
                }
                else
                {
                    var propertyOwnerExpression = (MemberExpression)memberExpression.Expression;
                    var delegateExpr = Expression.Lambda(propertyOwnerExpression).Compile();

                    if (delegateExpr == null) throw new NullReferenceException();
 
                    propertyOwner = delegateExpr.DynamicInvoke();
                    propertyInfo.SetValue(propertyOwner, value);
                }
            }

            if (expression is UnaryExpression)
            {
                // Property, field or method returning a value type, usually a conversion
                var unaryExpression = (UnaryExpression)expression;
                if (unaryExpression.Operand is MethodCallExpression)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var memberExpression = (MemberExpression)unaryExpression.Operand;
                    var propertyInfo = (PropertyInfo)memberExpression.Member;
                    var propertyOwnerExpression = (MemberExpression)memberExpression.Expression;
                    propertyOwner = Expression.Lambda(propertyOwnerExpression).Compile().DynamicInvoke();
                    propertyInfo.SetValue(propertyOwner, value, null);
                }
            }
        }

        protected static void SetPropertyValue<T>(Expression<Func<T>> expression, object value)
        {
            SetPropertyValue(expression.Body, value);
        }

        protected static void SetPropertyValue<T>(Expression<Action<T>> expression, object value)
        {
            SetPropertyValue(expression.Body, value);
        }


    }
}
