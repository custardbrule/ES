namespace RequestValidatior
{
    public static class ValidatorHelper
    {
        public static bool IsValidTimeZone(string id) => !string.IsNullOrWhiteSpace(id) && TimeZoneInfo.TryFindSystemTimeZoneById(id, out _);
    }
}
