using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Sicoob.Data;
using Sicoob.Models;
    
namespace Sicoob.Service;

public class LogService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public LogService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task RegistrarAsync(string acao, string descricao, int? usuarioIdExplicito = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        int userId;
        if (usuarioIdExplicito.HasValue)
        {
            userId = usuarioIdExplicito.Value;
        }
        else
        {
            var userIdClaim  = httpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            userId = userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }
        
       
        var ip = httpContext.Connection.RemoteIpAddress.ToString() ??  "desconhecido" ;

        var log = new Log
        {
            UsuarioId = userId,
            Acao = acao,
            Descricao = descricao,
            Ip = ip,
            DataHora = DateTime.Now
        };
        
        _context.Logs.Add(log);
        await _context.SaveChangesAsync();

    }
}