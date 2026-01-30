using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces.Repositories;
using FluxoCaixa.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FluxoCaixa.Infra.Data.Repositories
{
    public class LancamentoRepository : Repository<Lancamento>, ILancamentoRepository
    {
        public LancamentoRepository(FluxoCaixaContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(Expression<Func<Lancamento, bool>> where)
        {
            return await _dbSet.Where(where)
                               .ToListAsync();
        }
    }
}
