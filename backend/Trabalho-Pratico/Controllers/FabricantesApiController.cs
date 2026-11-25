using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Data;
using Trabalho_Pratico.Models;

namespace Trabalho_Pratico.Controllers;

[ApiController]
[Route("api/fabricantes")]
public sealed class FabricantesApiController : ControllerBase
{
    private readonly ApplicationContext _context;

    public FabricantesApiController(ApplicationContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? busca, CancellationToken cancellationToken)
    {
        var query = _context.Fabricantes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(f => f.Nome.Contains(busca) || f.CNPJ.Contains(busca));

        var fabricantes = await query.OrderBy(f => f.Nome)
            .Select(f => new { f.Id, f.Nome, f.CNPJ, TotalVeiculos = f.Veiculos.Count })
            .ToListAsync(cancellationToken);
        return Ok(fabricantes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var fabricante = await _context.Fabricantes.AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new { f.Id, f.Nome, f.CNPJ, TotalVeiculos = f.Veiculos.Count })
            .FirstOrDefaultAsync(cancellationToken);
        return fabricante is null ? NotFound(new { mensagem = "Fabricante não encontrado." }) : Ok(fabricante);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Fabricante fabricante, CancellationToken cancellationToken)
    {
        if (await _context.Fabricantes.AnyAsync(f => f.CNPJ == fabricante.CNPJ, cancellationToken))
            return Conflict(new { mensagem = "Já existe um fabricante com este CNPJ." });

        _context.Fabricantes.Add(fabricante);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = fabricante.Id }, fabricante);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Fabricante dados, CancellationToken cancellationToken)
    {
        var fabricante = await _context.Fabricantes.FindAsync([id], cancellationToken);
        if (fabricante is null) return NotFound(new { mensagem = "Fabricante não encontrado." });
        if (await _context.Fabricantes.AnyAsync(f => f.Id != id && f.CNPJ == dados.CNPJ, cancellationToken))
            return Conflict(new { mensagem = "Este CNPJ já está cadastrado." });

        fabricante.Nome = dados.Nome.Trim();
        fabricante.CNPJ = dados.CNPJ.Trim();
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var fabricante = await _context.Fabricantes.FindAsync([id], cancellationToken);
        if (fabricante is null) return NotFound(new { mensagem = "Fabricante não encontrado." });
        if (await _context.Veiculos.AnyAsync(v => v.FabricanteId == id, cancellationToken))
            return Conflict(new { mensagem = "Remova ou transfira os veículos antes de excluir este fabricante." });

        _context.Fabricantes.Remove(fabricante);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
