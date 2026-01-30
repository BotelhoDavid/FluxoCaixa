using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections;

namespace FluxoCaixa.Infra.Data.Context
{
    public class FluxoCaixaContext : DbContext
    {
        public FluxoCaixaContext() { }

        public FluxoCaixaContext(DbContextOptions<FluxoCaixaContext> options) : base(options)
        {
        }

        public DbSet<Lancamento> Lancamentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(FluxoCaixaContext).Assembly
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot _configuracaoBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                                                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                                                                                    .AddJsonFile($"appsettings.json", optional: true, reloadOnChange: false)
                                                                                    .Build();

                optionsBuilder.UseSqlServer(GetConnectionStringFromEnvironment())
                              .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

                optionsBuilder.EnableSensitiveDataLogging();
            }
        }

        public static string GetConnectionStringFromEnvironment()
        {
            IDictionary _envVars = Environment.GetEnvironmentVariables();
            string _dataSource = _envVars["DB_DATA_SOURCE"]?.ToString() ?? "";
            string _dataBase = _envVars["DB_CATALOG"]?.ToString() ?? "";
            string _user = _envVars["DB_DATABASE_USER"]?.ToString() ?? "";
            string _password = _envVars["DB_DATABASE_USER_PASSWORD"]?.ToString() ?? "";

            SqlConnectionStringBuilder _connectionStringBuilder = new SqlConnectionStringBuilder();

            _connectionStringBuilder.DataSource = _dataSource;
            _connectionStringBuilder.InitialCatalog = _dataBase;
            _connectionStringBuilder.IntegratedSecurity = true;
            _connectionStringBuilder.PersistSecurityInfo = false;
            _connectionStringBuilder.UserID = _user;
            _connectionStringBuilder.Password = _password;
            _connectionStringBuilder.MultipleActiveResultSets = false;
            _connectionStringBuilder.Encrypt = false;
            _connectionStringBuilder.TrustServerCertificate = false;
            _connectionStringBuilder.Add(keyword: "Trusted_Connection", value: false);
            _connectionStringBuilder.Pooling = true;
            _connectionStringBuilder.MaxPoolSize = 5000;

            return _connectionStringBuilder.ConnectionString;
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, 
                                                   CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            foreach (var entry in ChangeTracker.Entries<Entity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.DataCriacao = DateTime.UtcNow;
                    entry.Entity.Deletado = false;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.DataModificacao = DateTime.UtcNow;
                }
            }
        }
    }
}
