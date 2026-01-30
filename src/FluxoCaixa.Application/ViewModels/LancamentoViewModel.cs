using FluxoCaixa.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace FluxoCaixa.Application.ViewModels
{
    public class LancamentoViewModel
    {
        /// <summary>
        /// Descrição detalhada do lançamento
        /// </summary>
        /// <example>Pagamento de fornecedor XYZ</example>
        [SwaggerSchema(Description = "Descrição detalhada do lançamento")]
        public string Descricao { get; set; }

        /// <summary>
        /// Valor do lançamento
        /// </summary>
        /// <example>1500.50</example>
        [SwaggerSchema(Description = "Valor do lançamento. Positivo para entrada e saída (o tipo define)")]
        public decimal Valor { get; set; }

        /// <summary>
        /// Data e hora do lançamento
        /// </summary>
        /// <example>2024-01-29T14:30:00</example>
        [SwaggerSchema(Description = "Data e hora do lançamento")]
        public DateTime DataLancamento { get; set; }

        /// <summary>
        /// Tipo de lançamento (1 = Crédito, 2 = Débito)
        /// </summary>
        /// <example>2</example>
        [SwaggerSchema(Description = "Tipo de lançamento (1 = Crédito, 2 = Débito)")]
        public TipoLancamento Tipo { get; set; }
    }
}
