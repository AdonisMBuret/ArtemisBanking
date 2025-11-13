using ArtemisBanking.Application.Common;
using ArtemisBanking.Models;
using ArtemisBanking.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ArtemisBanking.Web.Controllers
{
    /// <summary>
    /// Controlador principal que maneja:
    /// - Redirección al home correcto según el rol
    /// - Página de errores
    /// - Página principal del sitio (si no está logueado)
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Página principal - redirige según el estado de autenticación
        /// Si está logueado: redirige a su home según rol
        /// Si no está logueado: redirige al login
        /// </summary>
        public IActionResult Index()
        {
            // Si el usuario está autenticado, redirigir según su rol
            if (User.Identity?.IsAuthenticated == true)
            {
                // Obtener el rol del usuario
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
            }

            // Si no está autenticado, redirigir al login
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Página de privacidad (opcional)
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Página de error genérica
        /// Se muestra cuando hay algún error no controlado en la aplicación
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            _logger.LogError($"Error en la aplicación. Request ID: {requestId}");

            return View(new ErrorViewModel
            {
                RequestId = requestId
            });
        }

        /// <summary>
        /// Página de error 404 - Página no encontrada
        /// </summary>
        [Route("/Error/404")]
        public IActionResult Error404()
        {
            _logger.LogWarning("Página no encontrada (404)");
            return View();
        }

        /// <summary>
        /// Página de error 500 - Error del servidor
        /// </summary>
        [Route("/Error/500")]
        public IActionResult Error500()
        {
            _logger.LogError("Error interno del servidor (500)");
            return View();
        }
    }
}