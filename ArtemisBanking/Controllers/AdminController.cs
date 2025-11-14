using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Interfaces.Repositories;
using ArtemisBanking.ViewModels.CuentaAhorro;
using ArtemisBanking.ViewModels.Prestamo;
using ArtemisBanking.ViewModels.TarjetaCredito;
using ArtemisBanking.ViewModels.Usuario;
using ArtemisBanking.Web.ViewModels;
using ArtemisBanking.Web.ViewModels.CuentaAhorro;
using ArtemisBanking.Web.ViewModels.TarjetaCredito;
using ArtemisBanking.Web.ViewModels.Usuario;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
    /// <summary>
    /// Controlador para todas las funcionalidades del Administrador
    /// Solo accesible para usuarios con rol "Administrador"
    /// </summary>
    [Authorize(Policy = "SoloAdministrador")]
    public class AdminController : Controller
    {
        private readonly IServicioUsuario _servicioUsuario;
        private readonly IServicioPrestamo _servicioPrestamo;
        private readonly IServicioTarjetaCredito _servicioTarjeta;
        private readonly IServicioCuentaAhorro _servicioCuenta;

        // ⭐ AGREGAR ESTOS REPOSITORIOS:
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioCuotaPrestamo _repositorioCuotaPrestamo;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumoTarjeta;
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;

        private readonly IMapper _mapper;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IServicioUsuario servicioUsuario,
            IServicioPrestamo servicioPrestamo,
            IServicioTarjetaCredito servicioTarjeta,
            IServicioCuentaAhorro servicioCuenta,

            // ⭐ AGREGAR ESTOS EN EL CONSTRUCTOR:
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioCuotaPrestamo repositorioCuotaPrestamo,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioConsumoTarjeta repositorioConsumoTarjeta,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioTransaccion repositorioTransaccion,

            IMapper mapper,
            ILogger<AdminController> logger)
        {
            _servicioUsuario = servicioUsuario;
            _servicioPrestamo = servicioPrestamo;
            _servicioTarjeta = servicioTarjeta;
            _servicioCuenta = servicioCuenta;

            // ⭐ ASIGNAR LOS REPOSITORIOS:
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioCuotaPrestamo = repositorioCuotaPrestamo;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioConsumoTarjeta = repositorioConsumoTarjeta;
            _repositorioCuenta = repositorioCuenta;
            _repositorioTransaccion = repositorioTransaccion;

            _mapper = mapper;
            _logger = logger;
        }

        // ==================== DASHBOARD (HOME) ====================

        /// <summary>
        /// Página principal del administrador con todos los indicadores
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Llamar al servicio para obtener todos los indicadores
            var resultado = await _servicioUsuario.ObtenerDashboardAdminAsync();

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return View(new DashboardAdminViewModel());
            }

            // Mapear el DTO al ViewModel
            var viewModel = _mapper.Map<DashboardAdminViewModel>(resultado.Datos);

            return View(viewModel);
        }

        // ==================== GESTIÓN DE USUARIOS ====================

        /// <summary>
        /// Lista paginada de usuarios con filtros
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Usuarios(int pagina = 1, string filtroRol = null)
        {
            // Obtener el ID del usuario actual (para no permitirle editarse a sí mismo)
            var usuarioActualId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Llamar al servicio para obtener usuarios paginados
            var resultado = await _servicioUsuario.ObtenerUsuariosPaginadosAsync(
                pagina,
                Constantes.TamanoPaginaPorDefecto,
                filtroRol,
                usuarioActualId);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return View(new ListaUsuariosViewModel());
            }

            // Mapear a ViewModel
            var viewModel = new ListaUsuariosViewModel
            {
                Usuarios = resultado.Datos.Datos.Select(u => new UsuarioListaItemViewModel
                {
                    Id = u.Id,
                    NombreUsuario = u.NombreUsuario,
                    Cedula = u.Cedula,
                    NombreCompleto = u.NombreCompleto,
                    Correo = u.Correo,
                    TipoUsuario = u.Rol,
                    EstaActivo = u.EstaActivo,
                    PuedeEditar = u.MontoInicial != 0 // Reutilizamos este campo
                }),
                PaginaActual = resultado.Datos.PaginaActual,
                TotalPaginas = resultado.Datos.TotalPaginas,
                TotalRegistros = resultado.Datos.TotalRegistros,
                FiltroRol = filtroRol
            };

            return View(viewModel);
        }

        /// <summary>
        /// Muestra el formulario para crear un nuevo usuario
        /// </summary>
        [HttpGet]
        public IActionResult CrearUsuario()
        {
            return View();
        }

        /// <summary>
        /// Procesa la creación de un nuevo usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearUsuario(CrearUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Mapear ViewModel a DTO
            var dto = _mapper.Map<CrearUsuarioDTO>(model);

            // Llamar al servicio
            var resultado = await _servicioUsuario.CrearUsuarioAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Usuarios));
            }

            // Si hubo error, mostrar mensaje
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        /// <summary>
        /// Muestra el formulario para editar un usuario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditarUsuario(string id)
        {
            // Validar que no sea el usuario actual
            var usuarioActualId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (id == usuarioActualId)
            {
                TempData["ErrorMessage"] = "No puede editar su propia cuenta";
                return RedirectToAction(nameof(Usuarios));
            }

            // Obtener el usuario
            var resultado = await _servicioUsuario.ObtenerUsuarioPorIdAsync(id);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Usuarios));
            }

            // Mapear a ViewModel
            var viewModel = new EditarUsuarioViewModel
            {
                Id = resultado.Datos.Id,
                Nombre = resultado.Datos.Nombre,
                Apellido = resultado.Datos.Apellido,
                Cedula = resultado.Datos.Cedula,
                Correo = resultado.Datos.Correo,
                NombreUsuario = resultado.Datos.NombreUsuario,
                TipoUsuario = resultado.Datos.Rol,
                MontoAdicional = 0
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la edición de un usuario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(EditarUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Mapear a DTO
            var dto = _mapper.Map<ActualizarUsuarioDTO>(model);

            // Llamar al servicio
            var resultado = await _servicioUsuario.ActualizarUsuarioAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Usuarios));
            }

            // Si hubo error
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        /// <summary>
        /// Cambia el estado de un usuario (activar/desactivar)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoUsuario(string id)
        {
            // Obtener el ID del usuario actual
            var usuarioActualId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Llamar al servicio
            var resultado = await _servicioUsuario.CambiarEstadoAsync(id, usuarioActualId);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
            }
            else
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
            }

            return RedirectToAction(nameof(Usuarios));
        }

        // ==================== MÉTODOS AJAX PARA VALIDACIONES ====================

        /// <summary>
        /// Verifica si un nombre de usuario ya existe (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> VerificarNombreUsuario(string nombreUsuario, string idExcluir = null)
        {
            var existe = await _servicioUsuario.ExisteNombreUsuarioAsync(nombreUsuario, idExcluir);
            return Json(!existe); // Retornar true si NO existe (para la validación)
        }

        /// <summary>
        /// Verifica si un correo ya existe (AJAX)
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> VerificarCorreo(string correo, string idExcluir = null)
        {
            var existe = await _servicioUsuario.ExisteCorreoAsync(correo, idExcluir);
            return Json(!existe); // Retornar true si NO existe
        }


        // ⚠️ AGREGAR ESTOS MÉTODOS AL AdminController.cs (después de la gestión de usuarios)

        // ==================== GESTIÓN DE PRÉSTAMOS ====================

        /// <summary>
        /// Lista paginada de préstamos con filtros
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Prestamos(int pagina = 1, string cedula = null, bool? estado = null)
        {
            // TODO: Implementar con el servicio de préstamos
            // Por ahora retornamos una vista vacía
            return View();
        }

        /// <summary>
        /// PASO 1: Muestra el listado de clientes para seleccionar uno
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AsignarPrestamo()
        {
            // Obtener clientes sin préstamo activo
            var resultado = await _servicioUsuario.ObtenerClientesSinPrestamoActivoAsync();

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Prestamos));
            }

            // Obtener deuda promedio
            var deudaPromedioResult = await _servicioPrestamo.ObtenerDeudaPromedioAsync();
            var deudaPromedio = deudaPromedioResult.Exito ? deudaPromedioResult.Datos : 0;

            // Mapear a ViewModel
            var viewModel = new SeleccionarClientePrestamoViewModel
            {
                Clientes = resultado.Datos.Select(c => new ClienteParaPrestamoViewModel
                {
                    Id = c.Id,
                    Cedula = c.Cedula,
                    NombreCompleto = c.NombreCompleto,
                    Correo = c.Correo,
                    DeudaTotal = c.MontoInicial // Reutilizamos este campo
                }),
                DeudaPromedio = deudaPromedio
            };

            return View(viewModel);
        }

        /// <summary>
        /// PASO 2: Muestra el formulario para configurar el préstamo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigurarPrestamo(string clienteId)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(AsignarPrestamo));
            }

            // Obtener datos del cliente
            var clienteResult = await _servicioUsuario.ObtenerUsuarioPorIdAsync(clienteId);

            if (!clienteResult.Exito)
            {
                TempData["ErrorMessage"] = clienteResult.Mensaje;
                return RedirectToAction(nameof(AsignarPrestamo));
            }

            // Obtener deuda promedio y deuda actual del cliente
            var deudaPromedioResult = await _servicioPrestamo.ObtenerDeudaPromedioAsync();
            var deudaPromedio = deudaPromedioResult.Exito ? deudaPromedioResult.Datos : 0;

            var viewModel = new ConfigurarPrestamoViewModel
            {
                ClienteId = clienteId,
                NombreCliente = clienteResult.Datos.NombreCompleto,
                DeudaActualCliente = clienteResult.Datos.MontoInicial,
                DeudaPromedio = deudaPromedio
            };

            return View(viewModel);
        }

        /// <summary>
        /// PASO 3: Procesa la asignación del préstamo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarAsignacionPrestamo(ConfigurarPrestamoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigurarPrestamo", model);
            }

            // Obtener ID del administrador actual
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Crear el DTO
            var dto = new AsignarPrestamoDTO
            {
                ClienteId = model.ClienteId,
                AdministradorId = adminId,
                MontoCapital = model.MontoCapital,
                PlazoMeses = model.PlazoMeses,
                TasaInteresAnual = model.TasaInteresAnual
            };

            // Primero validar si es cliente de alto riesgo
            var riesgoResult = await _servicioPrestamo.ValidarRiesgoClienteAsync(
                model.ClienteId,
                model.MontoCapital);

            if (riesgoResult.Exito && riesgoResult.Datos) // Es alto riesgo
            {
                // Mostrar pantalla de advertencia
                var deudaPromedioResult = await _servicioPrestamo.ObtenerDeudaPromedioAsync();
                var deudaPromedio = deudaPromedioResult.Exito ? deudaPromedioResult.Datos : 0;

                var advertenciaVM = new AdvertenciaRiesgoViewModel
                {
                    ClienteId = model.ClienteId,
                    NombreCliente = model.NombreCliente,
                    MontoCapital = model.MontoCapital,
                    PlazoMeses = model.PlazoMeses,
                    TasaInteresAnual = model.TasaInteresAnual,
                    DeudaActual = model.DeudaActualCliente,
                    DeudaPromedio = deudaPromedio,
                    DeudaDespuesDelPrestamo = model.DeudaActualCliente + model.MontoCapital,
                    MensajeAdvertencia = model.DeudaActualCliente > deudaPromedio
                        ? "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema"
                        : "Asignar este préstamo convertirá al cliente en un cliente de alto riesgo, ya que su deuda superará el umbral promedio del sistema"
                };

                return View("AdvertenciaRiesgo", advertenciaVM);
            }

            // Si no es alto riesgo, o ya fue confirmado, proceder a asignar
            var resultado = await _servicioPrestamo.AsignarPrestamoAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Prestamos));
            }

            // Si hubo error
            TempData["ErrorMessage"] = resultado.Mensaje;
            return View("ConfigurarPrestamo", model);
        }

        /// <summary>
        /// Confirma la asignación del préstamo (cliente de alto riesgo)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPrestamoAltoRiesgo(AdvertenciaRiesgoViewModel model)
        {
            // Obtener ID del administrador actual
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var dto = new AsignarPrestamoDTO
            {
                ClienteId = model.ClienteId,
                AdministradorId = adminId,
                MontoCapital = model.MontoCapital,
                PlazoMeses = model.PlazoMeses,
                TasaInteresAnual = model.TasaInteresAnual
            };

            var resultado = await _servicioPrestamo.AsignarPrestamoAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Prestamos));
            }

            TempData["ErrorMessage"] = resultado.Mensaje;
            return RedirectToAction(nameof(Prestamos));
        }

        /// <summary>
        /// Muestra el detalle de un préstamo (tabla de amortización)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetallePrestamo(int id)
        {
            // TODO: Implementar con el servicio
            return View();
        }

        /// <summary>
        /// Muestra el formulario para editar la tasa de interés
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditarPrestamo(int id)
        {
            // TODO: Implementar con el servicio
            return View();
        }

        /// <summary>
        /// Procesa la actualización de la tasa de interés
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPrestamo(int id, decimal nuevaTasa)
        {
            var dto = new ActualizarTasaPrestamoDTO
            {
                PrestamoId = id,
                NuevaTasaInteres = nuevaTasa
            };

            var resultado = await _servicioPrestamo.ActualizarTasaInteresAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Prestamos));
            }

            TempData["ErrorMessage"] = resultado.Mensaje;
            return RedirectToAction(nameof(DetallePrestamo), new { id });
        }


        // ⚠️ AGREGAR ESTOS MÉTODOS AL AdminController.cs (después de gestión de préstamos)

        // ==================== GESTIÓN DE TARJETAS DE CRÉDITO ====================

        /// <summary>
        /// Lista paginada de tarjetas de crédito con filtros
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Tarjetas(int pagina = 1, string cedula = null, bool? estado = null)
        {
            // TODO: Implementar listado paginado con el servicio
            // Por ahora retornamos vista vacía para estructura
            return View(new ListaTarjetasViewModel());
        }

        /// <summary>
        /// PASO 1: Muestra el listado de clientes para seleccionar uno
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AsignarTarjeta()
        {
            // Obtener todos los clientes activos
            var resultado = await _servicioUsuario.ObtenerClientesActivosAsync();

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Tarjetas));
            }

            // Obtener deuda promedio para mostrar en la pantalla
            var deudaPromedioResult = await _servicioPrestamo.ObtenerDeudaPromedioAsync();
            var deudaPromedio = deudaPromedioResult.Exito ? deudaPromedioResult.Datos : 0;

            // Mapear a ViewModel
            var viewModel = new SeleccionarClienteTarjetaViewModel
            {
                Clientes = resultado.Datos.Select(c => new ClienteParaTarjetaViewModel
                {
                    Id = c.Id,
                    Cedula = c.Cedula,
                    NombreCompleto = c.NombreCompleto,
                    Correo = c.Correo,
                    DeudaTotal = c.MontoInicial // Reutilizamos este campo para la deuda
                }),
                DeudaPromedio = deudaPromedio
            };

            return View(viewModel);
        }

        /// <summary>
        /// PASO 2: Muestra el formulario para configurar la tarjeta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigurarTarjeta(string clienteId)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(AsignarTarjeta));
            }

            // Obtener datos del cliente
            var clienteResult = await _servicioUsuario.ObtenerUsuarioPorIdAsync(clienteId);

            if (!clienteResult.Exito)
            {
                TempData["ErrorMessage"] = clienteResult.Mensaje;
                return RedirectToAction(nameof(AsignarTarjeta));
            }

            var viewModel = new ConfigurarTarjetaViewModel
            {
                ClienteId = clienteId,
                NombreCliente = clienteResult.Datos.NombreCompleto
            };

            return View(viewModel);
        }

        /// <summary>
        /// PASO 3: Procesa la asignación de la tarjeta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarAsignacionTarjeta(ConfigurarTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigurarTarjeta", model);
            }

            // Obtener ID del administrador actual
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Crear el DTO
            var dto = new AsignarTarjetaDTO
            {
                ClienteId = model.ClienteId,
                AdministradorId = adminId,
                LimiteCredito = model.LimiteCredito
            };

            // Llamar al servicio para asignar la tarjeta
            var resultado = await _servicioTarjeta.AsignarTarjetaAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Tarjetas));
            }

            // Si hubo error
            TempData["ErrorMessage"] = resultado.Mensaje;
            return View("ConfigurarTarjeta", model);
        }

        /// <summary>
        /// Muestra el detalle de una tarjeta (todos los consumos)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetalleTarjeta(int id)
        {
            // Obtener la tarjeta con sus consumos
            var resultado = await _servicioTarjeta.ObtenerTarjetaPorIdAsync(id);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Tarjetas));
            }

            // Mapear a ViewModel
            var viewModel = new DetalleTarjetaViewModel
            {
                Id = resultado.Datos.Id,
                NumeroTarjeta = resultado.Datos.NumeroTarjeta,
                UltimosCuatroDigitos = resultado.Datos.UltimosCuatroDigitos,
                NombreCliente = $"{resultado.Datos.NombreCliente} {resultado.Datos.ApellidoCliente}",
                LimiteCredito = resultado.Datos.LimiteCredito,
                DeudaActual = resultado.Datos.DeudaActual,
                CreditoDisponible = resultado.Datos.CreditoDisponible,
                FechaExpiracion = resultado.Datos.FechaExpiracion,
                EstaActiva = resultado.Datos.EstaActiva,
                Consumos = new List<ConsumoTarjetaViewModel>() // TODO: mapear consumos
            };

            return View(viewModel);
        }

        /// <summary>
        /// Muestra el formulario para editar el límite de una tarjeta
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditarTarjeta(int id)
        {
            // Obtener la tarjeta
            var resultado = await _servicioTarjeta.ObtenerTarjetaPorIdAsync(id);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Tarjetas));
            }

            // Mapear a ViewModel
            var viewModel = new EditarTarjetaViewModel
            {
                Id = resultado.Datos.Id,
                NumeroTarjeta = resultado.Datos.NumeroTarjeta,
                UltimosCuatroDigitos = resultado.Datos.UltimosCuatroDigitos,
                NombreCliente = $"{resultado.Datos.NombreCliente} {resultado.Datos.ApellidoCliente}",
                DeudaActual = resultado.Datos.DeudaActual,
                LimiteCredito = resultado.Datos.LimiteCredito
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la actualización del límite de crédito
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTarjeta(EditarTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Crear el DTO
            var dto = new ActualizarLimiteTarjetaDTO
            {
                TarjetaId = model.Id,
                NuevoLimite = model.LimiteCredito
            };

            // Llamar al servicio
            var resultado = await _servicioTarjeta.ActualizarLimiteAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Tarjetas));
            }

            // Si hubo error
            TempData["ErrorMessage"] = resultado.Mensaje;
            return View(model);
        }

        /// <summary>
        /// Muestra la pantalla de confirmación para cancelar una tarjeta
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CancelarTarjeta(int id)
        {
            // Obtener la tarjeta
            var resultado = await _servicioTarjeta.ObtenerTarjetaPorIdAsync(id);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Tarjetas));
            }

            // Mapear a ViewModel
            var viewModel = new CancelarTarjetaViewModel
            {
                Id = resultado.Datos.Id,
                UltimosCuatroDigitos = resultado.Datos.UltimosCuatroDigitos,
                NombreCliente = $"{resultado.Datos.NombreCliente} {resultado.Datos.ApellidoCliente}",
                DeudaActual = resultado.Datos.DeudaActual
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la cancelación de una tarjeta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCancelacionTarjeta(int id)
        {
            // Llamar al servicio para cancelar la tarjeta
            var resultado = await _servicioTarjeta.CancelarTarjetaAsync(id);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
            }
            else
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
            }

            return RedirectToAction(nameof(Tarjetas));
        }


        // ⚠️ AGREGAR ESTOS MÉTODOS AL AdminController.cs (después de gestión de tarjetas)

        // ==================== GESTIÓN DE CUENTAS DE AHORRO ====================

        /// <summary>
        /// Lista paginada de cuentas de ahorro con filtros
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Cuentas(
            int pagina = 1,
            string cedula = null,
            bool? estado = null,
            bool? tipo = null)
        {
            // TODO: Implementar listado paginado con el servicio
            // Por ahora retornamos vista vacía para estructura
            return View(new ListaCuentasViewModel());
        }

        /// <summary>
        /// PASO 1: Muestra el listado de clientes para seleccionar uno
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AsignarCuenta()
        {
            // Obtener todos los clientes activos
            var resultado = await _servicioUsuario.ObtenerClientesActivosAsync();

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Cuentas));
            }

            // Mapear a ViewModel
            var viewModel = new SeleccionarClienteCuentaViewModel
            {
                Clientes = resultado.Datos.Select(c => new ClienteParaCuentaViewModel
                {
                    Id = c.Id,
                    Cedula = c.Cedula,
                    NombreCompleto = c.NombreCompleto,
                    Correo = c.Correo,
                    DeudaTotal = c.MontoInicial // Reutilizamos este campo
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// PASO 2: Muestra el formulario para configurar la cuenta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigurarCuenta(string clienteId)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(AsignarCuenta));
            }

            // Obtener datos del cliente
            var clienteResult = await _servicioUsuario.ObtenerUsuarioPorIdAsync(clienteId);

            if (!clienteResult.Exito)
            {
                TempData["ErrorMessage"] = clienteResult.Mensaje;
                return RedirectToAction(nameof(AsignarCuenta));
            }

            var viewModel = new ConfigurarCuentaViewModel
            {
                ClienteId = clienteId,
                NombreCliente = clienteResult.Datos.NombreCompleto,
                BalanceInicial = 0
            };

            return View(viewModel);
        }

        /// <summary>
        /// PASO 3: Procesa la asignación de la cuenta secundaria
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarAsignacionCuenta(ConfigurarCuentaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigurarCuenta", model);
            }

            // Obtener ID del administrador actual
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Crear el DTO
            var dto = new CrearCuentaSecundariaDTO
            {
                ClienteId = model.ClienteId,
                AdministradorId = adminId,
                BalanceInicial = model.BalanceInicial
            };

            // Llamar al servicio para crear la cuenta
            var resultado = await _servicioCuenta.CrearCuentaSecundariaAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Cuentas));
            }

            // Si hubo error
            TempData["ErrorMessage"] = resultado.Mensaje;
            return View("ConfigurarCuenta", model);
        }

        /// <summary>
        /// Muestra el detalle de una cuenta (todas las transacciones)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetalleCuenta(int id)
        {
            // Obtener la cuenta con sus transacciones
            var resultado = await _servicioCuenta.ObtenerCuentaPorIdAsync(id);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Cuentas));
            }

            // Mapear a ViewModel
            var viewModel = new DetalleCuentaViewModel
            {
                Id = resultado.Datos.Id,
                NumeroCuenta = resultado.Datos.NumeroCuenta,
                NombreCliente = $"{resultado.Datos.NombreCliente} {resultado.Datos.ApellidoCliente}",
                Balance = resultado.Datos.Balance,
                EsPrincipal = resultado.Datos.EsPrincipal,
                EstaActiva = resultado.Datos.EstaActiva,
                Transacciones = new List<TransaccionViewModel>() // TODO: mapear transacciones
            };

            return View(viewModel);
        }

        /// <summary>
        /// Muestra la pantalla de confirmación para cancelar una cuenta
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CancelarCuenta(int id)
        {
            // Obtener la cuenta
            var resultado = await _servicioCuenta.ObtenerCuentaPorIdAsync(id);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Cuentas));
            }

            // Validar que no sea cuenta principal
            if (resultado.Datos.EsPrincipal)
            {
                TempData["ErrorMessage"] = "No se puede cancelar la cuenta principal";
                return RedirectToAction(nameof(Cuentas));
            }

            // Mapear a ViewModel
            var viewModel = new CancelarCuentaViewModel
            {
                Id = resultado.Datos.Id,
                NumeroCuenta = resultado.Datos.NumeroCuenta,
                NombreCliente = $"{resultado.Datos.NombreCliente} {resultado.Datos.ApellidoCliente}",
                Balance = resultado.Datos.Balance,
                EsPrincipal = resultado.Datos.EsPrincipal
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la cancelación de una cuenta secundaria
        /// Si tiene fondos, los transfiere a la cuenta principal
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCancelacionCuenta(int id)
        {
            // Obtener ID del usuario actual (para pasarlo al servicio)
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            // Obtener primero la cuenta para saber de qué usuario es
            var cuentaResult = await _servicioCuenta.ObtenerCuentaPorIdAsync(id);

            if (!cuentaResult.Exito)
            {
                TempData["ErrorMessage"] = cuentaResult.Mensaje;
                return RedirectToAction(nameof(Cuentas));
            }

            // Llamar al servicio para cancelar la cuenta
            // Nota: El servicio necesita el ID del usuario dueño de la cuenta
            // Deberíamos agregarlo al DTO de la cuenta
            var resultado = await _servicioCuenta.CancelarCuentaAsync(id);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
            }
            else
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
            }

            return RedirectToAction(nameof(Cuentas));
        }

        // ==================== FIN DEL CONTROLADOR ====================
        // Cierra la clase aquí: }
    }

}
