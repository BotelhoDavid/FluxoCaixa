using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces.Repositories;
using FluxoCaixa.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FluxoCaixa.Infra.Data.Repositories
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        public UsuarioRepository(FluxoCaixaContext context) : base(context)
        {
        }

        public async Task<Usuario> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var emailNormalizado = email.ToLowerInvariant().Trim();
            return await _dbSet.FirstOrDefaultAsync(x => x.Email == emailNormalizado);
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var emailNormalizado = email.ToLowerInvariant().Trim();
            return await _dbSet.AnyAsync(x => x.Email == emailNormalizado);
        }
    }
}

