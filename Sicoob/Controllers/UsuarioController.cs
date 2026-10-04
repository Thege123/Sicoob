using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sicoob.Data;
using Sicoob.Models;
using Sicoob.Models.ViewModels;
using Sicoob.Service;

namespace Sicoob.Controllers;

[Authorize(Roles = "Admin")]
public class UsuarioController:Controller
{
    private readonly AppDbContext _context;
    private readonly LogService _logService;
    
    public UsuarioController(AppDbContext context, LogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _context.Usuarios .OrderBy(u => u.Nome).ToListAsync();
        return View(usuarios);
    }
    
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(UsuarioViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Senha))
        {
            ModelState.AddModelError("Senha", "A senha é obrigatória ao criar um usuário.");
        }
        
        var nomeUsuarioExiste = await _context.Usuarios.AnyAsync(u => u.NomeUsuario == model.NomeUsuario);

        if (nomeUsuarioExiste)
        {
            ModelState.AddModelError("NomeUsuario", "Esse nome de usuário já está em uso.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var Usuario = new Usuario
        {
            Nome = model.Nome,
            NomeUsuario = model.NomeUsuario,
            Email = model.Email,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword((model.Senha)),
            Perfil = model.Perfil, Ativo = true,
            DataCriacao = DateTime.Now,
        };

        _context.Usuarios.Add(Usuario);
        await _context.SaveChangesAsync();
        await _logService.RegistrarAsync("Cadastro de Usuário", $"Usuário {Usuario.NomeUsuario} cadastrado com perfil {Usuario.Perfil}.");
        
        TempData["Mensagem"] = "Usuário criado com sucesso!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound();
        }

        var model = new UsuarioViewModel
        {
            Nome = usuario.Nome,
            NomeUsuario = usuario.NomeUsuario,
            Email = usuario.Email,
            Perfil = usuario.Perfil,
        };
        
        ViewBag.UsuarioId = usuario.Id;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, UsuarioViewModel model)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(model.Senha))
        {
            ModelState.Remove(nameof(model.Senha));
        }

        var nomeUsuarioExiste = await _context.Usuarios
            .AnyAsync(u => u.NomeUsuario == model.NomeUsuario && u.Id != id);

        if (nomeUsuarioExiste)
        {
            ModelState.AddModelError("NomeUsuario", "Esse nome de usuário já está em uso.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.UsuarioId = usuario.Id;
            return View(model);
        }

        var perfilAntigo = usuario.Perfil;

        usuario.Nome = model.Nome;
        usuario.NomeUsuario = model.NomeUsuario;
        usuario.Email = model.Email;
        usuario.Perfil = model.Perfil;

        if (!string.IsNullOrWhiteSpace(model.Senha))
        {
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.Senha);
        }

        await _context.SaveChangesAsync();

        if (perfilAntigo != usuario.Perfil)
        {
            await _logService.RegistrarAsync("Alteração de Permissões",
                $"Perfil do usuário {usuario.NomeUsuario} alterado de {perfilAntigo} para {usuario.Perfil}.");
        }
        else
        {
            await _logService.RegistrarAsync("Alteração de Usuário",
                $"Dados do usuário {usuario.NomeUsuario} atualizados.");
        }

        TempData["Mensagem"] = "Usuário atualizado com sucesso!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> AlternarStatus(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return NotFound();
        }
        
        usuario.Ativo = !usuario.Ativo;
        await _context.SaveChangesAsync();
        
        var statusTexto = usuario.Ativo ? "reativado" : "desativado";
        await _logService.RegistrarAsync("Alteração de Usuário", $"Usuário {usuario.NomeUsuario} {statusTexto}.");

        TempData["Mensagem"] = $"Usuário {statusTexto} com sucesso!";
        return RedirectToAction("Index");
    }
}