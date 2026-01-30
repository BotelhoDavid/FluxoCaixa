using FluxoCaixa.Application.Interfaces;
using FluxoCaixa.Application.ViewModels;
using FluxoCaixa.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace FluxoCaixa.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Endpoints para gerenciamento de lançamentos financeiros")]
    public class LancamentoController : ControllerBase
    {
        private readonly ILancamentoAppService _service;

        public LancamentoController(ILancamentoAppService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Listar lançamentos", Description = "Retorna uma lista de lançamentos filtrados por período.")]
        [ProducesResponseType(typeof(List<LancamentoViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromQuery] DataLancamentoViewModel data)
        {
            var lancamentos = await _service.ObterLancamentosAsync(data);
            return Ok(lancamentos);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Criar lançamento", Description = "Registra um novo lançamento (crédito ou débito) no sistema.")]
        [ProducesResponseType(typeof(LancamentoViewModel), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Post([FromBody]LancamentoViewModel lancamento)
        {
            await _service.RegistrarLancamentoAsync(lancamento);
            return Created("", lancamento);
        }

        [HttpGet("relatorio")]
        [SwaggerOperation(Summary = "Gerar relatório Excel", Description = "Gera um arquivo Excel (.xlsx) contendo os lançamentos e o saldo final para a data informada.")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiException), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GerarRelatorio([FromQuery] DateTime data)
        {
            var arquivo = await _service.GerarRelatorioAsync(data);
            return File(arquivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"relatorio_fluxocaixa_{data:ddMMyyyy}.xlsx");
        }
    }
}
