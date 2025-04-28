using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CondominioReservas.Models
{
    public class LogAtividade
    {
        [Key]
        public int Id { get; set; }

        public int? UsuarioId { get; set; }

        [Required]
        [StringLength(50)]
        public string? Acao { get; set; }

        public string? Descricao { get; set; }

        [StringLength(50)]
        public string? Entidade { get; set; }

        public int? EntidadeId { get; set; }

        [Required]
        public DateTime DataHora { get; set; } = DateTime.Now;

        // Navegação
        [ForeignKey("UsuarioId")]
        public  virtual Usuario? Usuario { get; set; }
    }
}
