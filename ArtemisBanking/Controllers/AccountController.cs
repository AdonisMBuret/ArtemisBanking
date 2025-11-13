using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
    /// <summary>
    /// Controlador de autenticación - ARQUITECTURA LIMPIA
    /// Este controlador SOLO:
    /// 1. Recibe datos del usuario
    /// 2. Llama al servicio correspondiente
    /// 3. Retorna la vista con el resultado
    /// NO contiene lógica de negocio
    /// </summary>
    public class AccountController : Controller
    {
        // Solo depende de la ABSTRACCIÓN (interfaz), no de Identity
        private readonly IServicioAutenticacion _servicioAuth;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IServicioAutenticacion servicioAuth,
            ILogger<AccountController> logger)
        {
            _servicioAuth = servicioAuth;
            _logger = logger;
        }

        // ==================== LOGIN ====================

        /// <summary>
        /// Muestra la pantalla de login
        /// Si ya está autenticado, redirige a su home
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir según su rol
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleHome();
            }

            return View();
        }

        /// <summary>
        /// Procesa el login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Validar el modelo
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Llamar al servicio (TODA la lógica está allí)
            var resultado = await _servicioAuth.LoginAsync(
                model.NombreUsuario,
                model.Contrasena,
                model.Recordarme);

            // Si falló, mostrar error
            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(model);
            }

            // Si fue exitoso, redirigir según el rol retornado
            _logger.LogInformation($"Login exitoso para {model.NombreUsuario}");

            return RedirectToRoleHomeByRole(resultado.Datos);
        }

        // ==================== LOGOUT ====================

        /// <summary>
        /// Cierra la sesión
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _servicioAuth.LogoutAsync();
            return RedirectToAction(nameof(Login));
        }

        // ==================== CONFIRMACIÓN DE CUENTA ====================

        /// <summary>
        /// Muestra el formulario de confirmación
        /// </summary>
        [HttpGet]
        public IActionResult ConfirmarCuenta(string usuarioId, string token)
        {
            if (string.IsNullOrEmpty(usuarioId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction(nameof(Login));
            }

            var model = new ConfirmarCuentaViewModel
            {
                UsuarioId = usuarioId,
                Token = token
            };

            return View(model);
        }

        /// <summary>
        /// Procesa la confirmación de cuenta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCuenta(ConfirmarCuentaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Llamar al servicio
            var resultado = await _servicioAuth.ConfirmarCuentaAsync(
                model.UsuarioId,
                model.Token);

            if (!resultado.Exito)
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(model);
            }

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Login));
        }

        // ==================== OLVIDÉ MI CONTRASEÑA ====================

        /// <summary>
        /// Muestra el formulario para solicitar reseteo
        /// </summary>
        [HttpGet]
        public IActionResult OlvideContrasena()
        {
            return View();
        }

        /// <summary>
        /// Procesa la solicitud de reseteo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvideContrasena(OlvideContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Llamar al servicio
            var resultado = await _servicioAuth.SolicitarReseteoContrasenaAsync(model.NombreUsuario);

            // Siempre mostrar el mismo mensaje (por seguridad)
            TempData["Mensaje"] = resultado.Mensaje;
            return RedirectToAction(nameof(Login));
        }

        // ==================== RESTABLECER CONTRASEÑA ====================

        /// <summary>
        /// Muestra el formulario de reseteo
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
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerContrasena(RestablecerContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Llamar al servicio
            var resultado = await _servicioAuth.RestablecerContrasenaAsync(
                model.UsuarioId,
                model.Token,
                model.Contrasena);

            if (!resultado.Exito)
            {
                // Agregar errores al modelo
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

            TempData["Exito"] = resultado.Mensaje;
            return RedirectToAction(nameof(Login));
        }

        // ==================== ACCESO DENEGADO ====================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }


        // ==================== MÉTODOS AUXILIARES ====================

        /// <summary>
        /// Redirige al home según el rol del usuario autenticado
        /// </summary>
        private IActionResult RedirectToRoleHome()
        {
            if (User.IsInRole("Administrador"))
                return RedirectToAction("Index", "Admin");

            if (User.IsInRole("Cajero"))
                return RedirectToAction("Index", "Cajero");

            if (User.IsInRole("Cliente"))
                return RedirectToAction("Index", "Cliente");

            return RedirectToAction(nameof(Login));
        }

        /// <summary>
        /// Redirige al home según el rol especificado
        /// </summary>
        private IActionResult RedirectToRoleHomeByRole(string rol)
        {
            return rol switch
            {
                "Administrador" => RedirectToAction("Index", "Admin"),
                "Cajero" => RedirectToAction("Index", "Cajero"),
                "Cliente" => RedirectToAction("Index", "Cliente"),
                _ => RedirectToAction(nameof(Login))
            };
        }
    }
}