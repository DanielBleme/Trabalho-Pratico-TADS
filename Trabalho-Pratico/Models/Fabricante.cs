using System.ComponentModel.DataAnnotations;

namespace Trabalho_Pratico.Models
{
    public class Fabricante
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nome { get; set; } = string.Empty;
        [Required]
        [StringLength(18)]
        public string CNPJ { get; set; } = string.Empty;
        public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();

    }
}
