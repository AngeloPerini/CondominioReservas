using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CondominioReservas.API.Data;
using CondominioReservas.API.Models;

namespace CondominioReservas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutenticacaoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AutenticacaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/Autenticacao/Google
        [HttpPost("Google")]
        public async Task<ActionResult<UsuarioAutenticadoDto>> AutenticarGoogle([FromBody] GoogleAuthDto googleAuth)
        {
            if (string.IsNullOrEmpty(googleAuth.Email))
            {
                return BadRequest("Email é obrigatório.");
            }

            // Verificar se o usuário já existe
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == googleAuth.Email && u.Ativo);

            if (usuario == null)
            {
                // Usuário não existe, retornar informação para cadastro
                return Ok(new UsuarioAutenticadoDto
                {
                    Autenticado = false,
                    UsuarioExiste = false,
                    Mensagem = "Usuário não encontrado. É necessário completar o cadastro."
                });
            }

            // Usuário existe, retornar informações
            return Ok(new UsuarioAutenticadoDto
            {
                Autenticado = true,
                UsuarioExiste = true,
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                CPF = usuario.CPF,
                NumeroCasa = usuario.NumeroCasa,
                Rua = usuario.Rua,
                Admin = usuario.Admin,
                Mensagem = "Autenticação realizada com sucesso."
            });
        }

        // POST: api/Autenticacao/ValidarCPF
        [HttpPost("ValidarCPF")]
        public async Task<ActionResult<ValidacaoCpfDto>> ValidarCPF([FromBody] string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
            {
                return BadRequest("CPF é obrigatório.");
            }

            // Remover caracteres não numéricos
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            // Verificar se o CPF tem 11 dígitos
            if (cpf.Length != 11)
            {
                return Ok(new ValidacaoCpfDto
                {
                    Valido = false,
                    Mensagem = "CPF deve ter 11 dígitos."
                });
            }

            // Verificar se todos os dígitos são iguais
            bool todosDigitosIguais = true;
            for (int i = 1; i < cpf.Length; i++)
            {
                if (cpf[i] != cpf[0])
                {
                    todosDigitosIguais = false;
                    break;
                }
            }

            if (todosDigitosIguais)
            {
                return Ok(new ValidacaoCpfDto
                {
                    Valido = false,
                    Mensagem = "CPF inválido."
                });
            }

            // Calcular primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (10 - i);
            }

            int resto = soma % 11;
            int digitoVerificador1 = resto < 2 ? 0 : 11 - resto;

            if (int.Parse(cpf[9].ToString()) != digitoVerificador1)
            {
                return Ok(new ValidacaoCpfDto
                {
                    Valido = false,
                    Mensagem = "CPF inválido."
                });
            }

            // Calcular segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (11 - i);
            }

            resto = soma % 11;
            int digitoVerificador2 = resto < 2 ? 0 : 11 - resto;

            if (int.Parse(cpf[10].ToString()) != digitoVerificador2)
            {
                return Ok(new ValidacaoCpfDto
                {
                    Valido = false,
                    Mensagem = "CPF inválido."
                });
            }

            // Verificar se o CPF já está cadastrado
            bool cpfJaCadastrado = await _context.Usuarios.AnyAsync(u => u.CPF == cpf && u.Ativo);

            if (cpfJaCadastrado)
            {
                return Ok(new ValidacaoCpfDto
                {
                    Valido = true,
                    JaCadastrado = true,
                    Mensagem = "CPF já cadastrado no sistema."
                });
            }

            // CPF válido e não cadastrado
            return Ok(new ValidacaoCpfDto
            {
                Valido = true,
                JaCadastrado = false,
                Mensagem = "CPF válido."
            });
        }
    }

    public class GoogleAuthDto
    {
        public required string Email { get; set; }
        public required string Nome { get; set; }
    }

    public class UsuarioAutenticadoDto
    {
        public bool Autenticado { get; set; }
        public bool UsuarioExiste { get; set; }
        public int UsuarioId { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? CPF { get; set; }
        public string? NumeroCasa { get; set; }
        public string? Rua { get; set; }
        public bool Admin { get; set; }
        public string? Mensagem { get; set; }
    }

    public class ValidacaoCpfDto
    {
        public bool Valido { get; set; }
        public bool JaCadastrado { get; set; }
        public string? Mensagem { get; set; }
    }
}
