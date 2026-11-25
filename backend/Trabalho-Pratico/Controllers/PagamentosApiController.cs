using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Data;
using Trabalho_Pratico.Models;

namespace Trabalho_Pratico.Controllers;

[ApiController]
[Route("api/pagamentos")]
public sealed class PagamentosApiController : ControllerBase
{
    private readonly ApplicationContext _context;

    public PagamentosApiController(ApplicationContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? busca, [FromQuery] string? status, [FromQuery] DateTime? inicio, [FromQuery] DateTime? fim, CancellationToken cancellationToken)
    {
        var query = _context.Pagamentos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(p => p.MetodoPagamento.Contains(busca)
                || p.Aluguel!.Cliente!.Nome.Contains(busca)
                || p.Aluguel.Veiculo!.Modelo.Contains(busca)
                || p.Aluguel.Veiculo.Placa.Contains(busca));
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);
        if (inicio.HasValue) query = query.Where(p => p.DataPagamento >= inicio.Value.Date);
        if (fim.HasValue) query = query.Where(p => p.DataPagamento < fim.Value.Date.AddDays(1));
        if (inicio.HasValue && fim.HasValue && fim.Value.Date < inicio.Value.Date)
            return BadRequest(new { mensagem = "A data final precisa ser igual ou posterior à data inicial." });

        var pagamentos = await query.OrderByDescending(p => p.DataPagamento)
            .Select(p => new
            {
                p.Id, p.AluguelId, p.DataPagamento, p.ValorPago, p.MetodoPagamento, p.Status,
                Cliente = p.Aluguel!.Cliente!.Nome,
                Veiculo = p.Aluguel.Veiculo!.Modelo,
                Placa = p.Aluguel.Veiculo.Placa
            })
            .ToListAsync(cancellationToken);
        return Ok(pagamentos);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var pagamento = await _context.Pagamentos.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id, p.AluguelId, p.DataPagamento, p.ValorPago, p.MetodoPagamento, p.Status,
                Cliente = p.Aluguel!.Cliente!.Nome,
                Veiculo = p.Aluguel.Veiculo!.Modelo,
                Placa = p.Aluguel.Veiculo.Placa
            })
            .FirstOrDefaultAsync(cancellationToken);
        return pagamento is null ? NotFound(new { mensagem = "Pagamento não encontrado." }) : Ok(pagamento);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PagamentoRequest request, CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null) return BadRequest(new { mensagem = validationError });
        if (!await _context.Alugueis.AnyAsync(a => a.Id == request.AluguelId, cancellationToken))
            return BadRequest(new { mensagem = "Selecione um aluguel válido." });

        var pagamento = new Pagamento
        {
            AluguelId = request.AluguelId,
            DataPagamento = request.DataPagamento,
            ValorPago = request.ValorPago,
            MetodoPagamento = request.MetodoPagamento.Trim(),
            Status = request.Status.Trim()
        };
        _context.Pagamentos.Add(pagamento);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = pagamento.Id }, new { pagamento.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PagamentoRequest request, CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null) return BadRequest(new { mensagem = validationError });
        var pagamento = await _context.Pagamentos.FindAsync([id], cancellationToken);
        if (pagamento is null) return NotFound(new { mensagem = "Pagamento não encontrado." });
        if (!await _context.Alugueis.AnyAsync(a => a.Id == request.AluguelId, cancellationToken))
            return BadRequest(new { mensagem = "Selecione um aluguel válido." });

        pagamento.AluguelId = request.AluguelId;
        pagamento.DataPagamento = request.DataPagamento;
        pagamento.ValorPago = request.ValorPago;
        pagamento.MetodoPagamento = request.MetodoPagamento.Trim();
        pagamento.Status = request.Status.Trim();
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var pagamento = await _context.Pagamentos.FindAsync([id], cancellationToken);
        if (pagamento is null) return NotFound(new { mensagem = "Pagamento não encontrado." });
        _context.Pagamentos.Remove(pagamento);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string? Validate(PagamentoRequest request)
    {
        if (request.ValorPago <= 0) return "O valor pago precisa ser maior que zero.";
        if (request.DataPagamento == default) return "Informe a data do pagamento.";
        if (request.DataPagamento > DateTime.Now.AddMinutes(5)) return "A data do pagamento não pode estar no futuro.";
        if (!new[] { "Pago", "Parcial", "Pendente" }.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
            return "Selecione um status de pagamento válido.";
        return null;
    }
}

public sealed class PagamentoRequest
{
    [Range(1, int.MaxValue)] public int AluguelId { get; set; }
    public DateTime DataPagamento { get; set; }
    public decimal ValorPago { get; set; }
    [Required, StringLength(50, MinimumLength = 2)] public string MetodoPagamento { get; set; } = string.Empty;
    [Required, StringLength(30, MinimumLength = 2)] public string Status { get; set; } = string.Empty;
}
