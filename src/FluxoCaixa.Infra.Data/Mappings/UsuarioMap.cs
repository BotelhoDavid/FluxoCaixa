using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FluxoCaixa.Infra.Data.Mappings
{
    internal class UsuarioMap : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios");

            builder.HasKey(usuario => usuario.Id);

            builder.Property(usuario => usuario.Nome)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(usuario => usuario.Email)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(usuario => usuario.Email)
                .IsUnique();

            // Configurar Value Object SenhaHash
            builder.OwnsOne(usuario => usuario.SenhaHash, senha =>
            {
                senha.Property(s => s.Hash)
                    .HasColumnName("SenhaHash")
                    .IsRequired()
                    .HasMaxLength(512);
            });

            builder.Property(usuario => usuario.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(usuario => usuario.DataUltimoAcesso)
                .IsRequired(false);

            builder.Property(usuario => usuario.DataCriacao)
                .IsRequired();

            builder.Property(usuario => usuario.DataModificacao)
                .IsRequired(false);

            builder.Property(usuario => usuario.Deletado)
                .IsRequired()
                .HasDefaultValue(false);

        }
    }
}
