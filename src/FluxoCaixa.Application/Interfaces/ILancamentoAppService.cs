using FluxoCaixa.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluxoCaixa.Application.Interfaces
{
    public interface ILancamentoAppService
    {
        Task<List<LancamentoViewModel>> ObterLancamentosAsync(DataLancamentoViewModel data);
        Task RegistrarLancamentoAsync(LancamentoViewModel lancamento);
        Task<byte[]> GerarRelatorioAsync(DateTime data);
    }
}
