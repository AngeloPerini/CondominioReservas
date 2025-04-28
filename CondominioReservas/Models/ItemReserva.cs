using CondominioReservas.API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CondominioReservas.Models
{
    public class ItemReserva
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReservaId { get; set; }

        [Required]
        public int ItemId { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ValorUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ValorTotal { get; set; }

        // Navegação
        [ForeignKey("ReservaId")]
        public virtual required Reserva Reserva { get; set; }

        [ForeignKey("ItemId")]
        public virtual required ItemAdicional ItemAdicional { get; set; }
    }
}
