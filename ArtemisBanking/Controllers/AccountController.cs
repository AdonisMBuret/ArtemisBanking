using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
     
    /// Controlador para manejar todo lo relacionado con autenticación
    /// Solo recibe datos, llama al servicio y retorna vistas
     
    public class AccountController : Controller
    {
        // Servicio que tiene TODA la lógica de autenticación
        private readonly IServicioAutenticacion _servicioAutenticacion;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IServicioAutenticacion servicioAutenticacion,
            ILogger<AccountController> logger)
        {
            _servicioAutenticacion = servicioAutenticacion;
            _logger = logger;
        }

        // ==================== LOGIN ====================

         
        /// Muestra el formulario de login
        /// Si el usuario ya está logueado, lo redirige a su home
         
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Si el usuario ya está autenticado, redirigir a su home
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

         
        /// Procesa el formulario de login
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Validar que el modelo sea válido
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Llamar al servicio de autenticación (aquí está TODA la lógica)
            var resultado = await _servicioAutenticacion.LoginAsync(
                model.NombreUsuario,
                model.Contrasena,
                model.Recordarme);

            // Si el login fue exitoso
            if (resultado.Exito)
            {
                _logger.LogInformation($"Usuario {model.NombreUsuario} inició sesión correctamente");

                // Redirigir según el rol del usuario
                var rol = resultado.Datos; // El servicio retorna el rol

                // Si hay una URL de retorno, ir ahí
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Si no, redirigir al home correspondiente según el rol
                return rol switch
                {
                    Constantes.RolAdministrador => RedirectToAction("Index", "Admin"),
                    Constantes.RolCajero => RedirectToAction("Index", "Cajero"),
                    Constantes.RolCliente => RedirectToAction("Index", "Cliente"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            // Si hubo error, mostrar el mensaje
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        // ==================== LOGOUT ====================

         
        /// Cierra la sesión del usuario y lo redirige al login
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] // Solo usuarios autenticados pueden hacer logout
        public async Task<IActionResult> Logout()
        {
            await _servicioAutenticacion.LogoutAsync();
            _logger.LogInformation("Usuario cerró sesión");

            return RedirectToAction(nameof(Login));
        }

        // ==================== CONFIRMAR CUENTA ====================

         
        /// Muestra el formulario para confirmar cuenta con token
         
        [HttpGet]
        public IActionResult ConfirmarCuenta(string userId = null, string token = null)
        {
            // Si vienen los datos en la URL, pre-llenar el formulario
            var model = new ConfirmarCuentaViewModel
            {
                UsuarioId = userId ?? string.Empty,
                Token = token ?? string.Empty
            };

            return View(model);
        }

         
        /// Procesa la confirmación de cuenta
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCuenta(ConfirmarCuentaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Llamar al servicio para confirmar la cuenta
            var resultado = await _servicioAutenticacion.ConfirmarCuentaAsync(
                model.UsuarioId,
                model.Token);

            if (resultado.Exito)
            {
                // Mostrar mensaje de éxito y redirigir al login
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Login));
            }

            // Si hubo error, mostrar el mensaje
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        // ==================== RESETEO DE CONTRASEÑA ====================

         
        /// Muestra el formulario para solicitar reseteo de contraseña
         
        [HttpGet]
        public IActionResult OlvideContrasena()
        {
            return View();
        }

         
        /// Procesa la solicitud de reseteo de contraseña
        /// Desactiva al usuario y envía correo con token
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvideContrasena(OlvideContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Llamar al servicio (desactiva usuario y envía correo)
            var resultado = await _servicioAutenticacion.SolicitarReseteoContrasenaAsync(
                model.NombreUsuario);

            // Siempre mostrar el mismo mensaje (por seguridad, no revelar si el usuario existe)
            TempData["InfoMessage"] = resultado.Mensaje;
            return RedirectToAction(nameof(Login));
        }

         
        /// Muestra el formulario para restablecer contraseña con token
         
        [HttpGet]
        public IActionResult RestablecerContrasena(string userId = null, string token = null)
        {
            // Si no vienen los parámetros, redirigir al login
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new RestablecerContrasenaViewModel
            {
                UsuarioId = userId,
                Token = token
            };

            return View(model);
        }

         
        /// Procesa el restablecimiento de contraseña
        /// Cambia la contraseña y reactiva al usuario
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerContrasena(RestablecerContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Llamar al servicio para cambiar la contraseña
            var resultado = await _servicioAutenticacion.RestablecerContrasenaAsync(
                model.UsuarioId,
                model.Token,
                model.Contrasena);

            if (resultado.Exito)
            {
                // Mostrar mensaje de éxito y redirigir al login
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Login));
            }

            // Si hubo errores, mostrarlos
            if (resultado.Errores.Any())
            {
                foreach (var error in resultado.Errores)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
            }

            return View(model);
        }

        // ==================== ACCESO DENEGADO ====================

         
        /// Página que se muestra cuando un usuario intenta acceder a una sección no autorizada
         
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}