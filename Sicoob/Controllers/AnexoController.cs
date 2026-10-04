using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sicoob.Data;
using Sicoob.Models;
using Sicoob.Service;

namespace Sicoob.Controllers;

[Authorize]
public class AnexoController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _ambiente;
    private readonly LogService _logService;
    
    private static readonly string[] ExtensoesPermitidas = { ".pdf", ".docx", ".png", ".jpg", ".jpeg" };

    public AnexoController(AppDbContext context, IWebHostEnvironment ambiente, LogService logService)
    {
        _context = context;
        _ambiente = ambiente;
        _logService = logService;
    }


    [HttpGet]
    public async Task<IActionResult> Anexar(int documentoId)
    {
        var documento = await _context.Documentos.FindAsync(documentoId);

        if (documento == null)
        {
            return NotFound();
        }

        ViewBag.Documento = documento;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Anexar(int documentoId, IFormFile arquivo)
    {
        var documento = await _context.Documentos.FindAsync(documentoId);

        if (documento == null)
        {
            return NotFound();
        }

        if (arquivo == null || arquivo.Length == 0)
        {
            ModelState.AddModelError("", "Selecione um arquivo");
            ViewBag.Documento = documento;
            return View();
        }

        var extensao = Path.GetExtension(arquivo.FileName).ToLower();

        if (!ExtensoesPermitidas.Contains(extensao))
        {
            ModelState.AddModelError("", "Formato não permitido. Use PDF, DOCX, PNG, JPG ou JPEG.");
            ViewBag.Documento = documento;
            return View();
        }

        var nomeArquivoUnico = $"{Guid.NewGuid()}{extensao}";
        var caminhoCompleto = Path.Combine(_ambiente.WebRootPath, "uploads", nomeArquivoUnico);

        using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
        {
            await arquivo.CopyToAsync(stream);
        }

        var ultimaVersao = await _context.Anexos
            .Where(a => a.DocumentoId == documentoId)
            .Select(a => (int?)a.Versao)
            .MaxAsync() ?? 0;

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var anexo = new Anexo
        {
            DocumentoId = documentoId,
            NomeArquivo = arquivo.FileName,
            CaminhoArquivo = $"/uploads/{nomeArquivoUnico}",
            Versao = ultimaVersao + 1,
            UsuarioId = userId,
            DataUpload = DateTime.Now,
        };

        _context.Anexos.Add(anexo);
        await _context.SaveChangesAsync();
        await _logService.RegistrarAsync("Upload de Anexo", $"Arquivo '{arquivo.FileName}' anexado ao documento {documento.NumeroDocumento} (versão {anexo.Versao}).");

        TempData["Mensagem"] = "Arquivo anexado com sucesso!";
        return RedirectToAction("Detalhes", "Documento", new { id = documentoId });
    }
  
}