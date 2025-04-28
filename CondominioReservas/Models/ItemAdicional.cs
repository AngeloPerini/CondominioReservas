using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CondominioReservas.Models
{
    public class ItemAdicional
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Nome { get; set; }

        public string? Descricao { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ValorUnitario { get; set; }

        [Required]
        public int QuantidadeTotal { get; set; }

        public int? EspacoId { get; set; }

        [Required]
        public bool Ativo { get; set; } = true;

        // Navegação
        [ForeignKey("EspacoId")]
        public virtual Espaco? Espaco { get; set; }

        public virtual ICollection<ItemReserva>? ItensReserva { get; set; }
        public int QuantidadeDisponivel { get; internal set; }
        public int TipoEspacoId { get; internal set; }
    }
}
