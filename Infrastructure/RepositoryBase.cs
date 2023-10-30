using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public abstract class RepositoryBase
    {

        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
            if (type == null) return Enumerable.Empty<FieldInfo>();

            var flags = BindingFlags.Public | 
                        BindingFlags.Instance | 
                        BindingFlags.NonPublic | 
                        BindingFlags.Static | 
                        BindingFlags.DeclaredOnly;

            return type.GetFields(flags).Concat(GetFields(type.BaseType));
        }

        protected static void SetBackingField(object instance, string backingFieldName, object value)
        {
            var fields = GetFields(instance.GetType());
            var fieldInfo = fields.FirstOrDefault(_ => _.Name == backingFieldName);

            if (fieldInfo != null)
                fieldInfo.SetValue(instance, value);
        }

        private static void SetPropertyValue(Expression expression, object value)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));

            object propertyOwner = null;

            if (expression is MemberExpression)
            {
                var memberExpression = (MemberExpression)expression;
                var propertyInfo = (PropertyInfo)memberExpression.Member;
                var propertyOwnerExpression = (MemberExpression)memberExpression.Expression;
                propertyOwner = Expression.Lambda(propertyOwnerExpression).Compile().DynamicInvoke();
                propertyInfo.SetValue(propertyOwner, value, null);
            }

            if (expression is UnaryExpression)
            {
                // Property, field or method returning a value type
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
