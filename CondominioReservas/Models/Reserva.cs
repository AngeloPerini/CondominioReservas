using CondominioReservas.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CondominioReservas.API.Models
{
    public class Reserva
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public int EspacoId { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        [Required]
        [StringLength(20)]
        public required string Status { get; set; } // pendente, confirmada, cancelada, concluída

        [Required]
        public bool RequerePagemento { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ValorTotal { get; set; }

        public string? Observacoes { get; set; }

        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? UltimaAtualizacao { get; set; }

        // Navegação
        [ForeignKey("UsuarioId")]
        public virtual required Usuario Usuario { get; set; }

        [ForeignKey("EspacoId")]
        public virtual required Espaco Espaco { get; set; }

        public virtual required ICollection<ItemReserva> ItensReserva { get; set; }

        public virtual required ICollection<Pagamento> Pagamentos { get; set; }
    }
}
