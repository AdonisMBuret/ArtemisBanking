using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
         
    public class AccountController : Controller
    {
        private readonly IServicioAutenticacion _servicioAutenticacion;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IServicioAutenticacion servicioAutenticacion,
            ILogger<AccountController> logger)
        {
            _servicioAutenticacion = servicioAutenticacion;
            _logger = logger;
        }

        //  LOGIN 
         
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
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

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _servicioAutenticacion.LoginAsync(
                model.NombreUsuario,
                model.Contrasena,
                model.Recordarme);

            if (resultado.Exito)
            {
                _logger.LogInformation($"Usuario {model.NombreUsuario} inició sesión correctamente");

                var rol = resultado.Datos; 

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return rol switch
                {
                    Constantes.RolAdministrador => RedirectToAction("Index", "Admin"),
                    Constantes.RolCajero => RedirectToAction("Index", "Cajero"),
                    Constantes.RolCliente => RedirectToAction("Index", "Cliente"),
                    Constantes.RolComercio => RedirectToAction("Index", "PortalComercio"),
                    _ => RedirectToAction("Index", "Home")
                };
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        //  LOGOUT 
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize] 
        public async Task<IActionResult> Logout()
        {
            await _servicioAutenticacion.LogoutAsync();
            _logger.LogInformation("Usuario cerró sesión");

            return RedirectToAction(nameof(Login));
        }

        //  CONFIRMAR CUENTA 
         
        [HttpGet]
        public IActionResult ConfirmarCuenta(string userId = null, string token = null)
        {
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

            var resultado = await _servicioAutenticacion.ConfirmarCuentaAsync(
                model.UsuarioId,
                model.Token);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Login));
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        //  RESETEO DE CONTRASEÑA 
       
        [HttpGet]
        public IActionResult OlvideContrasena()
        {
            return View();
        }

         
        /// Procesa la solicitud de reseteo de contraseña
  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvideContrasena(OlvideContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _servicioAutenticacion.SolicitarReseteoContrasenaAsync(
                model.NombreUsuario);

            TempData["InfoMessage"] = resultado.Mensaje;
            return RedirectToAction(nameof(Login));
        }

        /// Muestra el formulario para restablecer contraseña con token
         
        [HttpGet]
        public IActionResult RestablecerContrasena(string userId = null, string token = null)
        {
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
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerContrasena(RestablecerContrasenaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = await _servicioAutenticacion.RestablecerContrasenaAsync(
                model.UsuarioId,
                model.Token,
                model.Contrasena);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Login));
            }

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

        //  ACCESO DENEGADO 

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}