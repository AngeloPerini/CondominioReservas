using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CondominioReservas.API.Models;

namespace CondominioReservas.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Nome { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(11)]
        public required string CPF { get; set; }

        [StringLength(20)]
        public required string Telefone { get; set; }

        [Required]
        [StringLength(10)]
        public required string NumeroCasa { get; set; }

        [StringLength(10)]
        public required string Rua { get; set; }

        [Required]
        public bool Ativo { get; set; } = true;

        [Required]
        public bool Admin { get; set; } = false;

        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? UltimaAtualizacao { get; set; }

        // Navegação
        public virtual required ICollection<Reserva> Reservas { get; set; }

        public virtual required ICollection<LogAtividade> LogsAtividades { get; set; }
    }
}
