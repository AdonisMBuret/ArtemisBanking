using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    public interface IServicioUsuario
    {
        // GESTIÓN DE USUARIOS 

        Task<ResultadoOperacion<UsuarioDTO>> CrearUsuarioAsync(CrearUsuarioDTO datos);
     
        Task<ResultadoOperacion> ActualizarUsuarioAsync(ActualizarUsuarioDTO datos);

        Task<ResultadoOperacion> CambiarEstadoAsync(string usuarioId, string usuarioActualId);
         
        Task<ResultadoOperacion<UsuarioDTO>> ObtenerUsuarioPorIdAsync(string usuarioId);
        
        Task<ResultadoOperacion<ResultadoPaginadoDTO<UsuarioDTO>>> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null,
            string usuarioActualId = null);

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