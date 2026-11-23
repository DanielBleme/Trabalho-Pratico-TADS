using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Data;

namespace Trabalho_Pratico.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiltrosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FiltrosController(ApplicationContext context)
        {
            _context = context;
        }

        // 1. GET: api/filtros/veiculos-por-fabricante?marca=Fiat
        [HttpGet("veiculos-por-fabricante")]
        public async Task<IActionResult> VeiculosComFabricantes([FromQuery] string? marca)
        {
            var query = await (from v in _context.Veiculos
                               join f in _context.Fabricantes on v.FabricanteId equals f.Id
                               where string.IsNullOrEmpty(marca) || f.Nome.Contains(marca)
                               select new
                               {
                                   VeiculoId = v.Id,
                                   v.Modelo,
                                   v.Placa,
                                   v.AnoFabricacao,
                                   Fabricante = f.Nome
                               }).ToListAsync();

            return Ok(query);
        }

        // 2. GET: api/filtros/clientes-historico
        [HttpGet("clientes-historico")]
        public async Task<IActionResult> ClientesHistorico()
        {
            var query = await (from c in _context.Clientes
                               join a in _context.Alugueis on c.Id equals a.ClienteId into grupo
                               from aluguel in grupo.DefaultIfEmpty()
                               group aluguel by new { c.Id, c.Nome, c.CPF, c.Email } into g
                               select new
                               {
                                   g.Key.Id,
                                   g.Key.Nome,
                                   g.Key.CPF,
                                   TotalLocacoes = g.Count(x => x != null),
                                   TotalGasto = g.Sum(x => x != null ? (x.ValorTotal ?? 0) : 0)
                               }).ToListAsync();

            return Ok(query);
        }

        // 3. GET: api/filtros/veiculos-nunca-alugados
        [HttpGet("veiculos-nunca-alugados")]
        public async Task<IActionResult> VeiculosNuncaAlugados()
        {
            var query = await (from v in _context.Veiculos
                               join f in _context.Fabricantes on v.FabricanteId equals f.Id
                               join a in _context.Alugueis on v.Id equals a.VeiculoId into grupo
                               from aluguel in grupo.DefaultIfEmpty()
                               where aluguel == null
                               select new
                               {
                                   v.Id,
                                   v.Modelo,
                                   v.Placa,
                                   Fabricante = f.Nome,
                                   v.Quilometragem
                               }).ToListAsync();

            return Ok(query);
        }

        // 4. GET: api/filtros/pagamentos-por-data?inicio=2026-01-01&fim=2026-12-31
        [HttpGet("pagamentos-por-data")]
        public async Task<IActionResult> PagamentosPorData([FromQuery] DateTime? inicio, [FromQuery] DateTime? fim)
        {
            var dataInicio = inicio ?? DateTime.Today.AddMonths(-1);
            var dataFim = fim ?? DateTime.Today;

            var query = await (from p in _context.Pagamentos
                               join a in _context.Alugueis on p.AluguelId equals a.Id
                               join c in _context.Clientes on a.ClienteId equals c.Id
                               where p.DataPagamento >= dataInicio && p.DataPagamento <= dataFim
                               select new
                               {
                                   PagamentoId = p.Id,
                                   Cliente = c.Nome,
                                   p.ValorPago,
                                   p.MetodoPagamento,
                                   p.DataPagamento,
                                   p.Status
                               }).ToListAsync();

            return Ok(query);
        }

        // 5. GET: api/filtros/alugueis-alta-rodagem?kmMinima=100
        [HttpGet("alugueis-alta-rodagem")]
        public async Task<IActionResult> AlugueisAltaRodagem([FromQuery] int kmMinima = 500)
        {
            var query = await (from a in _context.Alugueis
                               join v in _context.Veiculos on a.VeiculoId equals v.Id
                               join c in _context.Clientes on a.ClienteId equals c.Id
                               where a.QuilometragemFinal != null && (a.QuilometragemFinal - a.QuilometragemInicial) >= kmMinima
                               select new
                               {
                                   AluguelId = a.Id,
                                   Cliente = c.Nome,
                                   Veiculo = v.Modelo,
                                   KmPercorrida = a.QuilometragemFinal - a.QuilometragemInicial,
                                   a.ValorTotal
                               }).ToListAsync();

            return Ok(query);
        }
    }
}