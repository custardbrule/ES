namespace Repository
{
    /// <summary>
    /// Predefine read repo requirement
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="T"></typeparam>
    public interface IReadRepository<TKey, T> 
    {
        Task<T> GetAsync(TKey key, CancellationToken cancellation = default);
    }
}
