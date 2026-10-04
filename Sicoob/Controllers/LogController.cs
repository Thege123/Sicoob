using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sicoob.Data;

namespace Sicoob.Controllers;

[Authorize(Roles = "Admin")]
public class LogController : Controller
{
    private readonly AppDbContext _context;
    
    public LogController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int pagina = 1)
    {
        const int itensPorPagina = 20;
        
        var totalLogs = await _context.Logs.CountAsync();
        var totalPaginas = (int)Math.Ceiling(totalLogs / (double)itensPorPagina);
        
        var logs = await _context.Logs.Include(l => l.Usuario).OrderByDescending(l => l.DataHora).Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina).ToListAsync();

        ViewBag.PaginaAtual = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        
        return View(logs);

    }
}