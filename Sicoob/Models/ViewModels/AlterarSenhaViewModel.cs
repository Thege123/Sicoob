using System.ComponentModel.DataAnnotations;

namespace Sicoob.Models.ViewModels;

public class AlterarSenhaViewModel
{
        [Required(ErrorMessage = "Informe a senha atual.")]
        public string SenhaAtual { get; set; }

        [Required(ErrorMessage = "Informe a nova senha.")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter no mínimo 6 caracteres.")]
        public string NovaSenha { get; set; }

        [Required(ErrorMessage = "Confirme a nova senha.")]
        [Compare("NovaSenha", ErrorMessage = "A confirmação não bate com a nova senha.")]
        public string ConfirmarNovaSenha { get; set; }
}