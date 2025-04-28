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
    public class PagamentosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PagamentosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pagamentos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pagamento>>> GetPagamentos()
        {
            return await _context.Pagamentos
                .Include(p => p.Reserva)
                .ToListAsync();
        }

        // GET: api/Pagamentos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pagamento>> GetPagamento(int id)
        {
            var pagamento = await _context.Pagamentos
                .Include(p => p.Reserva)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pagamento == null)
            {
                return NotFound();
            }

            return pagamento;
        }

        // GET: api/Pagamentos/Reserva/5
        [HttpGet("Reserva/{reservaId}")]
        public async Task<ActionResult<IEnumerable<Pagamento>>> GetPagamentosByReserva(int reservaId)
        {
            return await _context.Pagamentos
                .Where(p => p.ReservaId == reservaId)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }

        // POST: api/Pagamentos
        [HttpPost]
        public async Task<ActionResult<Pagamento>> PostPagamento(Pagamento pagamento)
        {
            // Verificar se a reserva existe
            var reserva = await _context.Reservas
                .Include(r => r.Espaco)
                    .ThenInclude(e => e.TipoEspaco)
                .FirstOrDefaultAsync(r => r.Id == pagamento.ReservaId);

            if (reserva == null)
            {
                return BadRequest("Reserva não encontrada.");
            }

            // Verificar se a reserva requer pagamento
            if (!reserva.RequerePagemento)
            {
                return BadRequest("Esta reserva não requer pagamento.");
            }

            // Verificar se já existe um pagamento confirmado para esta reserva
            var pagamentoExistente = await _context.Pagamentos
                .Where(p => p.ReservaId == pagamento.ReservaId && p.Status == "confirmado")
                .AnyAsync();

            if (pagamentoExistente)
            {
                return BadRequest("Já existe um pagamento confirmado para esta reserva.");
            }

            // Definir valores padrão
            pagamento.Status = "pendente";
            pagamento.DataCriacao = DateTime.Now;

            // Gerar código PIX (simulação)
            pagamento.CodigoCopiaECola = GerarCodigoPix(pagamento.ReservaId, pagamento.Valor);
            pagamento.QrCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=200x200&data={pagamento.CodigoCopiaECola}";

            // Adicionar o pagamento
            _context.Pagamentos.Add(pagamento);
            await _context.SaveChangesAsync();

            // Registrar log de atividade
            var log = new LogAtividade
            {
                Acao = "Criar",
                Entidade = "Pagamento",
                EntidadeId = pagamento.Id,
                Descricao = $"Pagamento criado para a reserva {pagamento.ReservaId} no valor de R$ {pagamento.Valor}",
                DataHora = DateTime.Now
            };
            _context.LogsAtividade.Add(log);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPagamento", new { id = pagamento.Id }, pagamento);
        }

        // PUT: api/Pagamentos/5/Status
        [HttpPut("{id}/Status")]
        public async Task<IActionResult> AtualizarStatusPagamento(int id, [FromBody] string status)
        {
            var pagamento = await _context.Pagamentos
                .Include(p => p.Reserva)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pagamento == null)
            {
                return NotFound();
            }

            pagamento.Status = status;
            pagamento.UltimaAtualizacao = DateTime.Now;

            // Se o pagamento foi confirmado, atualizar também o status da reserva
            if (status == "confirmado")
            {
                pagamento.DataPagamento = DateTime.Now;

                var reserva = pagamento.Reserva;
                if (reserva != null)
                {
                    reserva.Status = "confirmada";
                    reserva.UltimaAtualizacao = DateTime.Now;
                }
            }

            try
            {
                await _context.SaveChangesAsync();

                // Registrar log de atividade
                var log = new LogAtividade
                {
                    Acao = "Atualizar",
                    Entidade = "Pagamento",
                    EntidadeId = pagamento.Id,
                    Descricao = $"Status do pagamento atualizado para {status}",
                    DataHora = DateTime.Now
                };
                _context.LogsAtividade.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PagamentoExists(id))
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

        // POST: api/Pagamentos/Webhook
        [HttpPost("Webhook")]
        public async Task<IActionResult> ProcessarWebhookPagamento([FromBody] WebhookPagamentoDto webhook)
        {
            // Simulação de webhook de pagamento
            if (webhook == null || string.IsNullOrEmpty(webhook.CodigoPix))
            {
                return BadRequest("Dados de webhook inválidos.");
            }

            // Extrair o ID da reserva do código PIX (simulação)
            if (!int.TryParse(webhook.CodigoPix.Split('_')[0], out int reservaId))
            {
                return BadRequest("Código PIX inválido.");
            }

            // Buscar o pagamento pendente mais recente para esta reserva
            var pagamento = await _context.Pagamentos
                .Where(p => p.ReservaId == reservaId && p.Status == "pendente")
                .OrderByDescending(p => p.DataCriacao)
                .FirstOrDefaultAsync();

            if (pagamento == null)
            {
                return NotFound("Pagamento não encontrado.");
            }

            // Atualizar o status do pagamento
            pagamento.Status = "confirmado";
            pagamento.DataPagamento = DateTime.Now;
            pagamento.UltimaAtualizacao = DateTime.Now;

            // Atualizar o status da reserva
            var reserva = await _context.Reservas.FindAsync(reservaId);
            if (reserva != null)
            {
                reserva.Status = "confirmada";
                reserva.UltimaAtualizacao = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Registrar log de atividade
            var log = new LogAtividade
            {
                Acao = "Confirmar",
                Entidade = "Pagamento",
                EntidadeId = pagamento.Id,
                Descricao = $"Pagamento confirmado via webhook",
                DataHora = DateTime.Now
            };
            _context.LogsAtividade.Add(log);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Pagamento processado com sucesso." });
        }

        private bool PagamentoExists(int id)
        {
            return _context.Pagamentos.Any(e => e.Id == id);
        }

        private string GerarCodigoPix(int reservaId, decimal valor)
        {
            // Simulação de geração de código PIX
            // Em um ambiente real, seria integrado com um provedor de pagamentos
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string valorFormatado = valor.ToString("0.00").Replace(".", "");
            return $"{reservaId}_{timestamp}_{valorFormatado}";
        }
    }

    public class WebhookPagamentoDto
    {
        public string CodigoPix { get; set; }
        public string Status { get; set; }
        public DateTime DataPagamento { get; set; }
    }
}
