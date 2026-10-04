namespace Sicoob.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string NomeUsuario { get; set; }
    public string Email { get; set; }
    public string SenhaHash { get; set; }
    public string Perfil { get; set; } // admin ou usuario
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime? UltimoAcesso { get; set; }
}