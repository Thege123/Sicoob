using Microsoft.EntityFrameworkCore;
using Sicoob.Models;
namespace Sicoob.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Documento> Documentos { get; set; }
    public DbSet<Anexo> Anexos { get; set; }
    public DbSet<Log> Logs { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Documento>()
            .HasIndex(d => d.NumeroDocumento)
            .IsUnique();
    }
}