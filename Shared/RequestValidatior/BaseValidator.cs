using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RequestValidatior
{
    public class PropValidator<TProp>
    {
        private readonly List<(Expression<Func<TProp, bool>> expression, string message)> _predicates = [];
        private readonly List<string> _errors = [];
        public IReadOnlyList<string> Errors => _errors;
        public bool HasErrors => _errors.Count > 0;

        public PropValidator<TProp> With(Expression<Func<TProp, bool>> expression, string message = "Undefined!")
        {
            _predicates.Add((expression, message));
            return this;
        }

        internal PropValidator<TProp> Validate(TProp propValue)
        {
            foreach (var (expression, message) in _predicates)
            {
                if (!expression.Compile()(propValue))
                    _errors.Add(message);
            }

            return this;
        }

        internal void ClearErrors() => _errors.Clear();
    }

    public abstract class BaseValidator<TSource>
    {
        private readonly Dictionary<string, object> _propValidators = [];
        private readonly Dictionary<string, List<string>> _errors = [];

        public PropValidator<TProp> RuleFor<TProp>(Expression<Func<TSource, TProp>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);

            if (!_propValidators.ContainsKey(propertyName))
                _propValidators[propertyName] = new PropValidator<TProp>();

            return (PropValidator<TProp>)_propValidators[propertyName];
        }

        public bool Validate(TSource source)
        {
            _errors.Clear();

            foreach (var kvp in _propValidators)
            {
                var propertyName = kvp.Key;
                var validatorType = kvp.Value.GetType();
                var propType = validatorType.GetGenericArguments()[0];

                var property = typeof(TSource).GetProperty(propertyName);
                if (property == null) continue;

                // Clear previous errors
                var clearMethod = validatorType.GetMethod(nameof(PropValidator<object>.ClearErrors), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                clearMethod?.Invoke(kvp.Value, null);

                var propValue = property.GetValue(source);
                var validateMethod = validatorType.GetMethod(nameof(PropValidator<object>.Validate), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                validateMethod?.Invoke(kvp.Value, [propValue]);

                var errorsProperty = validatorType.GetProperty(nameof(PropValidator<object>.Errors));
                var errors = (IReadOnlyList<string>)errorsProperty?.GetValue(kvp.Value)!;

                if (errors?.Count > 0)
                    _errors[propertyName] = [.. errors];
            }

            return _errors.Count == 0;
        }

        public IReadOnlyDictionary<string, List<string>> GetErrors() => _errors;

        private static string GetPropertyName<TProp>(Expression<Func<TSource, TProp>> expression)
        {
            if (expression.Body is MemberExpression memberExpression) return memberExpression.Member.Name;

            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand) return operand.Member.Name;

            throw new ArgumentException("Invalid property expression");
        }
    }
}
