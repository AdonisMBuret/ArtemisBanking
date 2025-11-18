using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.DTOs.Api;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.ViewModels.Comercio;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly UserManager<Usuario> _userManager;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly IMapper _mapper;
        private readonly ILogger<ComercioController> _logger;
        private readonly IConfiguration _configuration;

        public ComercioController(
            IServicioComercio servicioComercio,
            IRepositorioConsumoTarjeta repositorioConsumoTarjeta,
            IRepositorioCuentaAhorro repositorioCuenta,
            UserManager<Usuario> userManager,
            IServicioCorreo servicioCorreo,
            IMapper mapper,
            ILogger<ComercioController> logger,
            IConfiguration configuration)
        {
            _servicioComercio = servicioComercio;
            _repositorioConsumoTarjeta = repositorioConsumoTarjeta;
            _repositorioCuenta = repositorioCuenta;
            _userManager = userManager;
            _servicioCorreo = servicioCorreo;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
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
            // ? LOGGING PARA DEBUG
            _logger.LogInformation($"=== CREANDO COMERCIO ===");
            _logger.LogInformation($"Nombre: {model.Nombre}");
            _logger.LogInformation($"RNC: {model.RNC}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                // ? MOSTRAR ERRORES DE VALIDACIÓN
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
                var dto = _mapper.Map<CrearComercioRequestDTO>(model);
                _logger.LogInformation($"DTO mapeado - Nombre: {dto.Nombre}, RNC: {dto.RNC}");
                
                var comercio = await _servicioComercio.CrearComercioAsync(dto);
                
                _logger.LogInformation($"Comercio creado exitosamente - ID: {comercio?.Id}");
                TempData["SuccessMessage"] = "Comercio creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear comercio");
                TempData["ErrorMessage"] = $"Error al crear el comercio: {ex.Message}";
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

        // ==================== ASIGNAR USUARIO A COMERCIO ====================

        /// <summary>
        /// Muestra el formulario para crear un usuario y asociarlo a un comercio
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AsignarUsuario(int id)
        {
            try
            {
                var comercio = await _servicioComercio.ObtenerComercioPorIdAsync(id);
                
                if (comercio == null)
                {
                    TempData["ErrorMessage"] = "Comercio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar si ya tiene usuario asignado
                if (await _servicioComercio.TieneUsuarioAsociadoAsync(id))
                {
                    TempData["ErrorMessage"] = "Este comercio ya tiene un usuario asignado";
                    return RedirectToAction(nameof(Detalle), new { id });
                }

                var viewModel = new AsignarUsuarioComercioViewModel
                {
                    ComercioId = comercio.Id,
                    NombreComercio = comercio.Nombre,
                    RNC = comercio.RNC
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar formulario de asignación {id}");
                TempData["ErrorMessage"] = "Error al cargar el formulario";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Procesa la creación del usuario y lo asocia al comercio
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarUsuario(AsignarUsuarioComercioViewModel model)
        {
            // ? LOGGING PARA DEBUG
            _logger.LogInformation($"=== ASIGNANDO USUARIO A COMERCIO ===");
            _logger.LogInformation($"ComercioId: {model.ComercioId}");
            _logger.LogInformation($"Usuario: {model.NombreUsuario}");
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
                _logger.LogInformation("Verificando que el comercio existe...");
                var comercio = await _servicioComercio.ObtenerComercioPorIdAsync(model.ComercioId);
                
                if (comercio == null)
                {
                    _logger.LogWarning($"Comercio {model.ComercioId} no encontrado");
                    TempData["ErrorMessage"] = "Comercio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Verificando que no tenga usuario asignado...");
                if (await _servicioComercio.TieneUsuarioAsociadoAsync(model.ComercioId))
                {
                    _logger.LogWarning($"Comercio {model.ComercioId} ya tiene usuario");
                    TempData["ErrorMessage"] = "Este comercio ya tiene un usuario asignado";
                    return RedirectToAction(nameof(Detalle), new { id = model.ComercioId });
                }

                _logger.LogInformation("Creando usuario de Identity...");
                // 1. Crear el usuario
                var nuevoUsuario = new Usuario
                {
                    UserName = model.NombreUsuario,
                    Email = model.Correo,
                    Nombre = model.Nombre,
                    Apellido = model.Apellido,
                    Cedula = model.Cedula,
                    EstaActivo = false, // Inactivo hasta que confirme el email
                    EmailConfirmed = false,
                    ComercioId = model.ComercioId, // ? Asociar al comercio
                    FechaCreacion = DateTime.Now
                };

                _logger.LogInformation($"Intentando crear usuario {model.NombreUsuario}...");
                var resultado = await _userManager.CreateAsync(nuevoUsuario, model.Contrasena);

                if (!resultado.Succeeded)
                {
                    var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                    _logger.LogError($"Error al crear usuario: {errores}");
                    TempData["ErrorMessage"] = $"Error al crear usuario: {errores}";
                    return View(model);
                }

                _logger.LogInformation("Usuario creado, asignando rol Comercio...");
                // 2. Asignar rol Comercio
                await _userManager.AddToRoleAsync(nuevoUsuario, "Comercio");

                _logger.LogInformation("Generando token de confirmación...");
                // 3. Generar token de confirmación
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(nuevoUsuario);

                _logger.LogInformation("Enviando correo de confirmación...");
                // 4. Enviar correo de confirmación
                var urlBase = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7103";
                await _servicioCorreo.EnviarCorreoConfirmacionAsync(
                    nuevoUsuario.Email,
                    nuevoUsuario.NombreCompleto,
                    nuevoUsuario.Id,
                    token,
                    urlBase);

                _logger.LogInformation("Creando cuenta de ahorro principal...");
                // 5. Crear cuenta de ahorro principal para el comercio
                var numeroCuenta = await GenerarNumeroCuentaUnicoAsync();
                var cuentaPrincipal = new CuentaAhorro
                {
                    NumeroCuenta = numeroCuenta,
                    Balance = 0,
                    EsPrincipal = true,
                    EstaActiva = true,
                    UsuarioId = nuevoUsuario.Id,
                    FechaCreacion = DateTime.Now
                };

                await _repositorioCuenta.AgregarAsync(cuentaPrincipal);
                await _repositorioCuenta.GuardarCambiosAsync();

                _logger.LogInformation($"Usuario {model.NombreUsuario} creado y asociado exitosamente");
                TempData["SuccessMessage"] = "Usuario creado y asociado al comercio. Se envió un correo de confirmación.";
                return RedirectToAction(nameof(Detalle), new { id = model.ComercioId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar usuario");
                TempData["ErrorMessage"] = $"Error al crear el usuario: {ex.Message}";
                return View(model);
            }
        }

        /// <summary>
        /// Genera un número de cuenta único de 9 dígitos
        /// </summary>
        private async Task<string> GenerarNumeroCuentaUnicoAsync()
        {
            var random = new Random();
            string numeroCuenta;

            do
            {
                numeroCuenta = "";
                for (int i = 0; i < 9; i++)
                {
                    numeroCuenta += random.Next(0, 10).ToString();
                }
            }
            while (await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(numeroCuenta) != null);

            return numeroCuenta;
        }
    }
}
