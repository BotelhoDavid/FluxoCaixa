using FluentAssertions;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces.Services;
using FluxoCaixa.Domain.Models;
using FluxoCaixa.Domain.ValueObjects;
using Moq;
using System.Net;
using Xunit;

namespace FluxoCaixa.Tests.Domain.Entities
{
    public class UsuarioTests
    {
        private readonly Mock<IPasswordHasher> _passwordHasherMock;

        public UsuarioTests()
        {
            _passwordHasherMock = new Mock<IPasswordHasher>();
        }

        [Fact]
        public void Deve_Criar_Usuario_Valido()
        {
            // Arrange
            var nome = "João Silva";
            var email = "joao.silva@example.com";
            var senha = "senhaSegura123";
            var senhaHash = new SenhaHash("hash_gerado");

            _passwordHasherMock.Setup(x => x.HashPassword(senha)).Returns(senhaHash);

            // Act
            var usuario = Usuario.Criar(nome, email, senha, _passwordHasherMock.Object);

            // Assert
            usuario.Should().NotBeNull();
            usuario.Nome.Should().Be(nome);
            usuario.Email.Should().Be(email);
            usuario.SenhaHash.Should().Be(senhaHash);
            usuario.Ativo.Should().BeTrue();
            usuario.DataUltimoAcesso.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("Jo")] // Menor que 3 caracteres
        public void Deve_Lancar_Excecao_Quando_Nome_Invalido(string nome)
        {
            // Arrange
            var email = "joao@example.com";
            var senha = "senha123";

            // Act
            Action act = () => Usuario.Criar(nome, email, senha, _passwordHasherMock.Object);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("O nome*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("emailinvalido")]
        [InlineData("usuario@")]
        public void Deve_Lancar_Excecao_Quando_Email_Invalido(string email)
        {
            // Arrange
            var nome = "João Silva";
            var senha = "senha123";

            // Act
            Action act = () => Usuario.Criar(nome, email, senha, _passwordHasherMock.Object);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("*e-mail*");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("12345")] // Menor que 6 caracteres
        public void Deve_Lancar_Excecao_Quando_Senha_Invalida(string senha)
        {
            // Arrange
            var nome = "João Silva";
            var email = "joao@example.com";

            // Act
            Action act = () => Usuario.Criar(nome, email, senha, _passwordHasherMock.Object);

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("A senha*");
        }

        [Fact]
        public void Deve_Autenticar_Com_Sucesso()
        {
            // Arrange
            var senha = "senha123";
            var hash = new SenhaHash("hash123");
            _passwordHasherMock.Setup(x => x.HashPassword(senha)).Returns(hash);
            _passwordHasherMock.Setup(x => x.VerifyPassword(senha, hash)).Returns(true);

            var usuario = Usuario.Criar("Teste", "teste@example.com", senha, _passwordHasherMock.Object);

            // Act
            Action act = () => usuario.Autenticar(senha, _passwordHasherMock.Object);
            
            // Assert
            act.Should().NotThrow();
            usuario.DataUltimoAcesso.Should().NotBeNull();
            // A data de último acesso deve ser recente (dentro de 1 segundo)
            usuario.DataUltimoAcesso.Value.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Deve_Falhar_Autenticacao_Com_Senha_Incorreta()
        {
            // Arrange
            var senhaCorreta = "senha123";
            var senhaIncorreta = "senhaErrada";
            var hash = new SenhaHash("hash123");
            
            _passwordHasherMock.Setup(x => x.HashPassword(senhaCorreta)).Returns(hash);
            _passwordHasherMock.Setup(x => x.VerifyPassword(senhaIncorreta, hash)).Returns(false);

            var usuario = Usuario.Criar("Teste", "teste@example.com", senhaCorreta, _passwordHasherMock.Object);

            // Act
            Action act = () => usuario.Autenticar(senhaIncorreta, _passwordHasherMock.Object);
            
            // Assert
            act.Should().Throw<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.Unauthorized && e.Message == "Usuário ou senha inválida");
            usuario.DataUltimoAcesso.Should().BeNull();
        }
    }
}
