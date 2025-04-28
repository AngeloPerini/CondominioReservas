using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CondominioReservas.Models
{
    public class RegraDisponibilidade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EspacoId { get; set; }

        public int? DiaSemana { get; set; } // 0 = Domingo, 1 = Segunda, ..., 6 = Sábado, NULL = Todos os dias

        public TimeSpan? HoraInicio { get; set; }

        public TimeSpan? HoraFim { get; set; }

        [Required]
        public bool Disponivel { get; set; } = true;

        // Navegação
        [ForeignKey("EspacoId")]
        public virtual required Espaco Espaco { get; set; }
    }
}
