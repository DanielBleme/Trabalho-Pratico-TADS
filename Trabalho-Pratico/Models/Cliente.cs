using System.ComponentModel.DataAnnotations;

namespace Trabalho_Pratico.Models
{
    public class Cliente
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public string Nome { get; set; } 

        [Required]
        public string CPF { get; set; } 

        [Required]
        [EmailAddress]
        public string Email { get; set; } 

        [Required]
        public string? Telefone { get; set; }

        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
