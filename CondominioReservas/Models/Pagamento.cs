using CondominioReservas.API.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace CondominioReservas.Models

{
    public class Pagamento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReservaId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Valor { get; set; }

        [Required]
        [StringLength(20)]
        public required string Metodo { get; set; } // pix, cartao, etc.

        [Required]
        [StringLength(20)]
        public string? Status { get; set; } // pendente, confirmado, cancelado

        public required string CodigoCopiaECola { get; set; }

        public required string QrCodeUrl { get; set; }

        public DateTime? DataPagamento { get; set; }

        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? UltimaAtualizacao { get; set; }

        // Navegação
        public virtual required Reserva Reserva { get; set; }
    }
}
