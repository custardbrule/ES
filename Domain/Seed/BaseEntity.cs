namespace Seed
{
    public abstract record BaseEntity<T>(T Id) where T : IComparable<T>;
    public abstract record BaseAuditEntity<T>(T Id, DateTimeOffset CreatedDate) : BaseEntity<T>(Id) where T : IComparable<T>;
}
