using ArtemisBanking.Application.Common;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
    /// <summary>
    /// Controlador para manejar autenticación y gestión de cuenta
    /// Incluye login, logout, olvido de contraseña y confirmación de cuenta
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IServicioCorreo servicioCorreo,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _servicioCorreo = servicioCorreo;
            _logger = logger;
        }

        /// <summary>
        /// Muestra la pantalla de login
        /// Si el usuario ya está logueado, lo redirige a su home correspondiente
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            // Si el usuario ya está autenticado, redirigirlo a su home
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(nameof(RedirigirSegunRol));
            }

            return View();
        }

        /// <summary>
        /// Procesa el formulario de login
        /// Valida credenciales y verifica que la cuenta esté activa
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar el usuario por nombre de usuario
            var usuario = await _userManager.FindByNameAsync(model.NombreUsuario);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Los datos de acceso son incorrectos.");
                return View(model);
            }

            // Verificar si la cuenta está activa
            if (!usuario.EstaActivo)
            {
                ModelState.AddModelError(string.Empty, 
                    "Su cuenta está inactiva. Es necesario activar la cuenta mediante el enlace enviado al correo electrónico registrado.");
                return View(model);
            }

            // Intentar iniciar sesión
            var resultado = await _signInManager.PasswordSignInAsync(
                usuario, 
                model.Contrasena, 
                model.Recordarme, 
                lockoutOnFailure: true);

            if (resultado.Succeeded)
            {
                _logger.LogInformation($"Usuario {usuario.UserName} inició sesión correctamente");
                return RedirectToAction(nameof(RedirigirSegunRol));
            }

            if (resultado.IsLockedOut)
            {
                _logger.LogWarning($"Usuario {usuario.UserName} está bloqueado");
                ModelState.AddModelError(string.Empty, "Su cuenta ha sido bloqueada temporalmente debido a múltiples intentos fallidos.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Los datos de acceso son incorrectos.");
            return View(model);
        }

        /// <summary>
        /// Redirige al usuario a su home según su rol
        /// </summary>
        [Authorize]
        public IActionResult RedirigirSegunRol()
        {
            if (User.IsInRole(Constantes.RolAdministrador))
            {
                return RedirectToAction("Index", "Admin");
            }
            else if (User.IsInRole(Constantes.RolCajero))
            {
                return RedirectToAction("Index", "Cajero");
            }
            else if (User.IsInRole(Constantes.RolCliente))
            {
                return RedirectToAction("Index", "Cliente");
            }

            // Si no tiene ningún rol conocido, cerrar sesión
            _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Cierra la sesión del usuario
        /// </summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Usuario cerró sesión");
            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Muestra la pantalla para solicitar reseteo de contraseña
        /// </summary>
        [HttpGet]
        public IActionResult OlvideContrasena()
        {
            return View();
        }

        /// <summary>
        /// Procesa la solicitud de reseteo de contraseña
        /// Genera un token y lo envía por correo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvideContrasena(OlvideContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar el usuario
            var usuario = await _userManager.FindByNameAsync(model.NombreUsuario);

            if (usuario == null)
            {
                // No revelar que el usuario no existe (seguridad)
                ViewBag.Mensaje = "Si el usuario existe, recibirá un correo con instrucciones para restablecer su contraseña.";
                return View("MensajeConfirmacion");
            }

            // Desactivar el usuario mientras resetea la contraseña
            usuario.EstaActivo = false;
            await _userManager.UpdateAsync(usuario);

            // Generar token de reseteo
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            // Enviar correo con el token
            await _servicioCorreo.EnviarCorreoReseteoContrasenaAsync(
                usuario.Email, 
                usuario.UserName, 
                token);

            _logger.LogInformation($"Token de reseteo generado para usuario {usuario.UserName}");

            ViewBag.Mensaje = "Si el usuario existe, recibirá un correo con instrucciones para restablecer su contraseña.";
            return View("MensajeConfirmacion");
        }

        /// <summary>
        /// Muestra el formulario para restablecer la contraseña
        /// </summary>
        [HttpGet]
        public IActionResult RestablecerContrasena(string usuarioId, string token)
        {
            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new RestablecerContrasenaViewModel
            {
                UsuarioId = usuarioId,
                Token = token
            };

            return View(model);
        }

        /// <summary>
        /// Procesa el restablecimiento de contraseña
        /// Valida el token y actualiza la contraseña
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerContrasena(RestablecerContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Buscar el usuario
            var usuario = await _userManager.FindByIdAsync(model.UsuarioId);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                return View(model);
            }

            // Resetear la contraseña
            var resultado = await _userManager.ResetPasswordAsync(
                usuario, 
                model.Token, 
                model.Contrasena);

            if (resultado.Succeeded)
            {
                // Reactivar el usuario
                usuario.EstaActivo = true;
                await _userManager.UpdateAsync(usuario);

                _logger.LogInformation($"Contraseña restablecida para usuario {usuario.UserName}");

                TempData["MensajeExito"] = "Contraseña restablecida exitosamente. Ya puede iniciar sesión.";
                return RedirectToAction(nameof(Login));
            }

            // Agregar errores al ModelState
            foreach (var error in resultado.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        /// <summary>
        /// Confirma la cuenta de un usuario usando el token enviado por correo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConfirmarCuenta(string usuarioId, string token)
        {
            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(token))
            {
                ViewBag.Mensaje = "El enlace de confirmación no es válido.";
                return View("MensajeConfirmacion");
            }

            // Buscar el usuario
            var usuario = await _userManager.FindByIdAsync(usuarioId);

            if (usuario == null)
            {
                ViewBag.Mensaje = "Usuario no encontrado.";
                return View("MensajeConfirmacion");
            }

            // Confirmar el correo
            var resultado = await _userManager.ConfirmEmailAsync(usuario, token);

            if (resultado.Succeeded)
            {
                // Activar el usuario
                usuario.EstaActivo = true;
                await _userManager.UpdateAsync(usuario);

                _logger.LogInformation($"Cuenta confirmada para usuario {usuario.UserName}");

                ViewBag.Mensaje = "¡Tu cuenta ha sido activada exitosamente! Ya puedes iniciar sesión.";
                ViewBag.MostrarBotonLogin = true;
            }
            else
            {
                ViewBag.Mensaje = "No se pudo confirmar la cuenta. El token puede ser inválido o estar vencido.";
            }

            return View("MensajeConfirmacion");
        }

        /// <summary>
        /// Muestra la pantalla de acceso denegado
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}