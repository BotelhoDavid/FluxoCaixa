using FluxoCaixa.Domain.Entities;

namespace FluxoCaixa.Domain.Interfaces.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario> GetByEmailAsync(string email);
        Task<bool> ExisteEmailAsync(string email);
    }
}

