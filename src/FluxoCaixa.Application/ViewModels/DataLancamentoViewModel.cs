using Swashbuckle.AspNetCore.Annotations;

namespace FluxoCaixa.Application.ViewModels
{
    public class DataLancamentoViewModel
    {
        /// <summary>
        /// Data inicial para filtro (opcional)
        /// </summary>
        /// <example>2024-01-01</example>
        [SwaggerSchema(Description = "Data inicial para filtro (opcional)")]
        public DateTime? DataInicial { get; set; }

        /// <summary>
        /// Data final para filtro (opcional)
        /// </summary>
        /// <example>2024-01-31</example>
        [SwaggerSchema(Description = "Data final para filtro (opcional)")]
        public DateTime? DataFinal { get; set; }

        public void ValidarDatas()
        {
            if (DataInicial.HasValue && DataFinal.HasValue && DataInicial > DataFinal)
                throw new ArgumentException("Data inicial não pode ser maior que a data final");
        }
    }
}
