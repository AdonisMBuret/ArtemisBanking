using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.ViewModels.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ArtemisBanking.Web.Controllers
{
     //hice push
    /// Controlador principal que maneja:
    /// - Redirección al home correcto según el rol
    /// - Página de errores
    /// - Página principal del sitio (si no está logueado)
     
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

         
        /// Página principal - redirige según el estado de autenticación
        /// Si está logueado: redirige a su home según rol
        /// Si no está logueado: redirige al login
         
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

         
        /// Página de privacidad (opcional)
         
        public IActionResult Privacy()
        {
            return View();
        }

         
        /// Página de error genérica
        /// Se muestra cuando hay algún error no controlado en la aplicación
         
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

         
        /// Página de error 404 - Página no encontrada
         
        [Route("/Error/404")]
        public IActionResult Error404()
        {
            _logger.LogWarning("Página no encontrada (404)");
            return View();
        }

         
        /// Página de error 500 - Error del servidor
         
        [Route("/Error/500")]
        public IActionResult Error500()
        {
            _logger.LogError("Error interno del servidor (500)");
            return View();
        }
    }
}