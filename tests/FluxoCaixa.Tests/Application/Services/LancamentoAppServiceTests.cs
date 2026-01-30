using FluentAssertions;
using FluxoCaixa.Application.Services;
using FluxoCaixa.Application.ViewModels;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Interfaces.Repositories;
using FluxoCaixa.Domain.Interfaces.UoW;
using FluxoCaixa.Domain.Models;
using MapsterMapper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Net;
using Xunit;

namespace FluxoCaixa.Tests.Application.Services
{
    public class LancamentoAppServiceTests
    {
        private readonly Mock<ILancamentoRepository> _lancamentoRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ILogger<LancamentoAppService>> _loggerMock;
        private readonly LancamentoAppService _service;

        public LancamentoAppServiceTests()
        {
            _lancamentoRepositoryMock = new Mock<ILancamentoRepository>();
            _mapperMock = new Mock<IMapper>();
            _uowMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<LancamentoAppService>>();

            _service = new LancamentoAppService(
                _lancamentoRepositoryMock.Object,
                _mapperMock.Object,
                _uowMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Deve_Obter_Lancamentos_Com_Sucesso()
        {
            // Arrange
            var dataVm = new DataLancamentoViewModel 
            { 
                DataInicial = new DateTime(2024, 1, 1), 
                DataFinal = new DateTime(2024, 1, 31) 
            };
            
            var lancamentos = new List<Lancamento>
            {
                new Lancamento { Descricao = "Teste 1", Valor = 100, Tipo = TipoLancamento.Credito }
            };

            _lancamentoRepositoryMock.Setup(r => r.ObterPorPeriodoAsync(It.IsAny<Expression<Func<Lancamento, bool>>>()))
                .ReturnsAsync(lancamentos);

            _mapperMock.Setup(m => m.Map<List<LancamentoViewModel>>(It.IsAny<List<Lancamento>>()))
                .Returns(new List<LancamentoViewModel> { new LancamentoViewModel { Descricao = "Teste 1" } });

            // Act
            var resultado = await _service.ObterLancamentosAsync(dataVm);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
        }

        [Fact]
        public async Task Deve_Gerar_Relatorio_Com_Sucesso()
        {
            // Arrange
            var dataVm = new DateTime(2024, 1, 1);
            
            var lancamentos = new List<Lancamento>
            {
                new Lancamento { Descricao = "Teste 1", Valor = 100, Tipo = TipoLancamento.Credito }
            };

            _lancamentoRepositoryMock.Setup(r => r.ObterPorPeriodoAsync(It.IsAny<Expression<Func<Lancamento, bool>>>()))
                .ReturnsAsync(lancamentos);

            // Act
            var resultado = await _service.GerarRelatorioAsync(dataVm);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Length.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Deve_Lancar_Excecao_Quando_Nenhum_Lancamento_Encontrado()
        {
            // Arrange
            var dataVm = new DataLancamentoViewModel();

            _lancamentoRepositoryMock.Setup(r => r.ObterPorPeriodoAsync(It.IsAny<Expression<Func<Lancamento, bool>>>()))
                .ReturnsAsync(new List<Lancamento>());

            // Act
            Func<Task> act = async () => await _service.ObterLancamentosAsync(dataVm);

            // Assert
            await act.Should().ThrowAsync<ApiException>()
                .Where(e => e.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Deve_Registrar_Lancamento_Com_Sucesso()
        {
            // Arrange
            var viewModel = new LancamentoViewModel { Descricao = "Novo", Valor = 100 };
            var entidade = new Lancamento { Descricao = "Novo", Valor = 100 };

            _mapperMock.Setup(m => m.Map<Lancamento>(viewModel)).Returns(entidade);
            _uowMock.Setup(u => u.CommitAsync()).ReturnsAsync(true);

            // Act
            await _service.RegistrarLancamentoAsync(viewModel);

            // Assert
            _lancamentoRepositoryMock.Verify(r => r.CreateAsync(entidade), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
