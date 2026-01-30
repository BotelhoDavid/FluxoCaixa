namespace FluxoCaixa.Domain.Interfaces.UoW
{
    public interface IUnitOfWork
    {
        Task<bool> CommitAsync();
    }
}
