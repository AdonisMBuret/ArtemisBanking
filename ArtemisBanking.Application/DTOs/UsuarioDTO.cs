
namespace ArtemisBanking.Application.DTOs
{
    public class UsuarioDTO
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto { get; set; }
        public string Cedula { get; set; }
        public string Correo { get; set; }
        public string NombreUsuario { get; set; }
        public string Rol { get; set; }
        public bool EstaActivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public decimal MontoInicial { get; set; }
    }
}