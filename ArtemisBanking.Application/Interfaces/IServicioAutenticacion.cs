using ArtemisBanking.Application.DTOs;

namespace ArtemisBanking.Application.Interfaces
{
     
    /// Servicio para manejar toda la lógica de autenticación
    /// Este servicio encapsula TODA la lógica de negocio relacionada con:
    /// - Login/Logout
    /// - Confirmación de cuentas
    /// - Reseteo de contraseñas
    /// - Validaciones de seguridad
     
    public interface IServicioAutenticacion
    {
        // ==================== LOGIN ====================
                 
        /// Realiza el login del usuario
        /// Valida:
        /// - Que el usuario existe
        /// - Que la cuenta está activa
        /// - Que el email está confirmado
        /// - Que la contraseña es correcta
        /// Retorna el rol del usuario para redirigir al home correcto
         
        Task<ResultadoOperacion<string>> LoginAsync(string nombreUsuario, string contrasena, bool recordarme);

         
        /// Cierra la sesión del usuario actual
         
        Task<ResultadoOperacion> LogoutAsync();

        // ==================== CONFIRMACIÓN DE CUENTA ====================
                 
        /// Confirma la cuenta de un usuario usando el token
        /// Activa el usuario y confirma su email
         
        Task<ResultadoOperacion> ConfirmarCuentaAsync(string usuarioId, string token);

        // ==================== RESETEO DE CONTRASEÑA ====================
                 
        /// Solicita un reseteo de contraseña
        /// Desactiva al usuario temporalmente y genera un token
        /// Retorna el token para enviarlo por correo
         
        Task<ResultadoOperacion<string>> SolicitarReseteoContrasenaAsync(string nombreUsuario);

         
        /// Restablece la contraseña usando el token
        /// Reactiva al usuario una vez cambiada la contraseña
         
        Task<ResultadoOperacion> RestablecerContrasenaAsync(string usuarioId, string token, string nuevaContrasena);

        // ==================== UTILIDADES ====================
                 
        /// Obtiene el rol del usuario autenticado actual
        /// Usado para redirigir al home correcto
         
        Task<string> ObtenerRolUsuarioAsync(string usuarioId);

         
        /// Verifica si un usuario está autenticado
         
        Task<bool> EstaAutenticadoAsync(string usuarioId);
    }
}