namespace Domain.User.UserRoot
{
    public class UserConstants
    {
        public const string Base = nameof(User);
        public const string ESIndex = "es_user";
        public static string GetId(Guid id) => $"{id:N}";
    }
}
