using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.ViewModels;
using ArtemisBanking.Application.ViewModels.CuentaAhorro;
using ArtemisBanking.Application.ViewModels.Prestamo;
using ArtemisBanking.Application.ViewModels.TarjetaCredito;
using ArtemisBanking.Application.ViewModels.Usuario;
using ArtemisBanking.Domain.Interfaces.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
     
    /// Controlador para todas las funcionalidades del Administrador
    /// Solo accesible para usuarios con rol "Administrador"
     
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
        public async Task<IActionResult> Index()
        {
            var resultado = await _servicioUsuario.ObtenerDashboardAdminAsync();

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return View(new DashboardAdminViewModel());
            }

            // Mapeo manual de DTO a ViewModel
            var viewModel = new DashboardAdminViewModel
            {
                TotalTransacciones = resultado.Datos.TotalTransacciones,
                TransaccionesDelDia = resultado.Datos.TransaccionesDelDia,
                TotalPagos = resultado.Datos.TotalPagos,
                PagosDelDia = resultado.Datos.PagosDelDia,
                ClientesActivos = resultado.Datos.ClientesActivos,
                ClientesInactivos = resultado.Datos.ClientesInactivos,
                TotalProductosFinancieros = resultado.Datos.TotalProductosFinancieros,
                PrestamosVigentes = resultado.Datos.PrestamosVigentes,
                TarjetasActivas = resultado.Datos.TarjetasActivas,
                CuentasAhorro = resultado.Datos.CuentasAhorro,
                DeudaPromedioCliente = resultado.Datos.DeudaPromedioCliente
            };

            return View(viewModel);
        }

       // ==================== GESTIÓN DE USUARIOS ====================

         
        /// Lista paginada de usuarios con filtros
         
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
         
        /// Muestra el formulario para crear un nuevo usuario
         
        [HttpGet]
        public IActionResult CrearUsuario()
        {
            return View();
        }

         
        /// Procesa la creación de un nuevo usuario
         
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

         
        /// Muestra el formulario para editar un usuario
         
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

         
        /// Procesa la edición de un usuario
         
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

         
        /// Cambia el estado de un usuario (activar/desactivar)
         
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

         
        /// Verifica si un nombre de usuario ya existe (AJAX)
         
        [HttpGet]
        public async Task<JsonResult> VerificarNombreUsuario(string nombreUsuario, string idExcluir = null)
        {
            var existe = await _servicioUsuario.ExisteNombreUsuarioAsync(nombreUsuario, idExcluir);
            return Json(!existe); // Retornar true si NO existe (para la validación)
        }

         
        /// Verifica si un correo ya existe (AJAX)
         
        [HttpGet]
        public async Task<JsonResult> VerificarCorreo(string correo, string idExcluir = null)
        {
            var existe = await _servicioUsuario.ExisteCorreoAsync(correo, idExcluir);
            return Json(!existe); // Retornar true si NO existe
        }


        // ==================== GESTIÓN DE PRÉSTAMOS (COMPLETA) ====================
        // Esta sección va DESPUÉS de la gestión de usuarios en tu AdminController

         
        /// Lista paginada de préstamos con filtros
        /// Muestra todos los préstamos del sistema ordenados del más reciente al más antiguo
         
        [HttpGet]
        public async Task<IActionResult> Prestamos(int pagina = 1, string cedula = null, bool? estado = null)
        {
            try
            {
                // Obtenemos los préstamos paginados del repositorio
                // Si hay cédula, filtramos por ese cliente
                // Si hay estado, filtramos por activos o completados
                var (prestamos, total) = await _repositorioPrestamo.ObtenerPrestamosPaginadosAsync(
                    pagina,
                    Constantes.TamanoPaginaPorDefecto,
                    cedula,
                    estado
                );

                // Mapeamos cada préstamo a su ViewModel
                var prestamosVM = prestamos.Select(p => new PrestamoListaItemViewModel
                {
                    Id = p.Id,
                    NumeroPrestamo = p.NumeroPrestamo,
                    NombreCliente = $"{p.Cliente.Nombre} {p.Cliente.Apellido}",
                    MontoCapital = p.MontoCapital,
                    TotalCuotas = p.PlazoMeses,
                    CuotasPagadas = p.CuotasPagadas,
                    MontoPendiente = p.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota),
                    TasaInteresAnual = p.TasaInteresAnual,
                    PlazoMeses = p.PlazoMeses,
                    EstaAlDia = p.EstaAlDia
                });

                // Creamos el ViewModel para la vista
                var viewModel = new ListaPrestamosViewModel
                {
                    Prestamos = prestamosVM,
                    PaginaActual = pagina,
                    TotalPaginas = (int)Math.Ceiling((double)total / Constantes.TamanoPaginaPorDefecto),
                    TotalRegistros = total,
                    FiltroCedula = cedula,
                    FiltroEstado = estado
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de préstamos");
                TempData["ErrorMessage"] = "Error al cargar los préstamos";
                return View(new ListaPrestamosViewModel());
            }
        }

         
        /// Muestra el detalle de un préstamo (tabla de amortización completa)
        /// Aquí se ven todas las cuotas: cuáles están pagadas, cuáles no, cuáles atrasadas
         
        [HttpGet]
        public async Task<IActionResult> DetallePrestamo(int id)
        {
            try
            {
                // Obtenemos el préstamo con todas sus relaciones
                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(id);

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "Préstamo no encontrado";
                    return RedirectToAction(nameof(Prestamos));
                }

                // Obtenemos todas las cuotas de la tabla de amortización
                var cuotas = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(id);

                // Creamos el ViewModel con toda la información
                var viewModel = new DetallePrestamoViewModel
                {
                    Id = prestamo.Id,
                    NumeroPrestamo = prestamo.NumeroPrestamo,
                    NombreCliente = $"{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}",
                    MontoCapital = prestamo.MontoCapital,
                    TasaInteresAnual = prestamo.TasaInteresAnual,
                    PlazoMeses = prestamo.PlazoMeses,
                    CuotaMensual = prestamo.CuotaMensual,
                    EstaActivo = prestamo.EstaActivo,
                    FechaCreacion = prestamo.FechaCreacion,
                    // Mapeamos todas las cuotas de la tabla de amortización
                    TablaAmortizacion = cuotas.Select(c => new CuotaPrestamoViewModel
                    {
                        FechaPago = c.FechaPago,
                        MontoCuota = c.MontoCuota,
                        EstaPagada = c.EstaPagada,
                        EstaAtrasada = c.EstaAtrasada
                    })
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalle del préstamo {id}");
                TempData["ErrorMessage"] = "Error al cargar el detalle del préstamo";
                return RedirectToAction(nameof(Prestamos));
            }
        }

         
        /// Muestra el formulario para editar la tasa de interés de un préstamo
        /// Solo se pueden editar préstamos activos
        /// Al cambiar la tasa, se recalculan las cuotas futuras
         
        [HttpGet]
        public async Task<IActionResult> EditarPrestamo(int id)
        {
            try
            {
                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(id);

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "Préstamo no encontrado";
                    return RedirectToAction(nameof(Prestamos));
                }

                if (!prestamo.EstaActivo)
                {
                    TempData["ErrorMessage"] = "No se puede editar un préstamo completado";
                    return RedirectToAction(nameof(Prestamos));
                }

                var viewModel = new EditarPrestamoViewModel
                {
                    Id = prestamo.Id,
                    NumeroPrestamo = prestamo.NumeroPrestamo,
                    NombreCliente = $"{prestamo.Cliente.Nombre} {prestamo.Cliente.Apellido}",
                    TasaInteresActual = prestamo.TasaInteresAnual,
                    NuevaTasaInteres = prestamo.TasaInteresAnual // Pre-llenamos con la tasa actual
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar formulario de edición del préstamo {id}");
                TempData["ErrorMessage"] = "Error al cargar el préstamo";
                return RedirectToAction(nameof(Prestamos));
            }
        }

         
        /// Procesa la actualización de la tasa de interés
        /// Recalcula todas las cuotas futuras (las que no se han pagado)
        /// Las cuotas ya pagadas NO se modifican
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPrestamo(EditarPrestamoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Creamos el DTO para el servicio
                var dto = new ActualizarTasaPrestamoDTO
                {
                    PrestamoId = model.Id,
                    NuevaTasaInteres = model.NuevaTasaInteres
                };

                // El servicio se encarga de:
                // 1. Validar que el préstamo existe y está activo
                // 2. Obtener las cuotas futuras (no pagadas)
                // 3. Recalcular el monto de cada cuota con la nueva tasa
                // 4. Actualizar la cuota mensual del préstamo
                // 5. Enviar correo al cliente notificando el cambio
                var resultado = await _servicioPrestamo.ActualizarTasaInteresAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(DetallePrestamo), new { id = model.Id });
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar tasa del préstamo {model.Id}");
                ModelState.AddModelError(string.Empty, "Error al actualizar la tasa de interés");
                return View(model);
            }
        }


        // ==================== GESTIÓN DE TARJETAS DE CRÉDITO (COMPLETA) ====================
        // Esta sección va DESPUÉS de la gestión de préstamos en tu AdminController

         
        /// Lista paginada de tarjetas de crédito con filtros
        /// Muestra todas las tarjetas del sistema ordenadas de la más reciente a la más antigua
         
        [HttpGet]
        public async Task<IActionResult> Tarjetas(int pagina = 1, string cedula = null, bool? estado = null)
        {
            try
            {
                // Obtenemos las tarjetas paginadas del repositorio
                var (tarjetas, total) = await _repositorioTarjeta.ObtenerTarjetasPaginadasAsync(
                    pagina,
                    Constantes.TamanoPaginaPorDefecto,
                    cedula,
                    estado
                );

                // Mapeamos cada tarjeta a su ViewModel
                var tarjetasVM = tarjetas.Select(t => new TarjetaListaItemViewModel
                {
                    Id = t.Id,
                    NumeroTarjeta = t.NumeroTarjeta,
                    NombreCliente = $"{t.Cliente.Nombre} {t.Cliente.Apellido}",
                    LimiteCredito = t.LimiteCredito,
                    DeudaActual = t.DeudaActual,
                    FechaExpiracion = t.FechaExpiracion,
                    EstaActiva = t.EstaActiva
                });

                // Creamos el ViewModel para la vista
                var viewModel = new ListaTarjetasViewModel
                {
                    Tarjetas = tarjetasVM,
                    PaginaActual = pagina,
                    TotalPaginas = (int)Math.Ceiling((double)total / Constantes.TamanoPaginaPorDefecto),
                    TotalRegistros = total,
                    FiltroCedula = cedula,
                    FiltroEstado = estado
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de tarjetas");
                TempData["ErrorMessage"] = "Error al cargar las tarjetas";
                return View(new ListaTarjetasViewModel());
            }
        }

        // Los métodos AsignarTarjeta, ConfigurarTarjeta, etc. ya los tienes implementados
        // DetalleTarjeta ya está implementado también

         
        /// Muestra el detalle de una tarjeta con todos sus consumos
         
        [HttpGet]
        public async Task<IActionResult> DetalleTarjeta(int id)
        {
            try
            {
                // Obtenemos la tarjeta
                var resultado = await _servicioTarjeta.ObtenerTarjetaPorIdAsync(id);

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Tarjetas));
                }

                // Obtenemos todos los consumos de la tarjeta
                var consumos = await _repositorioConsumoTarjeta.ObtenerConsumosDeTarjetaAsync(id);

                // Mapeamos a ViewModel
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
                    // Mapeamos todos los consumos
                    Consumos = consumos.Select(c => new ConsumoTarjetaViewModel
                    {
                        FechaConsumo = c.FechaConsumo,
                        Monto = c.Monto,
                        NombreComercio = c.NombreComercio,
                        EstadoConsumo = c.EstadoConsumo
                    })
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalle de tarjeta {id}");
                TempData["ErrorMessage"] = "Error al cargar el detalle de la tarjeta";
                return RedirectToAction(nameof(Tarjetas));
            }
        }


        // ==================== GESTIÓN DE CUENTAS DE AHORRO (COMPLETA) ====================
        // Esta sección va DESPUÉS de la gestión de tarjetas en tu AdminController

         
        /// Lista paginada de cuentas de ahorro con filtros
        /// Muestra todas las cuentas (principales y secundarias) ordenadas de la más reciente a la más antigua
         
        [HttpGet]
        public async Task<IActionResult> Cuentas(
            int pagina = 1,
            string cedula = null,
            bool? estado = null,
            bool? tipo = null) // true = principal, false = secundaria
        {
            try
            {
                // Obtenemos las cuentas paginadas del repositorio
                var (cuentas, total) = await _repositorioCuenta.ObtenerCuentasPaginadasAsync(
                    pagina,
                    Constantes.TamanoPaginaPorDefecto,
                    cedula,
                    estado,
                    tipo
                );

                // Mapeamos cada cuenta a su ViewModel
                var cuentasVM = cuentas.Select(c => new CuentaListaItemViewModel
                {
                    Id = c.Id,
                    NumeroCuenta = c.NumeroCuenta,
                    NombreCliente = $"{c.Usuario.Nombre} {c.Usuario.Apellido}",
                    Balance = c.Balance,
                    TipoCuenta = c.EsPrincipal ? "Principal" : "Secundaria",
                    EstaActiva = c.EstaActiva,
                    EsPrincipal = c.EsPrincipal
                });

                // Creamos el ViewModel para la vista
                var viewModel = new ListaCuentasViewModel
                {
                    Cuentas = cuentasVM,
                    PaginaActual = pagina,
                    TotalPaginas = (int)Math.Ceiling((double)total / Constantes.TamanoPaginaPorDefecto),
                    TotalRegistros = total,
                    FiltroCedula = cedula,
                    FiltroEstado = estado,
                    FiltroTipo = tipo
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener listado de cuentas");
                TempData["ErrorMessage"] = "Error al cargar las cuentas";
                return View(new ListaCuentasViewModel());
            }
        }

        // Los métodos AsignarCuenta, ConfigurarCuenta, etc. ya los tienes implementados

         
        /// Muestra el detalle de una cuenta con todas sus transacciones
        /// Aquí se ven todos los movimientos: depósitos, retiros, transferencias, pagos, etc.
         
        [HttpGet]
        public async Task<IActionResult> DetalleCuenta(int id)
        {
            try
            {
                // Obtenemos la cuenta
                var resultado = await _servicioCuenta.ObtenerCuentaPorIdAsync(id);

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Cuentas));
                }

                // Obtenemos todas las transacciones de la cuenta
                var transacciones = await _repositorioTransaccion.ObtenerTransaccionesDeCuentaAsync(id);

                // Mapeamos a ViewModel
                var viewModel = new DetalleCuentaViewModel
                {
                    Id = resultado.Datos.Id,
                    NumeroCuenta = resultado.Datos.NumeroCuenta,
                    NombreCliente = $"{resultado.Datos.NombreCliente} {resultado.Datos.ApellidoCliente}",
                    Balance = resultado.Datos.Balance,
                    EsPrincipal = resultado.Datos.EsPrincipal,
                    EstaActiva = resultado.Datos.EstaActiva,
                    // Mapeamos todas las transacciones
                    Transacciones = transacciones.Select(t => new TransaccionViewModel
                    {
                        FechaTransaccion = t.FechaTransaccion,
                        Monto = t.Monto,
                        TipoTransaccion = t.TipoTransaccion,
                        Beneficiario = t.Beneficiario,
                        Origen = t.Origen,
                        EstadoTransaccion = t.EstadoTransaccion
                    })
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalle de cuenta {id}");
                TempData["ErrorMessage"] = "Error al cargar el detalle de la cuenta";
                return RedirectToAction(nameof(Cuentas));
            }
        }

        // CancelarCuenta ya lo tienes implementado
        // ConfirmarCancelacionCuenta ya lo tienes implementado, solo necesita este ajuste:

         
        /// Procesa la cancelación de una cuenta secundaria
        /// Si tiene fondos, los transfiere a la cuenta principal automáticamente
        /// Las cuentas principales NO se pueden cancelar
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCancelacionCuenta(int id)
        {
            try
            {
                // El servicio se encarga de:
                // 1. Validar que la cuenta existe
                // 2. Validar que NO sea cuenta principal
                // 3. Si tiene balance, transferirlo a la cuenta principal del cliente
                // 4. Marcar la cuenta como inactiva
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cancelar cuenta {id}");
                TempData["ErrorMessage"] = "Error al cancelar la cuenta";
                return RedirectToAction(nameof(Cuentas));
            }
        }


        // ==================== ASIGNACIÓN DE PRÉSTAMOS (COMPLETO) ====================

         
        /// Muestra el listado de clientes para asignar préstamo
         
        [HttpGet]
        public async Task<IActionResult> AsignarPrestamo()
        {
            try
            {
                // Obtener clientes sin préstamo activo
                var resultado = await _servicioUsuario.ObtenerClientesSinPrestamoActivoAsync();

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Prestamos));
                }

                // Obtener deuda promedio del sistema
                var deudaPromedioResult = await _servicioPrestamo.ObtenerDeudaPromedioAsync();

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
                    DeudaPromedio = deudaPromedioResult.Exito ? deudaPromedioResult.Datos : 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar clientes para préstamo");
                TempData["ErrorMessage"] = "Error al cargar los clientes";
                return RedirectToAction(nameof(Prestamos));
            }
        }

         
        /// Muestra el formulario para configurar el préstamo
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigurarPrestamo(string clienteId)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(AsignarPrestamo));
            }

            try
            {
                // Obtener datos del cliente
                var clienteResult = await _servicioUsuario.ObtenerUsuarioPorIdAsync(clienteId);

                if (!clienteResult.Exito)
                {
                    TempData["ErrorMessage"] = "Cliente no encontrado";
                    return RedirectToAction(nameof(AsignarPrestamo));
                }

                // Obtener deuda actual y promedio
                var deudaActual = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(clienteId);
                var deudaPromedio = await _repositorioPrestamo.CalcularDeudaPromedioAsync();

                var viewModel = new ConfigurarPrestamoViewModel
                {
                    ClienteId = clienteId,
                    NombreCliente = clienteResult.Datos.NombreCompleto,
                    DeudaActualCliente = deudaActual,
                    DeudaPromedio = deudaPromedio
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar préstamo");
                TempData["ErrorMessage"] = "Error al configurar el préstamo";
                return RedirectToAction(nameof(AsignarPrestamo));
            }
        }

         
        /// Procesa la asignación del préstamo
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarAsignacionPrestamo(ConfigurarPrestamoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigurarPrestamo", model);
            }

            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var dto = new AsignarPrestamoDTO
                {
                    ClienteId = model.ClienteId,
                    AdministradorId = adminId,
                    MontoCapital = model.MontoCapital,
                    PlazoMeses = model.PlazoMeses,
                    TasaInteresAnual = model.TasaInteresAnual
                };

                // Validar riesgo del cliente
                var riesgoResult = await _servicioPrestamo.ValidarRiesgoClienteAsync(
                    model.ClienteId,
                    model.MontoCapital
                );

                // ⭐ SI ES ALTO RIESGO, mostrar advertencia
                if (riesgoResult.Datos) // Ahora verificamos el resultado real
                {
                    var deudaActual = await _repositorioPrestamo.CalcularDeudaTotalClienteAsync(model.ClienteId);
                    var deudaPromedio = await _repositorioPrestamo.CalcularDeudaPromedioAsync();

                    var advertenciaVM = new AdvertenciaRiesgoViewModel
                    {
                        ClienteId = model.ClienteId,
                        NombreCliente = model.NombreCliente,
                        MontoCapital = model.MontoCapital,
                        PlazoMeses = model.PlazoMeses,
                        TasaInteresAnual = model.TasaInteresAnual,
                        DeudaActual = deudaActual,
                        DeudaPromedio = deudaPromedio,
                        DeudaDespuesDelPrestamo = deudaActual + model.MontoCapital,
                        MensajeAdvertencia = "Este cliente tiene un nivel de riesgo alto. Revise cuidadosamente antes de continuar."
                    };

                    return View("AdvertenciaRiesgoPrestamo", advertenciaVM);
                }

                // ⭐ SI NO ES ALTO RIESGO, asignar directamente
                var resultado = await _servicioPrestamo.AsignarPrestamoAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Prestamos));
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View("ConfigurarPrestamo", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar asignación de préstamo");
                ModelState.AddModelError(string.Empty, "Error al asignar el préstamo");
                return View("ConfigurarPrestamo", model);
            }
        }

         
        /// Confirma la asignación de préstamo para cliente de alto riesgo
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPrestamoAltoRiesgo(AdvertenciaRiesgoViewModel model)
        {
            try
            {
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
                }
                else
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                }

                return RedirectToAction(nameof(Prestamos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar préstamo de alto riesgo");
                TempData["ErrorMessage"] = "Error al asignar el préstamo";
                return RedirectToAction(nameof(Prestamos));
            }
        }

        // ==================== ASIGNACIÓN DE TARJETAS ====================

         
        /// Muestra el listado de clientes para asignar tarjeta
         
        [HttpGet]
        public async Task<IActionResult> AsignarTarjeta()
        {
            try
            {
                var resultado = await _servicioUsuario.ObtenerClientesActivosAsync();

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Tarjetas));
                }

                var deudaPromedioResult = await _servicioPrestamo.ObtenerDeudaPromedioAsync();

                var viewModel = new SeleccionarClienteTarjetaViewModel
                {
                    Clientes = resultado.Datos.Select(c => new ClienteParaTarjetaViewModel
                    {
                        Id = c.Id,
                        Cedula = c.Cedula,
                        NombreCompleto = c.NombreCompleto,
                        Correo = c.Correo,
                        DeudaTotal = c.MontoInicial
                    }),
                    DeudaPromedio = deudaPromedioResult.Exito ? deudaPromedioResult.Datos : 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar clientes para tarjeta");
                TempData["ErrorMessage"] = "Error al cargar los clientes";
                return RedirectToAction(nameof(Tarjetas));
            }
        }

         
        /// Muestra el formulario para configurar la tarjeta
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigurarTarjeta(string clienteId)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(AsignarTarjeta));
            }

            try
            {
                var clienteResult = await _servicioUsuario.ObtenerUsuarioPorIdAsync(clienteId);

                if (!clienteResult.Exito)
                {
                    TempData["ErrorMessage"] = "Cliente no encontrado";
                    return RedirectToAction(nameof(AsignarTarjeta));
                }

                var viewModel = new ConfigurarTarjetaViewModel
                {
                    ClienteId = clienteId,
                    NombreCliente = clienteResult.Datos.NombreCompleto
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar tarjeta");
                TempData["ErrorMessage"] = "Error al configurar la tarjeta";
                return RedirectToAction(nameof(AsignarTarjeta));
            }
        }

         
        /// Procesa la asignación de la tarjeta
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarAsignacionTarjeta(ConfigurarTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigurarTarjeta", model);
            }

            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var dto = new AsignarTarjetaDTO
                {
                    ClienteId = model.ClienteId,
                    AdministradorId = adminId,
                    LimiteCredito = model.LimiteCredito
                };

                var resultado = await _servicioTarjeta.AsignarTarjetaAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Tarjetas));
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View("ConfigurarTarjeta", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar tarjeta");
                ModelState.AddModelError(string.Empty, "Error al asignar la tarjeta");
                return View("ConfigurarTarjeta", model);
            }
        }

        // ==================== EDITAR TARJETA ====================

        [HttpGet]
        public async Task<IActionResult> EditarTarjeta(int id)
        {
            try
            {
                var resultado = await _servicioTarjeta.ObtenerTarjetaPorIdAsync(id);

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Tarjetas));
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar tarjeta {id}");
                TempData["ErrorMessage"] = "Error al cargar la tarjeta";
                return RedirectToAction(nameof(Tarjetas));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarTarjeta(EditarTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var dto = new ActualizarLimiteTarjetaDTO
                {
                    TarjetaId = model.Id,
                    NuevoLimite = model.LimiteCredito
                };

                var resultado = await _servicioTarjeta.ActualizarLimiteAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Tarjetas));
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al editar tarjeta {model.Id}");
                ModelState.AddModelError(string.Empty, "Error al actualizar la tarjeta");
                return View(model);
            }
        }

        // ==================== CANCELAR TARJETA ====================

        [HttpGet]
        public async Task<IActionResult> CancelarTarjeta(int id)
        {
            try
            {
                var resultado = await _servicioTarjeta.ObtenerTarjetaPorIdAsync(id);

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Tarjetas));
                }

                var viewModel = new CancelarTarjetaViewModel
                {
                    Id = resultado.Datos.Id,
                    UltimosCuatroDigitos = resultado.Datos.UltimosCuatroDigitos,
                    NombreCliente = $"{resultado.Datos.NombreCliente} {resultado.Datos.ApellidoCliente}",
                    DeudaActual = resultado.Datos.DeudaActual
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar tarjeta para cancelación {id}");
                TempData["ErrorMessage"] = "Error al cargar la tarjeta";
                return RedirectToAction(nameof(Tarjetas));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCancelacionTarjeta(int id)
        {
            try
            {
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cancelar tarjeta {id}");
                TempData["ErrorMessage"] = "Error al cancelar la tarjeta";
                return RedirectToAction(nameof(Tarjetas));
            }
        }

        // ==================== ASIGNACIÓN DE CUENTAS DE AHORRO ====================

        [HttpGet]
        public async Task<IActionResult> AsignarCuenta()
        {
            try
            {
                var resultado = await _servicioUsuario.ObtenerClientesActivosAsync();

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Cuentas));
                }

                var viewModel = new SeleccionarClienteCuentaViewModel
                {
                    Clientes = resultado.Datos.Select(c => new ClienteParaCuentaViewModel
                    {
                        Id = c.Id,
                        Cedula = c.Cedula,
                        NombreCompleto = c.NombreCompleto,
                        Correo = c.Correo,
                        DeudaTotal = c.MontoInicial
                    })
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar clientes para cuenta");
                TempData["ErrorMessage"] = "Error al cargar los clientes";
                return RedirectToAction(nameof(Cuentas));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigurarCuenta(string clienteId)
        {
            if (string.IsNullOrEmpty(clienteId))
            {
                TempData["ErrorMessage"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(AsignarCuenta));
            }

            try
            {
                var clienteResult = await _servicioUsuario.ObtenerUsuarioPorIdAsync(clienteId);

                if (!clienteResult.Exito)
                {
                    TempData["ErrorMessage"] = "Cliente no encontrado";
                    return RedirectToAction(nameof(AsignarCuenta));
                }

                var viewModel = new ConfigurarCuentaViewModel
                {
                    ClienteId = clienteId,
                    NombreCliente = clienteResult.Datos.NombreCompleto
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar cuenta");
                TempData["ErrorMessage"] = "Error al configurar la cuenta";
                return RedirectToAction(nameof(AsignarCuenta));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarAsignacionCuenta(ConfigurarCuentaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfigurarCuenta", model);
            }

            try
            {
                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var dto = new CrearCuentaSecundariaDTO
                {
                    ClienteId = model.ClienteId,
                    AdministradorId = adminId,
                    BalanceInicial = model.BalanceInicial
                };

                var resultado = await _servicioCuenta.CrearCuentaSecundariaAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Cuentas));
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View("ConfigurarCuenta", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar cuenta");
                ModelState.AddModelError(string.Empty, "Error al asignar la cuenta");
                return View("ConfigurarCuenta", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CancelarCuenta(int id)
        {
            try
            {
                var resultado = await _servicioCuenta.ObtenerCuentaPorIdAsync(id);

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Cuentas));
                }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar cuenta para cancelación {id}");
                TempData["ErrorMessage"] = "Error al cargar la cuenta";
                return RedirectToAction(nameof(Cuentas));
            }
        }

    }
}
