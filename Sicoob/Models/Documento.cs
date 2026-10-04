namespace Sicoob.Models;

public class Documento
{
    public int Id { get; set; }
    public string TipoDocumento { get; set; } 
    public string NumeroDocumento { get; set; }
    public DateTime DataDocumento { get; set; }
    public string Assunto { get; set; }
    public string Destinatario { get; set; }
    public string Setor { get; set; }
    public string Descricao { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } 

    public DateTime DataCadastro { get; set; } = DateTime.Now;
    public string Status { get; set; }
}