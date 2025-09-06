namespace Seed
{
    public interface IAuditEntity
    {
        DateTimeOffset CreatedDate { get; }
        DateTimeOffset LastModifiedDate { get; }
    }
    public abstract record BaseEntity<T>(T Id) where T : IComparable<T>;
    public abstract record BaseAuditEntity<T>(T Id, DateTimeOffset CreatedDate, DateTimeOffset LastModifiedDate) : BaseEntity<T>(Id), IAuditEntity where T : IComparable<T>;
}
