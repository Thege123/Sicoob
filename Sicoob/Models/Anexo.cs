namespace Sicoob.Models;

public class Anexo
{
    public int Id { get; set; }

    public int DocumentoId { get; set; }
    public Documento Documento { get; set; }

    public string NomeArquivo { get; set; }
    public string CaminhoArquivo { get; set; }
    public int Versao { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }

    public DateTime DataUpload { get; set; } = DateTime.Now;
}