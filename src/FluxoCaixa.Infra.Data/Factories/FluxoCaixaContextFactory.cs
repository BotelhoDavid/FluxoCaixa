using FluxoCaixa.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FluxoCaixa.Infra.Data.Factories
{
    /// <summary>
    /// Factory para criação do DbContext durante migrations
    /// </summary>
    public class FluxoCaixaContextFactory : IDesignTimeDbContextFactory<FluxoCaixaContext>
    {
        public FluxoCaixaContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FluxoCaixaContext>();
            
            // Usar connection string do ambiente
            var connectionString = FluxoCaixaContext.GetConnectionStringFromEnvironment();
            
            optionsBuilder.UseSqlServer(connectionString);

            return new FluxoCaixaContext(optionsBuilder.Options);
        }
    }
}
