using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de usuarios
    /// Contiene toda la lógica de negocio relacionada con usuarios
    /// </summary>
    public interface IServicioUsuario
    {
        // ==================== GESTIÓN DE USUARIOS ====================

        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// Si es cliente, crea también su cuenta de ahorro principal
        /// Envía correo de confirmación
        /// </summary>
        Task<ResultadoOperacion<UsuarioDTO>> CrearUsuarioAsync(CrearUsuarioDTO datos);

        /// <summary>
        /// Actualiza los datos de un usuario existente
        /// Si es cliente y hay monto adicional, lo suma a la cuenta principal
        /// </summary>
        Task<ResultadoOperacion> ActualizarUsuarioAsync(ActualizarUsuarioDTO datos);

        /// <summary>
        /// Activa o desactiva un usuario
        /// El administrador no puede cambiar su propio estado
        /// </summary>
        Task<ResultadoOperacion> CambiarEstadoAsync(string usuarioId, string usuarioActualId);

        /// <summary>
        /// Obtiene un usuario por su ID con toda su información
        /// </summary>
        Task<ResultadoOperacion<UsuarioDTO>> ObtenerUsuarioPorIdAsync(string usuarioId);

        /// <summary>
        /// Obtiene un listado paginado de usuarios
        /// Permite filtrar por rol
        /// </summary>
        Task<ResultadoOperacion<ResultadoPaginadoDTO<UsuarioDTO>>> ObtenerUsuariosPaginadosAsync(
            int pagina,
            int tamano,
            string rol = null,
            string usuarioActualId = null);

        // ==================== DASHBOARDS ====================

        /// <summary>
        /// Obtiene los datos del dashboard del administrador
        /// Incluye indicadores de transacciones, clientes y productos
        /// </summary>
        Task<ResultadoOperacion<DashboardAdminDTO>> ObtenerDashboardAdminAsync();

        /// <summary>
        /// Obtiene los datos del dashboard del cajero
        /// Incluye indicadores del día actual
        /// </summary>
        Task<ResultadoOperacion<DashboardCajeroDTO>> ObtenerDashboardCajeroAsync(string cajeroId);

        // ==================== VALIDACIONES (LÓGICA DE NEGOCIO) ====================

        /// <summary>
        /// Verifica si un nombre de usuario ya existe en el sistema
        /// Usado en validaciones de formularios
        /// </summary>
        Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, string usuarioIdExcluir = null);

        /// <summary>
        /// Verifica si un correo ya existe en el sistema
        /// Usado en validaciones de formularios
        /// </summary>
        Task<bool> ExisteCorreoAsync(string correo, string usuarioIdExcluir = null);

        /// <summary>
        /// Cuenta cuántos usuarios activos hay del rol especificado
        /// Usado para estadísticas y reportes
        /// </summary>
        Task<int> ContarUsuariosActivosPorRolAsync(string rol);

        /// <summary>
        /// Obtiene todos los clientes activos sin préstamo activo
        /// Usado al asignar nuevos préstamos
        /// </summary>
        Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesSinPrestamoActivoAsync();

        /// <summary>
        /// Obtiene todos los clientes activos del sistema
        /// Usado en listados de selección
        /// </summary>
        Task<ResultadoOperacion<IEnumerable<UsuarioDTO>>> ObtenerClientesActivosAsync();
    }
}