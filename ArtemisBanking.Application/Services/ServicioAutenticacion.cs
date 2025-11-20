using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArtemisBanking.Application.Services
{
     
    /// Servicio que maneja toda la lógica de autenticación
    /// Aquí va TODO lo relacionado con login, logout, confirmación de cuenta, etc.
     
    public class ServicioAutenticacion : IServicioAutenticacion
    {
        private readonly SignInManager<Usuario> _signInManager;
        private readonly UserManager<Usuario> _userManager;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServicioAutenticacion> _logger;

        public ServicioAutenticacion(
            SignInManager<Usuario> signInManager,
            UserManager<Usuario> userManager,
            IServicioCorreo servicioCorreo,
            IConfiguration configuration,
            ILogger<ServicioAutenticacion> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _servicioCorreo = servicioCorreo;
            _configuration = configuration;
            _logger = logger;
        }
                 
       
        public async Task<ResultadoOperacion<string>> LoginAsync(
            string nombreUsuario,
            string contrasena,
            bool recordarme)
        {
            try
            {
                var usuario = await _userManager.FindByNameAsync(nombreUsuario);

                if (usuario == null)
                {
                    return ResultadoOperacion<string>.Fallo("Usuario o contraseña incorrectos.");
                }

                if (!usuario.EstaActivo)
                {
                    return ResultadoOperacion<string>.Fallo(
                        "Su cuenta está inactiva. Es necesario activar la cuenta mediante el enlace enviado al correo electrónico registrado.");
                }

                if (!usuario.EmailConfirmed)
                {
                    return ResultadoOperacion<string>.Fallo(
                        "Debe confirmar su correo electrónico antes de iniciar sesión.");
                }

                
                var resultado = await _signInManager.PasswordSignInAsync(
                    usuario,
                    contrasena,
                    recordarme,
                    lockoutOnFailure: true);

                if (resultado.Succeeded)
                {
                    _logger.LogInformation($"Usuario {nombreUsuario} inició sesión exitosamente");

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

                return ResultadoOperacion<string>.Fallo("Usuario o contraseña incorrectos.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de login");
                return ResultadoOperacion<string>.Fallo("Ocurrió un error al iniciar sesión.");
            }
        }

         
         
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

         
         
        public async Task<ResultadoOperacion> ConfirmarCuentaAsync(string usuarioId, string token)
        {
            try
            {
                var usuario = await _userManager.FindByIdAsync(usuarioId);

                if (usuario == null)
                {
                    return ResultadoOperacion.Fallo("Usuario no encontrado.");
                }

               
                var resultado = await _userManager.ConfirmEmailAsync(usuario, token);

                if (!resultado.Succeeded)
                {
                    return ResultadoOperacion.Fallo(
                        "Error al confirmar la cuenta. El token puede ser inválido o haber expirado.");
                }

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

                usuario.EstaActivo = false;
                await _userManager.UpdateAsync(usuario);

                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

                var urlBase = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7103";

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

                var resultado = await _userManager.ResetPasswordAsync(usuario, token, nuevaContrasena);

                if (!resultado.Succeeded)
                {
                    var errores = resultado.Errors.Select(e => e.Description).ToList();
                    return ResultadoOperacion.FalloConErrores(
                        "Error al restablecer la contraseña.",
                        errores);
                }

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


        ///  esto es para Obtener el rol del usuario para redirigir al home correcto

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