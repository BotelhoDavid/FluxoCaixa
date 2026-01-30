using FluxoCaixa.Domain.Interfaces.UoW;
using FluxoCaixa.Domain.Models;
using FluxoCaixa.Infra.Data.Context;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace FluxoCaixa.Infra.Data.Uow
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FluxoCaixaContext _context;

        public UnitOfWork(FluxoCaixaContext context)
        {
            _context = context;
        }

        public async Task<bool> CommitAsync()
        {
            try
            {
                int _commited = await _context.SaveChangesAsync();

                return _commited > 0;

            }
            catch (Exception ex)
            {
                throw new ApiException(message: "Erro ao acessar base de dados",
                                       statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
