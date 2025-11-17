using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
     
    /// Interfaz para el servicio de usuarios
    /// Contiene toda la lógica de negocio relacionada con usuarios
     
    public interface IServicioUsuario
    {
        // ==================== GESTIÓN DE USUARIOS ====================

         
        /// Crea un nuevo usuario en el sistema
        /// Si es cliente, crea también su cuenta de ahorro principal
        /// Envía correo de confirmación
         
        Task<ResultadoOperacion<UsuarioDTO>> CrearUsuarioAsync(CrearUsuarioDTO datos);

         
        /// Actualiza los datos de un usuario existente
        /// Si es cliente y hay monto adicional, lo suma a la cuenta principal
         
        Task<ResultadoOperacion> ActualizarUsuarioAsync(ActualizarUsuarioDTO datos);

         
        /// Activa o desactiva un usuario
        /// El administrador no puede cambiar su propio estado
         
        Task<ResultadoOperacion> CambiarEstadoAsync(string usuarioId, string usuarioActualId);

         
        /// Obtiene un usuario por su ID con toda su información
         
        Task<ResultadoOperacion<UsuarioDTO>> ObtenerUsuarioPorIdAsync(string usuarioId);

         
        /// Obtiene un listado paginado de usuarios
        /// Permite filtrar por rol
         
        Task<ResultadoOperacion<ResultadoPaginadoDTO<UsuarioDTO>>> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null,
            string usuarioActualId = null);

        // ==================== DASHBOARDS ====================

         
        /// Obtiene los datos del dashboard del administrador
        /// Incluye indicadores de transacciones, clientes y productos
         
        Task<ResultadoOperacion<DashboardAdminDTO>> ObtenerDashboardAdminAsync();

         
        /// Obtiene los datos del dashboard del cajero
        /// Incluye indicadores del día actual
         
        Task<ResultadoOperacion<DashboardCajeroDTO>> ObtenerDashboardCajeroAsync(string cajeroId);

        // ==================== VALIDACIONES (LÓGICA DE NEGOCIO) ====================

         
        /// Verifica si un nombre de usuario ya existe en el sistema
        /// Usado en validaciones de formularios
         
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, string usuarioIdExcluir = null);

         
        /// Verifica si un correo ya existe en el sistema
        /// Usado en validaciones de formularios
         
        Task<bool> ExisteCorreoAsync(string correo, string usuarioIdExcluir = null);

         
        /// Cuenta cuántos usuarios activos hay del rol especificado
        /// Usado para estadísticas y reportes
         
        Task<int> ContarUsuariosActivosPorRolAsync(string rol);

         
        /// Obtiene todos los clientes activos sin préstamo activo
        /// Usado al asignar nuevos préstamos
         
        Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesSinPrestamoActivoAsync();

         
        /// Obtiene todos los clientes activos del sistema
        /// Usado en listados de selección
         
        Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesActivosAsync();

    }
}