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
    public class ReservasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReservasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservas()
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Espaco)
                    .ThenInclude(e => e.TipoEspaco)
                .Include(r => r.ItensReserva)
                    .ThenInclude(ir => ir.ItemAdicional)
                .ToListAsync();
        }

        // GET: api/Reservas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reserva>> GetReserva(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Usuario)
                .Include(r => r.Espaco)
                    .ThenInclude(e => e.TipoEspaco)
                .Include(r => r.ItensReserva)
                    .ThenInclude(ir => ir.ItemAdicional)
                .Include(r => r.Pagamentos)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound();
            }

            return reserva;
        }

        // GET: api/Reservas/Usuario/5
        [HttpGet("Usuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservasByUsuario(int usuarioId)
        {
            return await _context.Reservas
                .Include(r => r.Espaco)
                    .ThenInclude(e => e.TipoEspaco)
                .Include(r => r.ItensReserva)
                    .ThenInclude(ir => ir.ItemAdicional)
                .Include(r => r.Pagamentos)
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.DataInicio)
                .ToListAsync();
        }

        // GET: api/Reservas/Espaco/5
        [HttpGet("Espaco/{espacoId}")]
        public async Task<ActionResult<IEnumerable<Reserva>>> GetReservasByEspaco(int espacoId)
        {
            return await _context.Reservas
                .Include(r => r.Usuario)
                .Where(r => r.EspacoId == espacoId)
                .OrderBy(r => r.DataInicio)
                .ToListAsync();
        }

        // POST: api/Reservas
        [HttpPost]
        public async Task<ActionResult<Reserva>> PostReserva(Reserva reserva)
        {
            // Verificar se o espaço existe
            var espaco = await _context.Espacos
                .Include(e => e.TipoEspaco)
                .FirstOrDefaultAsync(e => e.Id == reserva.EspacoId);

            if (espaco == null)
            {
                return BadRequest("Espaço não encontrado.");
            }

            // Verificar se o usuário existe
            var usuario = await _context.Usuarios.FindAsync(reserva.UsuarioId);
            if (usuario == null)
            {
                return BadRequest("Usuário não encontrado.");
            }

            // Verificar se o período está disponível
            bool conflito = await _context.Reservas
                .Where(r => r.EspacoId == reserva.EspacoId)
                .Where(r => r.Status != "cancelada")
                .Where(r => (r.DataInicio <= reserva.DataFim && r.DataFim >= reserva.DataInicio))
                .AnyAsync();

            if (conflito)
            {
                return BadRequest("Já existe uma reserva para este espaço no período selecionado.");
            }

            // Verificar duração mínima e máxima
            TimeSpan duracao = reserva.DataFim - reserva.DataInicio;
            int duracaoMinutos = (int)duracao.TotalMinutes;

            if (duracaoMinutos < espaco.TipoEspaco.DuracaoMinimaMinutos)
            {
                return BadRequest($"A duração mínima para este espaço é de {espaco.TipoEspaco.DuracaoMinimaMinutos / 60} horas.");
            }

            if (duracaoMinutos > espaco.TipoEspaco.DuracaoMaximaMinutos)
            {
                return BadRequest($"A duração máxima para este espaço é de {espaco.TipoEspaco.DuracaoMaximaMinutos / 60} horas.");
            }

            // Verificar se o espaço requer pagamento
            reserva.RequerePagemento = espaco.TipoEspaco.RequerePagemento;

            // Se não requer pagamento, já confirma a reserva
            if (!reserva.RequerePagemento)
            {
                reserva.Status = "confirmada";
                reserva.ValorTotal = 0;
            }
            else
            {
                // Se requer pagamento, define status como pendente
                reserva.Status = "pendente";

                // Define o valor da reserva com base no tipo de espaço
                reserva.ValorTotal = espaco.TipoEspaco.ValorReserva;

                // Adiciona valores dos itens adicionais, se houver
                if (reserva.ItensReserva != null && reserva.ItensReserva.Any())
                {
                    foreach (var item in reserva.ItensReserva)
                    {
                        var itemAdicional = await _context.ItensAdicionais.FindAsync(item.ItemId);
                        if (itemAdicional != null)
                        {
                            item.ValorUnitario = itemAdicional.ValorUnitario;
                            item.ValorTotal = item.ValorUnitario * item.Quantidade;
                            reserva.ValorTotal += item.ValorTotal;
                        }
                    }
                }
            }

            // Definir data de criação
            reserva.DataCriacao = DateTime.Now;

            // Adicionar a reserva
            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            // Registrar log de atividade
            var log = new LogAtividade
            {
                UsuarioId = reserva.UsuarioId,
                Acao = "Criar",
                Entidade = "Reserva",
                EntidadeId = reserva.Id,
                Descricao = $"Reserva criada para o espaço {espaco.Nome} de {reserva.DataInicio} até {reserva.DataFim}",
                DataHora = DateTime.Now
            };
            _context.LogsAtividade.Add(log);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReserva", new { id = reserva.Id }, reserva);
        }

        // PUT: api/Reservas/5/Status
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> AtualizarStatusReserva(int id, [FromBody] string status)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            reserva.Status = status;
            reserva.UltimaAtualizacao = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();

                // Registrar log de atividade
                var log = new LogAtividade
                {
                    Acao = "Atualizar",
                    Entidade = "Reserva",
                    EntidadeId = reserva.Id,
                    Descricao = $"Status da reserva atualizado para {status}",
                    DataHora = DateTime.Now
                };
                _context.LogsAtividade.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservaExists(id))
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

        // DELETE: api/Reservas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            // Em vez de excluir, apenas atualiza o status para cancelada
            reserva.Status = "cancelada";
            reserva.UltimaAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            // Registrar log de atividade
            var log = new LogAtividade
            {
                UsuarioId = reserva.UsuarioId,
                Acao = "Cancelar",
                Entidade = "Reserva",
                EntidadeId = reserva.Id,
                Descricao = $"Reserva cancelada",
                DataHora = DateTime.Now
            };
            _context.LogsAtividade.Add(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.Id == id);
        }
    }
}
