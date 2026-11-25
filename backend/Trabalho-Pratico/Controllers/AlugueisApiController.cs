using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Data;
using Trabalho_Pratico.Models;

namespace Trabalho_Pratico.Controllers;

[ApiController]
[Route("api/alugueis")]
public sealed class AlugueisApiController : ControllerBase
{
    private readonly ApplicationContext _context;

    public AlugueisApiController(ApplicationContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? busca, [FromQuery] string? situacao, CancellationToken cancellationToken)
    {
        var query = _context.Alugueis.AsNoTracking().Include(a => a.Cliente).Include(a => a.Veiculo).AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(a => a.Cliente!.Nome.Contains(busca)
                || a.Veiculo!.Modelo.Contains(busca)
                || a.Veiculo.Placa.Contains(busca)
                || a.Veiculo.Fabricante!.Nome.Contains(busca));

        var rows = await query.OrderByDescending(a => a.DataInicio)
            .Select(a => new
            {
                a.Id, a.ClienteId, Cliente = a.Cliente!.Nome, a.VeiculoId,
                Veiculo = a.Veiculo!.Modelo, Placa = a.Veiculo.Placa,
                a.DataInicio, a.DataFimPrevista, a.DataDevolucao,
                a.QuilometragemInicial, a.QuilometragemFinal, a.ValorDiaria, a.ValorTotal
            })
            .ToListAsync(cancellationToken);

        var now = DateTime.Now;
        var result = rows.Select(a => new
        {
            a.Id, a.ClienteId, a.Cliente, a.VeiculoId, a.Veiculo, a.Placa,
            a.DataInicio, a.DataFimPrevista, a.DataDevolucao,
            a.QuilometragemInicial, a.QuilometragemFinal, a.ValorDiaria, a.ValorTotal,
            Situacao = Status(a.DataInicio, a.DataFimPrevista, a.DataDevolucao, now)
        });
        if (!string.IsNullOrWhiteSpace(situacao))
            result = result.Where(a => string.Equals(a.Situacao, situacao, StringComparison.OrdinalIgnoreCase));

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var aluguel = await _context.Alugueis.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id, a.ClienteId, Cliente = a.Cliente!.Nome, a.VeiculoId,
                Veiculo = a.Veiculo!.Modelo, Placa = a.Veiculo.Placa,
                a.DataInicio, a.DataFimPrevista, a.DataDevolucao,
                a.QuilometragemInicial, a.QuilometragemFinal, a.ValorDiaria, a.ValorTotal
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (aluguel is null) return NotFound(new { mensagem = "Aluguel não encontrado." });
        return Ok(new { aluguel.Id, aluguel.ClienteId, aluguel.Cliente, aluguel.VeiculoId, aluguel.Veiculo, aluguel.Placa, aluguel.DataInicio, aluguel.DataFimPrevista, aluguel.DataDevolucao, aluguel.QuilometragemInicial, aluguel.QuilometragemFinal, aluguel.ValorDiaria, aluguel.ValorTotal, Situacao = Status(aluguel.DataInicio, aluguel.DataFimPrevista, aluguel.DataDevolucao, DateTime.Now) });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AluguelRequest request, CancellationToken cancellationToken)
    {
        if (request.DataFimPrevista <= request.DataInicio)
            return BadRequest(new { mensagem = "A devolução prevista deve ser posterior à retirada." });
        if (request.ValorDiaria <= 0)
            return BadRequest(new { mensagem = "O valor da diária precisa ser maior que zero." });
        var veiculo = await _context.Veiculos.FindAsync([request.VeiculoId], cancellationToken);
        if (veiculo is null) return BadRequest(new { mensagem = "Selecione um veículo válido." });
        if (!await _context.Clientes.AnyAsync(c => c.Id == request.ClienteId, cancellationToken))
            return BadRequest(new { mensagem = "Selecione um cliente válido." });

        var conflict = await _context.Alugueis.AnyAsync(a => a.VeiculoId == request.VeiculoId
            && a.DataInicio < request.DataFimPrevista
            && (a.DataDevolucao ?? a.DataFimPrevista) > request.DataInicio, cancellationToken);
        if (conflict) return Conflict(new { mensagem = "O veículo já está reservado nesse período." });

        var aluguel = new Aluguel
        {
            ClienteId = request.ClienteId,
            VeiculoId = request.VeiculoId,
            DataInicio = request.DataInicio,
            DataFimPrevista = request.DataFimPrevista,
            ValorDiaria = request.ValorDiaria,
            QuilometragemInicial = veiculo.Quilometragem
        };
        _context.Alugueis.Add(aluguel);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = aluguel.Id }, new { aluguel.Id });
    }

    [HttpPost("{id:int}/devolucao")]
    public async Task<IActionResult> Devolucao(int id, [FromBody] DevolucaoRequest request, CancellationToken cancellationToken)
    {
        var aluguel = await _context.Alugueis.Include(a => a.Veiculo).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (aluguel is null) return NotFound(new { mensagem = "Aluguel não encontrado." });
        if (aluguel.DataDevolucao.HasValue) return Conflict(new { mensagem = "Este veículo já foi devolvido." });
        if (request.DataDevolucao < aluguel.DataInicio) return BadRequest(new { mensagem = "A devolução não pode anteceder a retirada." });
        if (request.DataDevolucao > DateTime.Now.AddMinutes(5)) return BadRequest(new { mensagem = "A data de devolução não pode estar no futuro." });
        if (request.QuilometragemFinal < aluguel.QuilometragemInicial) return BadRequest(new { mensagem = "A quilometragem final não pode ser inferior à inicial." });

        aluguel.DataDevolucao = request.DataDevolucao;
        aluguel.QuilometragemFinal = request.QuilometragemFinal;
        aluguel.ValorTotal = Math.Max(1, (request.DataDevolucao.Date - aluguel.DataInicio.Date).Days) * aluguel.ValorDiaria;
        aluguel.Veiculo!.Quilometragem = request.QuilometragemFinal;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string Status(DateTime inicio, DateTime fim, DateTime? devolucao, DateTime agora)
    {
        if (devolucao.HasValue) return "Finalizado";
        if (inicio > agora) return "Reservado";
        if (fim < agora) return "Atrasado";
        return "Em viagem";
    }
}

public sealed class AluguelRequest
{
    [Range(1, int.MaxValue)] public int ClienteId { get; set; }
    [Range(1, int.MaxValue)] public int VeiculoId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFimPrevista { get; set; }
    public decimal ValorDiaria { get; set; }
}

public sealed class DevolucaoRequest
{
    public DateTime DataDevolucao { get; set; }
    [Range(0, int.MaxValue)] public int QuilometragemFinal { get; set; }
}
