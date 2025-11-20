using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.DTOs.Api;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioUsuario
    {
        // GESTIÓN DE USUARIOS (Web MVC)

        Task<ResultadoOperacion<UsuarioDTO>> CrearUsuarioAsync(CrearUsuarioDTO datos);
     
        Task<ResultadoOperacion> ActualizarUsuarioAsync(ActualizarUsuarioDTO datos);

        Task<ResultadoOperacion> CambiarEstadoAsync(string usuarioId, string usuarioActualId);
         
        Task<ResultadoOperacion<UsuarioDTO>> ObtenerUsuarioPorIdAsync(string usuarioId);
        
        Task<ResultadoOperacion<ResultadoPaginadoDTO<UsuarioDTO>>> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null,
            string usuarioActualId = null);

        // MÉTODOS PARA API REST (retornan DTOs específicos de API)

    
        Task<ResultadoOperacion<PaginatedResponseDTO<UsuarioApiResponseDTO>>> ObtenerUsuariosPaginadosParaApiAsync(
            int page,
            int pageSize,
            string rol = null,
            bool excludeComercio = false);


        Task<ResultadoOperacion<PaginatedResponseDTO<UsuarioApiResponseDTO>>> ObtenerUsuariosComercioParaApiAsync(
            int page,
            int pageSize);

 
        Task<ResultadoOperacion<UsuarioApiResponseDTO>> CrearUsuarioParaApiAsync(CrearUsuarioDTO datos);

        Task<ResultadoOperacion<UsuarioApiResponseDTO>> CrearUsuarioComercioParaApiAsync(
            int commerceId,
            string nombre,
            string apellido,
            string cedula,
            string email,
            string userName,
            string password);

   
        Task<ResultadoOperacion<UsuarioApiResponseDTO>> ObtenerUsuarioPorIdParaApiAsync(string usuarioId);

        // DASHBOARDS 

        Task<ResultadoOperacion<DashboardAdminDTO>> ObtenerDashboardAdminAsync();

        Task<ResultadoOperacion<DashboardCajeroDTO>> ObtenerDashboardCajeroAsync(string cajeroId);

        // VALIDACIONES (LÓGICA DE NEGOCIO) 

        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, string usuarioIdExcluir = null);

        Task<bool> ExisteCorreoAsync(string correo, string usuarioIdExcluir = null);

        Task<int> ContarUsuariosActivosPorRolAsync(string rol);
 
        Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesSinPrestamoActivoAsync();
        Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesActivosAsync();

    }
}