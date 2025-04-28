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
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Where(u => u.Ativo)
                .ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null || !usuario.Ativo)
            {
                return NotFound();
            }

            return usuario;
        }

        // GET: api/Usuarios/Email/usuario@exemplo.com
        [HttpGet("Email/{email}")]
        public async Task<ActionResult<Usuario>> GetUsuarioByEmail(string email)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // GET: api/Usuarios/CPF/12345678900
        [HttpGet("CPF/{cpf}")]
        public async Task<ActionResult<Usuario>> GetUsuarioByCpf(string cpf)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.CPF == cpf && u.Ativo);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest();
            }

            usuario.UltimaAtualizacao = DateTime.Now;

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Registrar log de atividade
                var log = new LogAtividade
                {
                    UsuarioId = usuario.Id,
                    Acao = "Atualizar",
                    Entidade = "Usuario",
                    EntidadeId = usuario.Id,
                    Descricao = $"Usuário {usuario.Nome} atualizado",
                    DataHora = DateTime.Now
                };
                _context.LogsAtividade.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
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

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // Verificar se já existe usuário com o mesmo email ou CPF
            bool emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
            bool cpfExiste = await _context.Usuarios.AnyAsync(u => u.CPF == usuario.CPF);

            if (emailExiste)
            {
                return BadRequest("Já existe um usuário com este email.");
            }

            if (cpfExiste)
            {
                return BadRequest("Já existe um usuário com este CPF.");
            }

            usuario.DataCriacao = DateTime.Now;
            usuario.Ativo = true;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Registrar log de atividade
            var log = new LogAtividade
            {
                UsuarioId = usuario.Id,
                Acao = "Criar",
                Entidade = "Usuario",
                EntidadeId = usuario.Id,
                Descricao = $"Usuário {usuario.Nome} criado",
                DataHora = DateTime.Now
            };
            _context.LogsAtividade.Add(log);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // Em vez de excluir, apenas desativa o usuário
            usuario.Ativo = false;
            usuario.UltimaAtualizacao = DateTime.Now;

            await _context.SaveChangesAsync();

            // Registrar log de atividade
            var log = new LogAtividade
            {
                UsuarioId = usuario.Id,
                Acao = "Desativar",
                Entidade = "Usuario",
                EntidadeId = usuario.Id,
                Descricao = $"Usuário {usuario.Nome} desativado",
                DataHora = DateTime.Now
            };
            _context.LogsAtividade.Add(log);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
