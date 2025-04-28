using CondominioReservas.API.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CondominioReservas.Models
{
    public class Espaco
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Required]
        public int TipoId { get; set; }

        public string? Descricao { get; set; }

        public int? Capacidade { get; set; }

        public string? ImagemUrl { get; set; }

        [Required]
        public bool Ativo { get; set; } = true;

        [Required]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? UltimaAtualizacao { get; set; }

        // Navegação
        [ForeignKey("TipoId")]
        public virtual TipoEspaco? TipoEspaco { get; set; }

        public virtual  ICollection<ItemAdicional>? ItensAdicionais { get; set; }

        public virtual ICollection<RegraDisponibilidade>? RegrasDisponibilidade { get; set; }

        public virtual ICollection<Reserva>? Reservas { get; set; }
        public int? TipoEspacoId { get; internal set; }
    }
}
