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
        private readonly Dictionary<string, (object validator, object compiledSelector)> _propValidators = [];
        private readonly Dictionary<string, List<string>> _errors = [];
        public IReadOnlyDictionary<string, List<string>> Errors => _errors;
        public bool IsValid => _errors.Count == 0;

        public PropValidator<TProp> RuleFor<TProp>(Expression<Func<TSource, TProp>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);

            if (_propValidators.TryGetValue(propertyName, out var existing))
                return (PropValidator<TProp>)existing.validator;

            var validator = new PropValidator<TProp>();
            var compiledSelector = propertyExpression.Compile();

            _propValidators[propertyName] = (validator, compiledSelector);

            return validator;
        }

        private void ValidateProperty<TProp>(string propertyName, PropValidator<TProp> validator, object compiledSelector, TSource source)
        {
            validator.ClearErrors();

            var compiled = (Func<TSource, TProp>)compiledSelector;
            var propValue = compiled(source);

            validator.Validate(propValue);

            if (!validator.IsValid)
                _errors[propertyName] = [.. validator.Errors];
        }

        public bool Validate(TSource source)
        {
            _errors.Clear();

            foreach (var (propertyName, (validator, compiledSelector)) in _propValidators)
            {
                var validatorType = validator.GetType();
                var propType = validatorType.GetGenericArguments()[0];

                var validatePropertyMethod = GetType()
                    .GetMethod(nameof(ValidateProperty), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(propType);

                validatePropertyMethod.Invoke(this, [propertyName, validator, compiledSelector, source]);
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
