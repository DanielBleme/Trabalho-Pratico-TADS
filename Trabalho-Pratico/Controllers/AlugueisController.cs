using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Data;
using Trabalho_Pratico.Models;

namespace Trabalho_Pratico.Controllers
{
    public class AlugueisController : Controller
    {
        private readonly ApplicationContext _context;

        public AlugueisController(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var alugueis = _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo);
            return View(await alugueis.ToListAsync());
        }

       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .Include(a => a.Pagamentos)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (aluguel == null) return NotFound();

            return View(aluguel);
        }

        
        public IActionResult Create()
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nome");
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Modelo");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId,VeiculoId,DataInicio,DataFimPrevista,ValorDiaria")] Aluguel aluguel)
        {
            var veiculo = await _context.Veiculos.FindAsync(aluguel.VeiculoId);
            if (veiculo == null)
            {
                ModelState.AddModelError("VeiculoId", "Veículo inválido.");
            }

            bool veiculoOcupado = await _context.Alugueis.AnyAsync(a => a.VeiculoId == aluguel.VeiculoId && a.DataDevolucao == null);
            if (veiculoOcupado)
            {
                ModelState.AddModelError("VeiculoId", "Este veículo já está alugado no momento.");
            }

            if (ModelState.IsValid)
            {
                aluguel.QuilometragemInicial = veiculo!.Quilometragem;
                _context.Add(aluguel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClienteId"] = new SelectList(_context.Clientes, "Id", "Nome", aluguel.ClienteId);
            ViewData["VeiculoId"] = new SelectList(_context.Veiculos, "Id", "Modelo", aluguel.VeiculoId);
            return View(aluguel);
        }

        public async Task<IActionResult> Devolucao(int? id)
        {
            if (id == null) return NotFound();

            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (aluguel == null || aluguel.DataDevolucao != null)
                return BadRequest("Locação já encerrada ou inexistente.");

            return View(aluguel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Devolucao(int id, DateTime dataDevolucao, int quilometragemFinal)
        {
            var aluguel = await _context.Alugueis.Include(a => a.Veiculo).FirstOrDefaultAsync(a => a.Id == id);
            if (aluguel == null) return NotFound();

            if (quilometragemFinal < aluguel.QuilometragemInicial)
            {
                ModelState.AddModelError(string.Empty, "A quilometragem final não pode ser inferior à inicial.");
                return View(aluguel);
            }

            aluguel.DataDevolucao = dataDevolucao;
            aluguel.QuilometragemFinal = quilometragemFinal;
            aluguel.Veiculo!.Quilometragem = quilometragemFinal;

            int dias = Math.Max(1, (dataDevolucao.Date - aluguel.DataInicio.Date).Days);
            aluguel.ValorTotal = dias * aluguel.ValorDiaria;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}