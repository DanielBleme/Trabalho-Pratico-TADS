using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Trabalho_Pratico.Data;
using Trabalho_Pratico.Models;

namespace Trabalho_Pratico.Controllers
{
    public class VeiculosController : Controller
    {
        private readonly ApplicationContext _context;

        public VeiculosController(ApplicationContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            var veiculos = _context.Veiculos.Include(v => v.Fabricante);
            return View(await veiculos.ToListAsync());
        }

    
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var veiculo = await _context.Veiculos
                .Include(v => v.Fabricante)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (veiculo == null) return NotFound();

            return View(veiculo);
        }

        
        public IActionResult Create()
        {
            ViewData["FabricanteId"] = new SelectList(_context.Fabricantes, "Id", "Nome");
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Modelo,AnoFabricacao,Placa,Quilometragem,FabricanteId")] Veiculo veiculo)
        {
            if (await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa))
            {
                ModelState.AddModelError("Placa", "Esta placa já está cadastrada no sistema.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(veiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FabricanteId"] = new SelectList(_context.Fabricantes, "Id", "Nome", veiculo.FabricanteId);
            return View(veiculo);
        }

       
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null) return NotFound();

            ViewData["FabricanteId"] = new SelectList(_context.Fabricantes, "Id", "Nome", veiculo.FabricanteId);
            return View(veiculo);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Modelo,AnoFabricacao,Placa,Quilometragem,FabricanteId")] Veiculo veiculo)
        {
            if (id != veiculo.Id) return NotFound();

            if (await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa && v.Id != id))
            {
                ModelState.AddModelError("Placa", "Esta placa já está sendo utilizada por outro veículo.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(veiculo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Veiculos.AnyAsync(e => e.Id == veiculo.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["FabricanteId"] = new SelectList(_context.Fabricantes, "Id", "Nome", veiculo.FabricanteId);
            return View(veiculo);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var veiculo = await _context.Veiculos
                .Include(v => v.Fabricante)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (veiculo == null) return NotFound();

            return View(veiculo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null) return NotFound();

            bool possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.VeiculoId == id);
            if (possuiAlugueis)
            {
                ModelState.AddModelError(string.Empty, "Não é possível excluir um veículo com histórico de locações.");
                return View(veiculo);
            }

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}