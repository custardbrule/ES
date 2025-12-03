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
        private readonly Dictionary<string, Action<TSource>> _validationActions = [];
        private readonly Dictionary<string, List<string>> _errors = [];
        public IReadOnlyDictionary<string, List<string>> Errors => _errors;
        public bool IsValid => _errors.Count == 0;

        public PropValidator<TProp> RuleFor<TProp>(Expression<Func<TSource, TProp>> propertyExpression)
        {
            var propertyName = GetPropertyName(propertyExpression);

            if (_validationActions.ContainsKey(propertyName))
            {
                // Property already registered, find and return the validator
                // We need to store validators separately for retrieval
                throw new InvalidOperationException($"Property {propertyName} is already registered.");
            }

            var validator = new PropValidator<TProp>();
            var compiledSelector = propertyExpression.Compile();

            // Create a validation action that captures the validator and selector
            _validationActions[propertyName] = source =>
            {
                validator.ClearErrors();
                var propValue = compiledSelector(source);
                validator.Validate(propValue);

                if (!validator.IsValid)
                    _errors[propertyName] = [.. validator.Errors];
            };

            return validator;
        }

        public bool Validate(TSource source)
        {
            _errors.Clear();

            foreach (var validationAction in _validationActions.Values)
                validationAction(source);

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
