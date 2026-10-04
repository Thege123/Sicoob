using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sicoob.Models;
using Sicoob.Data;
using Sicoob.Models.ViewModels;

namespace Sicoob.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity!.IsAuthenticated)
        {
            return View(new DashboardViewModel());
        }
        
        var anoAtual = DateTime.Now.Year;

        var model = new DashboardViewModel
        {
            TotalCA = await _context.Documentos.CountAsync(d => d.TipoDocumento == "CA" && d.Status == "Ativo"),
            TotalMI = await _context.Documentos.CountAsync(d => d.TipoDocumento == "MI" && d.Status == "Ativo"),
            TotalDocumentosNoAno = await _context.Documentos.CountAsync(d => d.DataCadastro.Year == anoAtual && d.Status == "Ativo"),
            TotalUsuarios = await _context.Usuarios.CountAsync(u => u.Ativo)
        };
        
        var documentosPorMes = await  _context.Documentos.Where(d => d.DataCadastro.Year == anoAtual  && d.Status == "Ativo").GroupBy(d => d.DataCadastro.Month).Select(g => new {Mes = g.Key, Quantidade = g.Count()}).OrderBy(g => g.Mes).ToListAsync();
        
        var nomeDosMeses = new [] {"Jan" , "Fev", "Mar", "Abr", "Mai", "Jun" , "Jul", "Aug", "Sep", "Oct", "Nov", "Dez"};

        foreach (var item in documentosPorMes)
        {
            model.MesesLabels.Add(nomeDosMeses[item.Mes - 1]);
            model.MesesValores.Add(item.Quantidade);
        }
        
        var documentosPorUsuario = await _context.Documentos.Include(d => d.Usuario).Where(d => d.Status == "Ativo").GroupBy(d => d.Usuario.Nome).Select(g => new {Usuario = g.Key, Quantidade = g.Count() }).ToListAsync();

        foreach (var item in documentosPorUsuario)
        {
            model.UsuariosLabels.Add(item.Usuario);
            model.UsuariosValores.Add(item.Quantidade);
        }
        
        return View(model);
    }



    public IActionResult Privacy()
    {
        return View();
    }
    

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}