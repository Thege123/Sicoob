using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sicoob.Data;
using Sicoob.Models.ViewModels;
using Sicoob.Service;


namespace Sicoob.Controllers;


public class AccountController: Controller
{

    private readonly AppDbContext _context;
    private readonly LogService _logService;
    
    public AccountController(AppDbContext context, LogService logService)
    {
        _context = context;
        _logService = logService;
    }
    
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NomeUsuario == model.NomeUsuario && u.Ativo);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Senha, usuario.SenhaHash))
        {
            ModelState.AddModelError("" , " Usuário ou senha inválidos.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Role, usuario.Perfil),
            new Claim("NomeUsuario", usuario.NomeUsuario)
        };
        
        

        var identify = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identify);
        
        await  HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        
        await _logService.RegistrarAsync("Login", $"Usuário {usuario.NomeUsuario} realizou login.", usuario.Id);
        
        usuario.UltimoAcesso = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var nomeUsuario = User.FindFirst("NomeUsuario")?.Value;
        await _logService.RegistrarAsync("Logout", $"Usuário {nomeUsuario} realizou logout.");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
    
    [Authorize]
    [HttpGet]
    public IActionResult AlterarSenha()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var usuario = await _context.Usuarios.FindAsync(userId);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.SenhaAtual, usuario.SenhaHash))
        {
            ModelState.AddModelError("SenhaAtual", "Senha atual incorreta.");
            return View(model);
        }

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
        await _context.SaveChangesAsync();

        ViewBag.Mensagem = "Senha alterada com sucesso!";
        return View();
    }
    
    [HttpGet]
    public IActionResult AcessoNegado()
    {
        return View();
    }
}