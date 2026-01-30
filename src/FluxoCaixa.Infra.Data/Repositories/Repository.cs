using FluxoCaixa.Domain.Interfaces.Repositories;
using FluxoCaixa.Domain.Models;
using FluxoCaixa.Infra.Data.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FluxoCaixa.Infra.Data.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly FluxoCaixaContext _context;

        protected DbSet<TEntity> _dbSet
        {
            get
            {
                return _context.Set<TEntity>();
            }
        }

        public Repository(FluxoCaixaContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<TEntity> CreateAsync(TEntity model)
        {
            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));

                await _dbSet.AddAsync(model);
                return model;
            }
            catch (Exception)
            {
                throw new ApiException(message: "Erro ao acessar base de dados",
                                       statusCode: HttpStatusCode.InternalServerError);
            }

        }

        public IQueryable<TEntity> Query()
        {
            try
            {
                return _dbSet.AsQueryable();
            }
            catch (Exception)
            {
                throw new ApiException(message: "Erro ao acessar base de dados",
                                       statusCode: HttpStatusCode.InternalServerError);
            }
        }

        public void Dispose()
        {
            try
            {
                if (_context != null)
                    _context.Dispose();
                GC.SuppressFinalize(this);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
