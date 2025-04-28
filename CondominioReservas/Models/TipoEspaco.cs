using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CondominioReservas.Models
{
    public class TipoEspaco
    {
        [Key]
        public required int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Nome { get; set; }

        public string? Descricao { get; set; }

        [Required]
        public int DuracaoMaximaMinutos { get; set; }

        [Required]
        public int DuracaoMinimaMinutos { get; set; }

        [Required]
        public bool RequerePagemento { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ValorReserva { get; set; }

        [Required]
        public bool Ativo { get; set; } = true;

        // Navegação
        public virtual ICollection<Espaco>? Espacos { get; set; }
        public int DuracaoMaximaHoras { get; internal set; }
        public int DuracaoMinimaHoras { get; internal set; }
    }
}
