namespace FluxoCaixa.Domain.Interfaces.Repositories
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        Task<TEntity> CreateAsync(TEntity model);
        IQueryable<TEntity> Query();
    }
}
