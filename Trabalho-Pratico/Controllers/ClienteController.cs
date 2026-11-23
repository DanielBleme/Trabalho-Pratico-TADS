using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Data;
using Trabalho_Pratico.Models;

namespace Trabalho_Pratico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ClientesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetClientes()
        {
            return Ok(await _context.Clientes.ToListAsync());
        }

        // GET: api/clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Alugueis)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cliente == null) return NotFound(new { mensagem = "Cliente não encontrado." });

            return Ok(cliente);
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> CreateCliente([FromBody] Cliente cliente)
        {
            if (await _context.Clientes.AnyAsync(c => c.CPF == cliente.CPF))
            {
                return BadRequest(new { mensagem = "Já existe um cliente cadastrado com este CPF." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, cliente);
        }

        // PUT: api/clientes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] Cliente cliente)
        {
            if (id != cliente.Id)
            {
                return BadRequest(new { mensagem = "O ID informado na URL difere do ID do corpo da requisição." });
            }

            if (await _context.Clientes.AnyAsync(c => c.CPF == cliente.CPF && c.Id != id))
            {
                return BadRequest(new { mensagem = "Este CPF já está sendo utilizado por outro cliente." });
            }

            _context.Entry(cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Clientes.AnyAsync(e => e.Id == id))
                    return NotFound(new { mensagem = "Cliente não encontrado." });
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound(new { mensagem = "Cliente não encontrado." });

            bool possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.ClienteId == id);
            if (possuiAlugueis)
            {
                return BadRequest(new { mensagem = "Não é possível excluir um cliente com histórico de aluguéis." });
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}