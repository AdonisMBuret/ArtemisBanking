using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.ViewModels.PortalComercio;
using ArtemisBanking.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{

    /// Controlador para el portal del comercio


    [Authorize(Policy = "SoloComercio")]
    public class PortalComercioController : Controller
    {
        private readonly IServicioConsumoTarjeta _servicioConsumo;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumo;
        private readonly IRepositorioComercio _repositorioComercio;
        private readonly ILogger<PortalComercioController> _logger;

        public PortalComercioController(
            IServicioConsumoTarjeta servicioConsumo,
            IRepositorioConsumoTarjeta repositorioConsumo,
            IRepositorioComercio repositorioComercio,
            ILogger<PortalComercioController> logger)
        {
            _servicioConsumo = servicioConsumo;
            _repositorioConsumo = repositorioConsumo;
            _repositorioComercio = repositorioComercio;
            _logger = logger;
        }

        private string ObtenerUsuarioActualId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

    
        /// Dashboard del comercio con estadísticas y consumos recientes
    
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();
                var comercio = await _repositorioComercio.ObtenerPorUsuarioIdAsync(usuarioId);

                if (comercio == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el comercio asociado a este usuario";
                    return View(new DashboardComercioViewModel());
                }

                var consumosHoy = await _repositorioConsumo.ObtenerConsumosPorComercioYFechaAsync(
                    comercio.Id, 
                    DateTime.Today, 
                    DateTime.Today.AddDays(1).AddSeconds(-1));

                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddSeconds(-1);
                var consumosMes = await _repositorioConsumo.ObtenerConsumosPorComercioYFechaAsync(
                    comercio.Id, 
                    inicioMes, 
                    finMes);

                var consumosRecientes = await _repositorioConsumo.ObtenerConsumosRecientesDeComercioAsync(comercio.Id, 10);

                var viewModel = new DashboardComercioViewModel
                {
                    NombreComercio = comercio.Nombre,
                    RNC = comercio.RNC,
                    TotalConsumosHoy = consumosHoy.Count(),
                    MontoTotalHoy = consumosHoy.Sum(c => c.Monto),
                    TotalConsumosMes = consumosMes.Count(),
                    MontoTotalMes = consumosMes.Sum(c => c.Monto),
                    ConsumosRecientes = consumosRecientes.Select(c => new ConsumoRecienteViewModel
                    {
                        FechaConsumo = c.FechaConsumo,
                        Monto = c.Monto,
                        UltimosCuatroDigitos = c.Tarjeta.UltimosCuatroDigitos,
                        EstadoConsumo = c.EstadoConsumo
                    })
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dashboard del comercio");
                TempData["ErrorMessage"] = "Error al cargar el dashboard";
                return View(new DashboardComercioViewModel());
            }
        }
    
        /// Muestra el formulario para registrar un consumo
    
        [HttpGet]
        public IActionResult RegistrarConsumo()
        {
            return View(new RegistrarConsumoViewModel());
        }

    
        /// Procesa el registro del consumo
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarConsumo(RegistrarConsumoViewModel model)
        {
            // ? LOGGING PARA DEBUG
            _logger.LogInformation($"=== REGISTRANDO CONSUMO ===");
            _logger.LogInformation($"NumeroTarjeta: {model.NumeroTarjeta}");
            _logger.LogInformation($"Monto: {model.Monto}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { 
                        Campo = x.Key, 
                        Errores = string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))
                    });
                
                foreach (var error in errores)
                {
                    _logger.LogError($"Error en {error.Campo}: {error.Errores}");
                }
                
                TempData["ErrorMessage"] = $"Error de validación: {string.Join("; ", errores.Select(e => $"{e.Campo}: {e.Errores}"))}";
                return View(model);
            }

            try
            {
                var infoTarjeta = await _servicioConsumo.ObtenerInfoTarjetaAsync(model.NumeroTarjeta);

                if (!infoTarjeta.Exito)
                {
                    _logger.LogWarning($"Tarjeta no encontrada: {infoTarjeta.Mensaje}");
                    TempData["ErrorMessage"] = infoTarjeta.Mensaje;
                    return View(model);
                }

                var confirmacionVM = new ConfirmarConsumoViewModel
                {
                    NumeroTarjeta = model.NumeroTarjeta,
                    UltimosCuatroDigitos = infoTarjeta.Datos.ultimosCuatroDigitos,
                    NombreCliente = infoTarjeta.Datos.nombreCliente,
                    LimiteCredito = infoTarjeta.Datos.limiteCredito,
                    DeudaActual = infoTarjeta.Datos.deudaActual,
                    CreditoDisponible = infoTarjeta.Datos.creditoDisponible,
                    Monto = model.Monto
                };

                return View("ConfirmarConsumo", confirmacionVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar consumo");
                TempData["ErrorMessage"] = "Error al procesar el consumo";
                return View(model);
            }
        }
            
        /// Confirma y procesa el consumo
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarConsumo(ConfirmarConsumoViewModel model)
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();
                var comercio = await _repositorioComercio.ObtenerPorUsuarioIdAsync(usuarioId);

                if (comercio == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el comercio asociado";
                    return RedirectToAction(nameof(RegistrarConsumo));
                }

                var dto = new RegistrarConsumoDTO
                {
                    NumeroTarjeta = model.NumeroTarjeta,
                    ComercioId = comercio.Id,
                    Monto = model.Monto
                };

                var resultado = await _servicioConsumo.RegistrarConsumoAsync(dto);

                if (resultado.Exito)
                {
                    _logger.LogInformation($"Consumo registrado exitosamente");
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogWarning($"Error al registrar consumo: {resultado.Mensaje}");
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(RegistrarConsumo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar consumo");
                TempData["ErrorMessage"] = "Error al confirmar el consumo";
                return RedirectToAction(nameof(RegistrarConsumo));
            }
        }

    
        /// Lista paginada de consumos del comercio
    
        [HttpGet]
        public async Task<IActionResult> Historial(int pagina = 1, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();
                var comercio = await _repositorioComercio.ObtenerPorUsuarioIdAsync(usuarioId);

                if (comercio == null)
                {
                    TempData["ErrorMessage"] = "No se encontró el comercio asociado";
                    return View(new ListaConsumosViewModel());
                }

                if (!fechaInicio.HasValue)
                {
                    fechaInicio = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                }
                if (!fechaFin.HasValue)
                {
                    fechaFin = fechaInicio.Value.AddMonths(1).AddSeconds(-1);
                }

                var (consumos, total) = await _repositorioConsumo.ObtenerConsumosPaginadosDeComercioAsync(
                    comercio.Id,
                    pagina,
                    Constantes.TamanoPaginaPorDefecto,
                    fechaInicio.Value,
                    fechaFin.Value);

                var viewModel = new ListaConsumosViewModel
                {
                    Consumos = consumos.Select(c => new ConsumoItemViewModel
                    {
                        Id = c.Id,
                        FechaConsumo = c.FechaConsumo,
                        Monto = c.Monto,
                        UltimosCuatroDigitosTarjeta = c.Tarjeta.UltimosCuatroDigitos,
                        NombreCliente = $"{c.Tarjeta.Cliente.Nombre} {c.Tarjeta.Cliente.Apellido}",
                        EstadoConsumo = c.EstadoConsumo
                    }),
                    PaginaActual = pagina,
                    TotalPaginas = (int)Math.Ceiling((double)total / Constantes.TamanoPaginaPorDefecto),
                    TotalRegistros = total,
                    FiltroFechaInicio = fechaInicio,
                    FiltroFechaFin = fechaFin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar historial de consumos");
                TempData["ErrorMessage"] = "Error al cargar el historial";
                return View(new ListaConsumosViewModel());
            }
        }
    }
}
