using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trabalho_Pratico.Models
{
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Modelo { get; set; } 

        [Required]
        public int AnoFabricacao { get; set; }

        [Required]
        public string Placa { get; set; } 

        [Required]
        public int Quilometragem { get; set; }

        // Chave Estrangeira
        [Required]
        [ForeignKey(nameof(Fabricante))]
        public int FabricanteId { get; set; }

        public Fabricante? Fabricante { get; set; }
        public ICollection<Aluguel> Alugueis { get; set; } = new List<Aluguel>();
    }
}