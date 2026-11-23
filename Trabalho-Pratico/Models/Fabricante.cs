using System.ComponentModel.DataAnnotations;

namespace Trabalho_Pratico.Models
{
    public class Fabricante
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Nome { get; set; }
        [Required]
        public int? CNPJ { get; set; }
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();

    }
}
