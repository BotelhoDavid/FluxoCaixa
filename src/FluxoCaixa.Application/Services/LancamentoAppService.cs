using ClosedXML.Excel;
using FluxoCaixa.Application.Interfaces;
using FluxoCaixa.Application.ViewModels;
using FluxoCaixa.Domain.Entities;
using FluxoCaixa.Domain.Extensions;
using FluxoCaixa.Domain.Interfaces.Repositories;
using FluxoCaixa.Domain.Interfaces.UoW;
using FluxoCaixa.Domain.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Net;

namespace FluxoCaixa.Application.Services
{
    public class LancamentoAppService : ILancamentoAppService
    {
        private readonly ILancamentoRepository _lancamentoRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<LancamentoAppService> _logger;

        public LancamentoAppService(
            ILancamentoRepository lancamentoRepository,
            IMapper mapper,
            IUnitOfWork uow,
            ILogger<LancamentoAppService> logger)
        {
            _lancamentoRepository = lancamentoRepository;
            _mapper = mapper;
            _uow = uow;
            _logger = logger;
        }

        public async Task<List<LancamentoViewModel>> ObterLancamentosAsync(DataLancamentoViewModel data)
        {
            _logger.LogInformation("Buscando lançamentos no período {DataInicial} a {DataFinal}", data.DataInicial, data.DataFinal);
            data.ValidarDatas();

            Expression<Func<Lancamento, bool>> _where = lancamento => !lancamento.Deletado;

            /* Filtra Por Status do Pedido */
            if (data.DataInicial is not null)
                _where = _where.And(lancamento => lancamento.DataCriacao.Date >= data.DataInicial.Value.Date);

            /* Filtra Por Status do Pedido */
            if (data.DataFinal is not null)
                _where = _where.And(lancamento => lancamento.DataCriacao.Date >= data.DataFinal.Value.Date);

            var lancamentos = await _lancamentoRepository.ObterPorPeriodoAsync(_where);

            if (!lancamentos.Any())
                throw new ApiException(message: "Nenhum Lançamento foi localizado",
                                       statusCode: HttpStatusCode.NotFound);

            return _mapper.Map<List<LancamentoViewModel>>(lancamentos);
        }

        public async Task<byte[]> GerarRelatorioAsync(DateTime data)
        {
            _logger.LogInformation("Gerando relatório Excel para a data {Data}", data.ToShortDateString());

            var lancamentos = await _lancamentoRepository.ObterPorPeriodoAsync(lancamento => !lancamento.Deletado && lancamento.DataLancamento.Date == data.Date);

            if (!lancamentos.Any())
                throw new ApiException(message: "Nenhum lançamento encontrado para a data informada",
                                       statusCode: HttpStatusCode.NotFound);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Relatório Diário");

                // Cabeçalho
                worksheet.Cell(1, 1).Value = "Data";
                worksheet.Cell(1, 2).Value = "Descrição";
                worksheet.Cell(1, 3).Value = "Valor";
                worksheet.Cell(1, 4).Value = "Tipo";

                var currentRow = 2;
                foreach (var lancamento in lancamentos)
                {
                    worksheet.Cell(currentRow, 1).Value = lancamento.DataLancamento;
                    worksheet.Cell(currentRow, 2).Value = lancamento.Descricao;
                    worksheet.Cell(currentRow, 3).Value = lancamento.Valor;
                    worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "_-R$ * #,##0.00_-;-R$ * #,##0.00_-;_-R$ * \"-\"??_-;_-@_-";
                    worksheet.Cell(currentRow, 4).Value = lancamento.Tipo.ToString();
                    currentRow++;
                }

                // Cálculo do Saldo Final
                var totalCreditos = lancamentos.Where(l => l.Tipo == TipoLancamento.Credito).Sum(l => l.Valor);
                var totalDebitos = lancamentos.Where(l => l.Tipo == TipoLancamento.Debito).Sum(l => l.Valor);
                var saldoFinal = totalCreditos - totalDebitos;

                currentRow++;
                worksheet.Cell(currentRow, 2).Value = "TOTAL CRÉDITOS:";
                worksheet.Cell(currentRow, 3).Value = totalCreditos;
                worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "_-R$ * #,##0.00_-;-R$ * #,##0.00_-;_-R$ * \"-\"??_-;_-@_-";
                worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

                currentRow++;
                worksheet.Cell(currentRow, 2).Value = "TOTAL DÉBITOS:";
                worksheet.Cell(currentRow, 3).Value = totalDebitos;
                worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "_-R$ * #,##0.00_-;-R$ * #,##0.00_-;_-R$ * \"-\"??_-;_-@_-";
                worksheet.Cell(currentRow, 3).Style.Font.Bold = true;

                currentRow++;
                worksheet.Cell(currentRow, 2).Value = "VALOR FINAL (SALDO):";
                worksheet.Cell(currentRow, 3).Value = saldoFinal;
                worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "_-R$ * #,##0.00_-;-R$ * #,##0.00_-;_-R$ * \"-\"??_-;_-@_-";
                worksheet.Cell(currentRow, 3).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 3).Style.Font.FontColor = saldoFinal >= 0 ? XLColor.DarkGreen : XLColor.DarkRed;

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task RegistrarLancamentoAsync(LancamentoViewModel lancamento)
        {
            _logger.LogInformation("Registrando novo lançamento: {Descricao}, Valor: {Valor}", lancamento.Descricao, lancamento.Valor);
            Lancamento _lancamento = _mapper.Map<Lancamento>(lancamento);

            await _lancamentoRepository.CreateAsync(_lancamento);

            if (!await _uow.CommitAsync())
            {
                _logger.LogError("Falha ao persistir lançamento no banco de dados.");
                throw new ApiException(message: "Houve um erro ao registrar o lançamento.",
                                       statusCode: HttpStatusCode.InternalServerError);
            }
            
            _logger.LogInformation("Lançamento registrado com sucesso. ID: {Id}", _lancamento.Id);
        }
    }
}
