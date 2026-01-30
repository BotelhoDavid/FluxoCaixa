using FluxoCaixa.Domain.Entities;
using System.Linq.Expressions;

namespace FluxoCaixa.Domain.Interfaces.Repositories
{
    public interface ILancamentoRepository : IRepository<Lancamento>
    {
        Task<IEnumerable<Lancamento>> ObterPorPeriodoAsync(Expression<Func<Lancamento, bool>> where);
    }
}
