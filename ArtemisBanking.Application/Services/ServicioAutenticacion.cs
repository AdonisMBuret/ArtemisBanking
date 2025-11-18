using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.Application.Services
{
     
    /// Servicio que maneja toda la lógica de autenticación
    /// Aquí va TODO lo relacionado con login, logout, confirmación de cuenta, etc.
    /// Los controladores SOLO llaman a estos métodos, sin lógica propia
     
    public class ServicioAutenticacion : IServicioAutenticacion
    {
        // Dependencias que necesitamos para trabajar con usuarios
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly ILogger<ServicioAutenticacion> _logger;

        public ServicioAutenticacion(
            SignInManager<Usuario> signInManager,
            UserManager<Usuario> userManager,
            IServicioCorreo servicioCorreo,
            ILogger<ServicioAutenticacion> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _servicioCorreo = servicioCorreo;
            _logger = logger;
        }
                 
        /// Maneja el proceso completo de login
        /// Valida credenciales, estado de cuenta y retorna el rol para redireccionar
         
        public async Task<ResultadoOperacion<string>> LoginAsync(
            string nombreUsuario,
            string contrasena,
            bool recordarme)
        {
            try
            {
                // 1. Buscar el usuario por nombre de usuario
                var usuario = await _userManager.FindByNameAsync(nombreUsuario);

                if (usuario == null)
                {
                    return ResultadoOperacion<string>.Fallo("Usuario o contraseña incorrectos.");
                }

                // 2. Validar que la cuenta esté activa
                if (!usuario.EstaActivo)
                {
                    return ResultadoOperacion<string>.Fallo(
                        "Su cuenta está inactiva. Es necesario activar la cuenta mediante el enlace enviado al correo electrónico registrado.");
                }

                // 3. Validar que el email esté confirmado
                if (!usuario.EmailConfirmed)
                {
                    return ResultadoOperacion<string>.Fallo(
                        "Debe confirmar su correo electrónico antes de iniciar sesión.");
                }

                // 4. Intentar hacer login con Identity
                var resultado = await _signInManager.PasswordSignInAsync(
                    usuario,
                    contrasena,
                    recordarme,
                    lockoutOnFailure: true);

                if (resultado.Succeeded)
                {
                    _logger.LogInformation($"Usuario {nombreUsuario} inició sesión exitosamente");

                    // Obtener el rol del usuario para saber a dónde redirigir
                    var roles = await _userManager.GetRolesAsync(usuario);
                    var rol = roles.FirstOrDefault() ?? "Cliente";

                    return ResultadoOperacion<string>.Ok(rol, "Login exitoso");
                }

                if (resultado.IsLockedOut)
                {
                    _logger.LogWarning($"Usuario {nombreUsuario} bloqueado por múltiples intentos fallidos");
                    return ResultadoOperacion<string>.Fallo(
                        "Su cuenta ha sido bloqueada temporalmente por múltiples intentos fallidos. Intente nuevamente en 5 minutos.");
                }

                // Si llegamos aquí, la contraseña es incorrecta
                return ResultadoOperacion<string>.Fallo("Usuario o contraseña incorrectos.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de login");
                return ResultadoOperacion<string>.Fallo("Ocurrió un error al iniciar sesión.");
            }
        }

         
        /// Cierra la sesión del usuario actual
         
        public async Task<ResultadoOperacion> LogoutAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("Usuario cerró sesión");
                return ResultadoOperacion.Ok("Sesión cerrada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión");
                return ResultadoOperacion.Fallo("Error al cerrar sesión");
            }
        }

         
        /// Confirma la cuenta del usuario usando el token enviado por correo
         
        public async Task<ResultadoOperacion> ConfirmarCuentaAsync(string usuarioId, string token)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(usuarioId);

                if (usuario == null)
                {
                    return ResultadoOperacion.Fallo("Usuario no encontrado.");
                }

                // Confirmar el email con Identity
                var resultado = await _userManager.ConfirmEmailAsync(usuario, token);

                if (!resultado.Succeeded)
                {
                    return ResultadoOperacion.Fallo(
                        "Error al confirmar la cuenta. El token puede ser inválido o haber expirado.");
                }

                // Activar el usuario
                usuario.EstaActivo = true;
                await _userManager.UpdateAsync(usuario);

                _logger.LogInformation($"Cuenta de usuario {usuario.UserName} confirmada y activada");

                return ResultadoOperacion.Ok("Su cuenta ha sido activada exitosamente. Ahora puede iniciar sesión.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar cuenta");
                return ResultadoOperacion.Fallo("Error al confirmar la cuenta.");
            }
        }

         
        /// Solicita el reseteo de contraseña
        /// Desactiva al usuario temporalmente y envía el token por correo
         
        public async Task<ResultadoOperacion<string>> SolicitarReseteoContrasenaAsync(string nombreUsuario)
        {
            try
            {
                var usuario = await _userManager.FindByNameAsync(nombreUsuario);

                // Por seguridad, no revelar si el usuario existe o no
                if (usuario == null)
                {
                    return ResultadoOperacion<string>.Ok(
                        null,
                        "Si el usuario existe, se ha enviado un correo con instrucciones.");
                }

                // Desactivar el usuario temporalmente
                usuario.EstaActivo = false;
                await _userManager.UpdateAsync(usuario);

                // Generar token de reseteo
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

                // Obtener la URL base desde la configuración o usar una predeterminada
                var urlBase = "https://localhost:7096"; // Cambiar según tu dominio en producción

                // Enviar correo con el link
                await _servicioCorreo.EnviarCorreoReseteoContrasenaAsync(
                    usuario.Email,
                    usuario.UserName,
                    usuario.Id,
                    token,
                    urlBase);

                _logger.LogInformation($"Token de reseteo generado y enviado a {usuario.UserName}");

                return ResultadoOperacion<string>.Ok(
                    token,
                    "Se ha enviado un correo con instrucciones para restablecer la contraseña.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar reseteo de contraseña");
                return ResultadoOperacion<string>.Fallo("Error al procesar la solicitud.");
            }
        }

         
        /// Restablece la contraseña del usuario usando el token
        /// Reactiva al usuario una vez cambiada la contraseña
         
        public async Task<ResultadoOperacion> RestablecerContrasenaAsync(
            string usuarioId,
            string token,
            string nuevaContrasena)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(usuarioId);

                if (usuario == null)
                {
                    return ResultadoOperacion.Fallo("Usuario no encontrado.");
                }

                // Resetear la contraseña con el token
                var resultado = await _userManager.ResetPasswordAsync(usuario, token, nuevaContrasena);

                if (!resultado.Succeeded)
                {
                    var errores = resultado.Errors.Select(e => e.Description).ToList();
                    return ResultadoOperacion.FalloConErrores(
                        "Error al restablecer la contraseña.",
                        errores);
                }

                // Reactivar el usuario
                usuario.EstaActivo = true;
                await _userManager.UpdateAsync(usuario);

                _logger.LogInformation($"Contraseña restablecida para usuario {usuario.UserName}");

                return ResultadoOperacion.Ok("Su contraseña ha sido restablecida exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña");
                return ResultadoOperacion.Fallo("Error al restablecer la contraseña.");
            }
        }

         
        /// Obtiene el rol del usuario para redirigir al home correcto
         
        public async Task<string> ObtenerRolUsuarioAsync(string usuarioId)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(usuarioId);
                if (usuario == null) return null;

                var roles = await _userManager.GetRolesAsync(usuario);
                return roles.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener rol de usuario");
                return null;
            }
        }

         
        /// Verifica si un usuario está autenticado y activo
         
        public async Task<bool> EstaAutenticadoAsync(string usuarioId)
        {
            try
            {
                if (string.IsNullOrEmpty(usuarioId))
                    return false;

                var usuario = await _userManager.FindByIdAsync(usuarioId);
                return usuario != null && usuario.EstaActivo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar autenticación");
                return false;
            }
        }
    }
}