namespace RequestValidatior
{
    public class ValidationError : Exception
    {
        public string Type => "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        public string Title => "One or more validation errors occurred.";
        public int Status => 400;
        public string TraceId => Guid.NewGuid().ToString();
        public Dictionary<string, List<string>> Errors { get; set; } = [];

        public ValidationError(params KeyValuePair<string, List<string>>[] errors) => Errors = new Dictionary<string, List<string>>(errors);
    }
}
