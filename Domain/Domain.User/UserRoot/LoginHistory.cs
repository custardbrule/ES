namespace Domain.User.UserRoot
{
    public record LoginHistory(Guid Id, Guid UserId, DateTimeOffset LoginAt, string IpAddress, string UserAgent);
}
