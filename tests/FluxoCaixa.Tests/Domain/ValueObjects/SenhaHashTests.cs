using FluentAssertions;
using FluxoCaixa.Domain.ValueObjects;
using Xunit;

namespace FluxoCaixa.Tests.Domain.ValueObjects
{
    public class SenhaHashTests
    {
        [Fact]
        public void Deve_Criar_SenhaHash_Com_Hash_Valido()
        {
            // Arrange
            var hash = "hash_criptografado_valido";

            // Act
            var senhaHash = new SenhaHash(hash);

            // Assert
            senhaHash.Should().NotBeNull();
            senhaHash.Hash.Should().Be(hash);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void Deve_Lancar_Excecao_Quando_Hash_Vazio_Ou_Nulo(string hash)
        {
            // Act
            Action act = () => new SenhaHash(hash);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("O hash da senha não pode ser vazio*");
        }

        [Fact]
        public void Deve_Retornar_Asteriscos_No_ToString_Para_Seguranca()
        {
            // Arrange
            var senhaHash = new SenhaHash("meu_segredo");

            // Act
            var resultado = senhaHash.ToString();

            // Assert
            resultado.Should().Be("***");
        }

        [Fact]
        public void Deve_Ser_Igual_Quando_Hashes_Forem_Iguais()
        {
            // Arrange
            var hash = "mesmo_hash";
            var senha1 = new SenhaHash(hash);
            var senha2 = new SenhaHash(hash);

            // Act & Assert
            senha1.Should().Be(senha2);
            senha1.GetHashCode().Should().Be(senha2.GetHashCode());
        }
    }
}
