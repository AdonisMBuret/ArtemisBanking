using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.ViewModels.Comercio;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
    /// <summary>
    /// Controlador para gestión de comercios
    /// Solo accesible por Administradores
    /// </summary>
    [Authorize(Policy = "SoloAdministrador")]
    public class ComercioController : Controller
    {
        private readonly IServicioComercio _servicioComercio;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumoTarjeta;
        private readonly IMapper _mapper;
        private readonly ILogger<ComercioController> _logger;

        public ComercioController(
            IServicioComercio servicioComercio,
            IRepositorioConsumoTarjeta repositorioConsumoTarjeta,
            IMapper mapper,
            ILogger<ComercioController> logger)
        {
            _servicioComercio = servicioComercio;
            _repositorioConsumoTarjeta = repositorioConsumoTarjeta;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: Comercio/Index
        [HttpGet]
        public async Task<IActionResult> Index(int pagina = 1)
        {
            try
            {
                var resultado = await _servicioComercio.ObtenerComerciosPaginadosAsync(pagina, 20);
                
                var viewModel = new ListaComerciosViewModel
                {
                    Comercios = _mapper.Map<IEnumerable<ComercioItemViewModel>>(resultado.Data),
                    PaginaActual = resultado.Page,
                    TotalPaginas = resultado.TotalPages,
                    TotalRegistros = resultado.TotalRecords
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener comercios");
                TempData["ErrorMessage"] = "Error al cargar los comercios";
                return View(new ListaComerciosViewModel());
            }
        }

        // GET: Comercio/Detalle/5
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var comercio = await _servicioComercio.ObtenerComercioPorIdAsync(id);
                
                if (comercio == null)
                {
                    TempData["ErrorMessage"] = "Comercio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener estadísticas de consumos
                var consumos = await _repositorioConsumoTarjeta.ObtenerConsumosPorComercioAsync(id);

                var viewModel = _mapper.Map<DetalleComercioViewModel>(comercio);
                viewModel.TotalConsumos = consumos.Count();
                viewModel.MontoTotalConsumos = consumos
                    .Where(c => c.EstadoConsumo == "APROBADO")
                    .Sum(c => c.Monto);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener comercio {id}");
                TempData["ErrorMessage"] = "Error al cargar el comercio";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Comercio/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new CrearComercioViewModel());
        }

        // POST: Comercio/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearComercioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = _mapper.Map<CrearComercioRequestDTO>(model);
                var comercio = await _servicioComercio.CrearComercioAsync(dto);

                TempData["SuccessMessage"] = "Comercio creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comercio");
                ModelState.AddModelError(string.Empty, "Error al crear el comercio");
                return View(model);
            }
        }

        // GET: Comercio/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var comercio = await _servicioComercio.ObtenerComercioPorIdAsync(id);
                
                if (comercio == null)
                {
                    TempData["ErrorMessage"] = "Comercio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<EditarComercioViewModel>(comercio);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar comercio {id}");
                TempData["ErrorMessage"] = "Error al cargar el comercio";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Comercio/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EditarComercioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = _mapper.Map<ActualizarComercioRequestDTO>(model);
                var resultado = await _servicioComercio.ActualizarComercioAsync(model.Id, dto);

                if (resultado)
                {
                    TempData["SuccessMessage"] = "Comercio actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "No se pudo actualizar el comercio");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar comercio {model.Id}");
                ModelState.AddModelError(string.Empty, "Error al actualizar el comercio");
                return View(model);
            }
        }

        // POST: Comercio/CambiarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            try
            {
                var comercio = await _servicioComercio.ObtenerComercioPorIdAsync(id);
                
                if (comercio == null)
                {
                    TempData["ErrorMessage"] = "Comercio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                var nuevoEstado = !comercio.EstaActivo;
                var resultado = await _servicioComercio.CambiarEstadoComercioAsync(id, nuevoEstado);

                if (resultado)
                {
                    var mensaje = nuevoEstado ? "activado" : "desactivado";
                    TempData["SuccessMessage"] = $"Comercio {mensaje} exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se pudo cambiar el estado del comercio";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cambiar estado del comercio {id}");
                TempData["ErrorMessage"] = "Error al cambiar el estado";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
