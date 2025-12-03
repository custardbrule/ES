using System.Linq.Expressions;

namespace RequestValidatior
{
    public sealed class PropValidator<TProp>
    {
        private readonly List<(Expression<Func<TProp, bool>> expression, string message)> _predicates = [];
        private readonly List<string> _errors = [];
        public IReadOnlyList<string> Errors => _errors;
        public bool IsValid => _errors.Count == 0;

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
        public IReadOnlyDictionary<string, List<string>> Errors => _errors;
        public bool IsValid => _errors.Count == 0;

        public PropValidator<TProp> RuleFor<TProp>(Expression<Func<TSource, TProp>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);

            if (!_propValidators.ContainsKey(propertyName))
                _propValidators[propertyName] = new PropValidator<TProp>();

            return (PropValidator<TProp>)_propValidators[propertyName];
        }

        private void ValidateProperty<TProp>(string propertyName, PropValidator<TProp> validator, TSource source)
        {
            var property = typeof(TSource).GetProperty(propertyName);
            if (property == null) return;

            validator.ClearErrors();

            var propValue = (TProp)property.GetValue(source)!;
            validator.Validate(propValue);

            if (validator.IsValid)
                _errors[propertyName] = [.. validator.Errors];
        }

        public bool Validate(TSource source)
        {
            _errors.Clear();

            foreach (var kvp in _propValidators)
            {
                var propertyName = kvp.Key;
                var validatorType = kvp.Value.GetType();
                var propType = validatorType.GetGenericArguments()[0];

                var validatePropertyMethod = GetType()
                    .GetMethod(nameof(ValidateProperty), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(propType);

                validatePropertyMethod.Invoke(this, [propertyName, kvp.Value, source]);
            }

            return _errors.Count == 0;
        }

        private static string GetPropertyName<TProp>(Expression<Func<TSource, TProp>> expression)
        {
            if (expression.Body is MemberExpression memberExpression) return memberExpression.Member.Name;

            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand) return operand.Member.Name;

            throw new ArgumentException("Invalid property expression");
        }
    }
}
