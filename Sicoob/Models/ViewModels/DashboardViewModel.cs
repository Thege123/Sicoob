namespace Sicoob.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalCA {get; set;}
    public int TotalMI {get; set;}
    public int TotalDocumentosNoAno {get; set;}
    public int TotalUsuarios {get; set;}

    public List<string> MesesLabels {get; set;} = new();
    public List<int> MesesValores { get; set; } = new();
    
    public List<string> UsuariosLabels {get; set;} = new();
    public List<int> UsuariosValores { get; set; } = new();
    
}