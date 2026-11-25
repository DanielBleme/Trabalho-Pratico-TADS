using System.ComponentModel.DataAnnotations;

namespace Trabalho_Pratico.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public string CPF { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Telefone { get; set; } = string.Empty;

        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}
