using System.ComponentModel.DataAnnotations;

namespace Sicoob.Models.ViewModels;


public class DocumentosViewModel
{
    [Required(ErrorMessage = "Selecione o tipo do documento.")]
    public string TipoDocumento { get; set; } 

    [Required(ErrorMessage = "Informe a data.")]
    public DateTime DataDocumento { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Informe o assunto.")]
    public string Assunto { get; set; }

    [Required(ErrorMessage = "Informe o destinatário.")]
    public string Destinatario { get; set; }

    [Required(ErrorMessage = "Informe o setor/departamento.")]
    public string Setor { get; set; }

    [Required(ErrorMessage = "Informe a descrição.")]
    public string Descricao { get; set; }
}