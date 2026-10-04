namespace Sicoob.Models;

public class Log
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }

    public string Acao { get; set; }
    public string Descricao { get; set; }
    public string Ip { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
}