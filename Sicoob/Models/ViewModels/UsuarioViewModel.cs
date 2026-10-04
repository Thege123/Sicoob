
using System.ComponentModel.DataAnnotations;

namespace Sicoob.Models.ViewModels;

public class UsuarioViewModel
{
    [Required(ErrorMessage = "Informe o nome")]
    public string Nome { get; set; }
    
    [Required(ErrorMessage = "Informe o nome de usuário.")]
    public string NomeUsuario { get; set; }
    
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; }
    
    [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")]
    public string? Senha { get; set; }
    
    [Required(ErrorMessage = "Selecione o perfil.")]
    public string Perfil { get; set; }
}