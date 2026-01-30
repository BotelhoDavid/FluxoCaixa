using FluxoCaixa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluxoCaixa.Infra.Data.Mappings
{
    public class LancamentoMap : IEntityTypeConfiguration<Lancamento>
    {
        public void Configure(EntityTypeBuilder<Lancamento> builder)
        {
            builder.HasIndex(lancamento => lancamento.Id);

            builder.HasKey(lancamento => lancamento.Id);

            builder.Property(lancamento => lancamento.Valor)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
        }
    }
}