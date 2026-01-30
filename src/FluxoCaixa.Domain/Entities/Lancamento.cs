using FluxoCaixa.Domain.Models;
using System;

namespace FluxoCaixa.Domain.Entities
{
    public class Lancamento : Entity
    {
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataLancamento { get; set; }
        public TipoLancamento Tipo { get; set; }
    }

    public enum TipoLancamento
    {
        Credito = 1,
        Debito = 2
    }
}
