using System.ComponentModel.DataAnnotations;

namespace ArtemisBanking.Application.ViewModels.Usuario
{
    public class UsuarioListaItemViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Usuario")]
        public string NombreUsuario { get; set; }

        [Display(Name = "Cédula")]
        public string Cedula { get; set; }

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Display(Name = "Tipo de Usuario")]
        public string TipoUsuario { get; set; }

        [Display(Name = "Estado")]
        public bool EstaActivo { get; set; }

        public bool PuedeEditar { get; set; }
    }
}
