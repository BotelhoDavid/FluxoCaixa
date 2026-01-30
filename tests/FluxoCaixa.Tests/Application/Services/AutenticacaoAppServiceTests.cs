using FluentAssertions;
using FluxoCaixa.Application.Services;
using FluxoCaixa.Application.ViewModels;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces.Repositories;
using FluxoCaixa.Domain.Interfaces.Services;
using FluxoCaixa.Domain.Interfaces.UoW;
using FluxoCaixa.Domain.Models;
using FluxoCaixa.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using Xunit;

namespace FluxoCaixa.Tests.Application.Services
{
    public class AutenticacaoAppServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ILogger<AutenticacaoAppService>> _loggerMock;
        private readonly AutenticacaoAppService _service;

        public AutenticacaoAppServiceTests()
        {
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _uowMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<AutenticacaoAppService>>();

            // Configuração para JWT
            _configurationMock.Setup(x => x["Jwt:Secret"]).Returns("ChaveSecretaParaTestesUnitariosMuitolongaaaaaa");
            _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("TesteIssuer");
            _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("TesteAudience");

            _service = new AutenticacaoAppService(
                _usuarioRepositoryMock.Object,
                _configurationMock.Object,
                _passwordHasherMock.Object,
                _uowMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Deve_Autenticar_Com_Sucesso()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "teste@example.com", Password = "senha123" };
            var senhaHash = new SenhaHash("hashed_password");
            
            _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns(senhaHash);
            
            var usuario = Usuario.Criar("Teste", loginRequest.Email, "senha_temp", _passwordHasherMock.Object);

            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync(loginRequest.Email))
                .ReturnsAsync(usuario);

            _passwordHasherMock.Setup(p => p.VerifyPassword(loginRequest.Password, It.IsAny<SenhaHash>()))
                .Returns(true);

            _uowMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

            // Act
            var response = await _service.AutenticarAsync(loginRequest);

            // Assert
            response.Should().NotBeNull();
            response.AccessToken.Should().NotBeNullOrEmpty();
            response.TokenType.Should().Be("Bearer");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Usuario_Nao_Encontrado()
        {
            // Arrange
            var loginRequest = new LoginRequest { Email = "inexistente@example.com", Password = "senha" };
            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync(loginRequest.Email))
                .ReturnsAsync((Usuario)null);

            // Act
            Func<Task> act = async () => await _service.AutenticarAsync(loginRequest);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.Unauthorized && e.Message == "Usuário ou senha inválida");
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Senha_Invalida()
        {
             // Arrange
            var loginRequest = new LoginRequest { Email = "teste@example.com", Password = "senhaErrada" };
            var senhaHash = new SenhaHash("hashed_password");
             _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns(senhaHash);
            
            var usuario = Usuario.Criar("Teste", loginRequest.Email, "senha_correta", _passwordHasherMock.Object);

            _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync(loginRequest.Email))
                .ReturnsAsync(usuario);

            _passwordHasherMock.Setup(p => p.VerifyPassword(loginRequest.Password, It.IsAny<SenhaHash>()))
                .Returns(false);

            // Act
            Func<Task> act = async () => await _service.AutenticarAsync(loginRequest);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.Unauthorized && e.Message == "Usuário ou senha inválida");
        }

        [Fact]
        public async Task Deve_Registrar_Usuario_Com_Sucesso()
        {
            // Arrange
            var registroRequest = new UsuarioRegistroRequest 
            { 
                Nome = "Novo Usuario", 
                Email = "novo@example.com", 
                Password = "senhaForte123" 
            };

            _usuarioRepositoryMock.Setup(r => r.ExisteEmailAsync(registroRequest.Email))
                .ReturnsAsync(false);
            
            _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<string>()))
                .Returns(new SenhaHash("hashed"));

            _uowMock.Setup(u => u.CommitAsync()).ReturnsAsync(true); // Retorna true (sucesso)

            // Act
            await _service.RegistrarAsync(registroRequest);

            // Assert
            _usuarioRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Email_Ja_Cadastrado()
        {
            // Arrange
            var registroRequest = new UsuarioRegistroRequest 
            { 
                Nome = "Usuario Existente", 
                Email = "existente@example.com", 
                Password = "senha" 
            };

            _usuarioRepositoryMock.Setup(r => r.ExisteEmailAsync(registroRequest.Email))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _service.RegistrarAsync(registroRequest);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.BadRequest && e.Message == "E-mail já cadastrado");
            
            _usuarioRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Never);
        }
    }
}
