using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Trabalho_Pratico.Models
{
    public class Pagamento
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        [ForeignKey(nameof(Aluguel))]
        public int? AluguelId { get; set; }

        [Required]
        public DateTime DataPagamento { get; set; }

        [Required]
        public decimal ValorPago { get; set; }

        [Required]
        public string MetodoPagamento { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; }

        public Aluguel? Aluguel { get; set; }
    }
}
