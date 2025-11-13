using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
    /// <summary>
    /// Servicio para manejar toda la lógica de autenticación
    /// Este servicio encapsula TODA la lógica de negocio relacionada con:
    /// - Login/Logout
    /// - Confirmación de cuentas
    /// - Reseteo de contraseñas
    /// - Validaciones de seguridad
    /// </summary>
    public interface IServicioAutenticacion
    {
        // ==================== LOGIN ====================

        /// <summary>
        /// Realiza el login del usuario
        /// Valida:
        /// - Que el usuario existe
        /// - Que la cuenta está activa
        /// - Que el email está confirmado
        /// - Que la contraseña es correcta
        /// Retorna el rol del usuario para redirigir al home correcto
        /// </summary>
        Task<ResultadoOperacion<string>> LoginAsync(string nombreUsuario, string contrasena, bool recordarme);

        /// <summary>
        /// Cierra la sesión del usuario actual
        /// </summary>
        Task<ResultadoOperacion> LogoutAsync();

        // ==================== CONFIRMACIÓN DE CUENTA ====================

        /// <summary>
        /// Confirma la cuenta de un usuario usando el token
        /// Activa el usuario y confirma su email
        /// </summary>
        Task<ResultadoOperacion> ConfirmarCuentaAsync(string usuarioId, string token);

        // ==================== RESETEO DE CONTRASEÑA ====================

        /// <summary>
        /// Solicita un reseteo de contraseña
        /// Desactiva al usuario temporalmente y genera un token
        /// Retorna el token para enviarlo por correo
        /// </summary>
        Task<ResultadoOperacion<string>> SolicitarReseteoContrasenaAsync(string nombreUsuario);

        /// <summary>
        /// Restablece la contraseña usando el token
        /// Reactiva al usuario una vez cambiada la contraseña
        /// </summary>
        Task<ResultadoOperacion> RestablecerContrasenaAsync(string usuarioId, string token, string nuevaContrasena);

        // ==================== UTILIDADES ====================

        /// <summary>
        /// Obtiene el rol del usuario autenticado actual
        /// Usado para redirigir al home correcto
        /// </summary>
        Task<string> ObtenerRolUsuarioAsync(string usuarioId);

        /// <summary>
        /// Verifica si un usuario está autenticado
        /// </summary>
        Task<bool> EstaAutenticadoAsync(string usuarioId);
    }
}