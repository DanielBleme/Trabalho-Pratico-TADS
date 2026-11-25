using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Data;
using Trabalho_Pratico.Models;

namespace Trabalho_Pratico.Controllers;

[ApiController]
[Route("api/veiculos")]
public sealed class VeiculosApiController : ControllerBase
{
    private readonly ApplicationContext _context;

    public VeiculosApiController(ApplicationContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? busca, [FromQuery] int? fabricanteId, CancellationToken cancellationToken)
    {
        var query = _context.Veiculos.AsNoTracking().Include(v => v.Fabricante).AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(v => v.Modelo.Contains(busca) || v.Placa.Contains(busca) || v.Fabricante!.Nome.Contains(busca));
        if (fabricanteId.HasValue)
            query = query.Where(v => v.FabricanteId == fabricanteId.Value);

        return Ok(await query.OrderBy(v => v.Modelo)
            .Select(v => new { v.Id, v.Modelo, v.AnoFabricacao, v.Placa, v.Quilometragem, v.FabricanteId, Fabricante = v.Fabricante!.Nome })
            .ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var veiculo = await _context.Veiculos.AsNoTracking().Include(v => v.Fabricante)
            .Where(v => v.Id == id)
            .Select(v => new { v.Id, v.Modelo, v.AnoFabricacao, v.Placa, v.Quilometragem, v.FabricanteId, Fabricante = v.Fabricante!.Nome })
            .FirstOrDefaultAsync(cancellationToken);
        return veiculo is null ? NotFound(new { mensagem = "Veículo não encontrado." }) : Ok(veiculo);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VeiculoRequest request, CancellationToken cancellationToken)
    {
        if (!await _context.Fabricantes.AnyAsync(f => f.Id == request.FabricanteId, cancellationToken))
            return BadRequest(new { mensagem = "Selecione um fabricante válido." });
        if (await _context.Veiculos.AnyAsync(v => v.Placa == request.Placa.Trim(), cancellationToken))
            return Conflict(new { mensagem = "Esta placa já está cadastrada." });

        var veiculo = new Veiculo
        {
            Modelo = request.Modelo.Trim(), AnoFabricacao = request.AnoFabricacao,
            Placa = request.Placa.Trim().ToUpperInvariant(), Quilometragem = request.Quilometragem,
            FabricanteId = request.FabricanteId
        };
        _context.Veiculos.Add(veiculo);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = veiculo.Id }, new { veiculo.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] VeiculoRequest request, CancellationToken cancellationToken)
    {
        var veiculo = await _context.Veiculos.FindAsync([id], cancellationToken);
        if (veiculo is null) return NotFound(new { mensagem = "Veículo não encontrado." });
        if (!await _context.Fabricantes.AnyAsync(f => f.Id == request.FabricanteId, cancellationToken))
            return BadRequest(new { mensagem = "Selecione um fabricante válido." });
        var placa = request.Placa.Trim().ToUpperInvariant();
        if (await _context.Veiculos.AnyAsync(v => v.Id != id && v.Placa == placa, cancellationToken))
            return Conflict(new { mensagem = "Esta placa já está cadastrada." });

        veiculo.Modelo = request.Modelo.Trim();
        veiculo.AnoFabricacao = request.AnoFabricacao;
        veiculo.Placa = placa;
        veiculo.Quilometragem = request.Quilometragem;
        veiculo.FabricanteId = request.FabricanteId;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var veiculo = await _context.Veiculos.FindAsync([id], cancellationToken);
        if (veiculo is null) return NotFound(new { mensagem = "Veículo não encontrado." });
        if (await _context.Alugueis.AnyAsync(a => a.VeiculoId == id, cancellationToken))
            return Conflict(new { mensagem = "Não é possível excluir um veículo com histórico de aluguéis." });

        _context.Veiculos.Remove(veiculo);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

public sealed class VeiculoRequest
{
    [Required, StringLength(100, MinimumLength = 2)] public string Modelo { get; set; } = string.Empty;
    [Range(1950, 2100)] public int AnoFabricacao { get; set; }
    [Required, StringLength(10, MinimumLength = 7)] public string Placa { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int Quilometragem { get; set; }
    [Range(1, int.MaxValue)] public int FabricanteId { get; set; }
}
