using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Trabalho_Pratico.Models
{
    public class Aluguel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFimPrevista { get; set; }

        public DateTime? DataDevolucao { get; set; }

        [Required]
        public int QuilometragemInicial { get; set; }

        public int? QuilometragemFinal { get; set; }

        [Required]
        public decimal ValorDiaria { get; set; }

        public decimal? ValorTotal { get; set; }

        // Chaves Estrangeiras
        [Required]
        [ForeignKey(nameof(Cliente))]
        public int ClienteId { get; set; }

        [Required]
        [ForeignKey(nameof(Veiculo))]
        public int VeiculoId { get; set; }

        public Cliente? Cliente { get; set; }
        public Veiculo? Veiculo { get; set; }
        public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
    }
}

