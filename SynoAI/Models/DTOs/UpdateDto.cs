using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System;

namespace SynoAI.Models.DTOs
{
    public abstract class UpdateDto<T>
    {
        public List<string> ChangedProperties = new List<string>();

        protected void NotifyPropertyChange([CallerMemberName] string propertyName = "")
        {
            ChangedProperties.Add(propertyName);
        }

        public bool HasChanged(Expression<Func<T, object>> expression)
        {
            if (expression.Body is not MemberExpression body)
            {
                body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }
            return HasChanged(body.Member.Name);
        }

        public bool HasChanged(string propertyName)
        {
            return ChangedProperties.Contains(propertyName);
        }
    }
}
