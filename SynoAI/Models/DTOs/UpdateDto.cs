using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace SynoAI.Models.DTOs
{
    /// <summary>
    /// Class for update DTOs
    /// </summary>
    public abstract class UpdateDto<T>
    {
        /// <summary>
        /// List for ChangedProperties
        /// </summary>
        public List<string> ChangedProperties = new();
        /// <summary>
        /// Notify on change of properties
        /// </summary>
        protected void NotifyPropertyChange([CallerMemberName] string propertyName = "")
        {
            ChangedProperties.Add(propertyName);
        }
        /// <summary>
        /// Boolean to indicate something has changed
        /// </summary>
        public bool HasChanged(Expression<Func<T, object>> expression)
        {
            if (expression.Body is not MemberExpression body)
            {
                body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }
            return HasChanged(body.Member.Name);
        }
        /// <summary>
        /// Boolean to indicate something with changed with the property that changed
        /// </summary>
        public bool HasChanged(string propertyName)
        {
            return ChangedProperties.Contains(propertyName);
        }
    }
}
