using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondominioReservas.API.Data;
using CondominioReservas.API.Models;
using CondominioReservas.Models;

namespace CondominioReservas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspacosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EspacosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Espacos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Espaco>>> GetEspacos()
        {
            return await _context.Espacos
                .Include(e => e.TipoEspaco)
                .Include(e => e.ItensAdicionais)
                .Where(e => e.Ativo)
                .ToListAsync();
        }

        // GET: api/Espacos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Espaco>> GetEspaco(int id)
        {
            var espaco = await _context.Espacos
                .Include(e => e.TipoEspaco)
                .Include(e => e.ItensAdicionais)
                .Include(e => e.RegrasDisponibilidade)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (espaco == null)
            {
                return NotFound();
            }

            return espaco;
        }

        // GET: api/Espacos/Tipo/5
        [HttpGet("Tipo/{tipoId}")]
        public async Task<ActionResult<IEnumerable<Espaco>>> GetEspacosByTipo(int tipoId)
        {
            return await _context.Espacos
                .Include(e => e.TipoEspaco)
                .Include(e => e.ItensAdicionais)
                .Where(e => e.TipoId == tipoId && e.Ativo)
                .ToListAsync();
        }

        // GET: api/Espacos/Disponibilidade?data=2025-04-25
        [HttpGet("Disponibilidade")]
        public async Task<ActionResult<IEnumerable<EspacoDisponibilidadeDto>>> GetDisponibilidade([FromQuery] string data)
        {
            if (string.IsNullOrEmpty(data) || !DateTime.TryParse(data, out DateTime dataConsulta))
            {
                dataConsulta = DateTime.Today;
            }

            // Buscar todos os espaços ativos
            var espacos = await _context.Espacos
                .Include(e => e.TipoEspaco)
                .Include(e => e.ItensAdicionais)
                .Include(e => e.RegrasDisponibilidade)
                .Where(e => e.Ativo)
                .ToListAsync();

            // Buscar reservas para a data consultada
            var reservas = await _context.Reservas
                .Where(r => r.DataInicio.Date <= dataConsulta.Date && r.DataFim.Date >= dataConsulta.Date)
                .Where(r => r.Status != "cancelada")
                .ToListAsync();

            // Verificar disponibilidade de cada espaço
            var resultado = new List<EspacoDisponibilidadeDto>();
            foreach (var espaco in espacos)
            {
                // Verificar se o espaço tem reservas para a data
                bool temReserva = reservas.Any(r => r.EspacoId == espaco.Id);

                // Verificar regras de disponibilidade
                bool disponivel = !temReserva && VerificarRegrasDisponibilidade(espaco, dataConsulta);

                // Criar DTO com informações do espaço e disponibilidade
#pragma warning disable CS8601 // Possível atribuição de referência nula.
                var espacoDto = new EspacoDisponibilidadeDto
                {
                    Id = espaco.Id,
                    Nome = espaco.Nome,
                    TipoId = espaco.TipoId,
                    TipoNome = espaco.TipoEspaco?.Nome,
                    Descricao = espaco.Descricao,
                    Capacidade = espaco.Capacidade,
                    ImagemUrl = espaco.ImagemUrl,
                    DuracaoMaxima = espaco.TipoEspaco?.DuracaoMaximaMinutos ?? 0,
                    DuracaoMinima = espaco.TipoEspaco?.DuracaoMinimaMinutos ?? 0,
                    RequerePagemento = espaco.TipoEspaco?.RequerePagemento ?? false,
                    ValorReserva = espaco.TipoEspaco?.ValorReserva ?? 0,
                    Disponivel = disponivel,
                    ItensAdicionais = espaco.ItensAdicionais?.Select(i => new ItemAdicionalDto
                    {
                        Id = i.Id,
                        Nome = i.Nome,
                        Descricao = i.Descricao,
                        ValorUnitario = i.ValorUnitario,
                        QuantidadeTotal = i.QuantidadeTotal
                    }).ToList()
                };
#pragma warning restore CS8601 // Possível atribuição de referência nula.

                resultado.Add(espacoDto);
            }

            return resultado;
        }

        // PUT: api/Espacos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEspaco(int id, Espaco espaco)
        {
            if (id != espaco.Id)
            {
                return BadRequest();
            }

            espaco.UltimaAtualizacao = DateTime.Now;

            _context.Entry(espaco).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Registrar log de atividade
                var log = new LogAtividade
                {
                    Acao = "Atualizar",
                    Entidade = "Espaco",
                    EntidadeId = espaco.Id,
                    Descricao = $"Espaço {espaco.Nome} atualizado",
                    DataHora = DateTime.Now
                };
                _context.LogsAtividade.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EspacoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Espacos
        [HttpPost]
        public async Task<ActionResult<Espaco>> PostEspaco(Espaco espaco)
        {
            espaco.DataCriacao = DateTime.Now;

            _context.Espacos.Add(espaco);
            await _context.SaveChangesAsync();

            // Registrar log de atividade
            var log = new LogAtividade
            {
                Acao = "Criar",
                Entidade = "Espaco",
                EntidadeId = espaco.Id,
                Descricao = $"Espaço {espaco.Nome} criado",
                DataHora = DateTime.Now
            };
            _context.LogsAtividade.Add(log);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEspaco", new { id = espaco.Id }, espaco);
        }

        // DELETE: api/Espacos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEspaco(int id)
        {
            var espaco = await _context.Espacos.FindAsync(id);
            if (espaco == null)
            {
                return NotFound();
            }

            // Em vez de excluir, apenas desativa o espaço
            espaco.Ativo = false;
            espaco.UltimaAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            // Registrar log de atividade
            var log = new LogAtividade
            {
                Acao = "Desativar",
                Entidade = "Espaco",
                EntidadeId = espaco.Id,
                Descricao = $"Espaço {espaco.Nome} desativado",
                DataHora = DateTime.Now
            };
            _context.LogsAtividade.Add(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EspacoExists(int id)
        {
            return _context.Espacos.Any(e => e.Id == id);
        }

        private bool VerificarRegrasDisponibilidade(Espaco espaco, DateTime data)
        {
            // Se não houver regras, considera disponível
            if (espaco.RegrasDisponibilidade == null || !espaco.RegrasDisponibilidade.Any())
            {
                return true;
            }

            int diaSemana = (int)data.DayOfWeek;
            TimeSpan horaAtual = data.TimeOfDay;

            // Verificar regras específicas para o dia da semana
            var regrasDia = espaco.RegrasDisponibilidade
                .Where(r => r.DiaSemana == diaSemana || r.DiaSemana == null)
                .ToList();

            // Se não houver regras para o dia, considera disponível
            if (!regrasDia.Any())
            {
                return true;
            }

            // Verificar se alguma regra permite a disponibilidade
            return regrasDia.Any(r => r.Disponivel);
        }
    }

    public class EspacoDisponibilidadeDto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public int TipoId { get; set; }
        public string? TipoNome { get; set; }
        public string? Descricao { get; set; }
        public int? Capacidade { get; set; }
        public required string ImagemUrl { get; set; }
        public int DuracaoMaxima { get; set; }
        public int DuracaoMinima { get; set; }
        public bool RequerePagemento { get; set; }
        public decimal ValorReserva { get; set; }
        public bool Disponivel { get; set; }
        public List<ItemAdicionalDto>? ItensAdicionais { get; set; }
    }

    public class ItemAdicionalDto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public string? Descricao { get; set; }
        public decimal ValorUnitario { get; set; }
        public int QuantidadeTotal { get; set; }
    }
}
