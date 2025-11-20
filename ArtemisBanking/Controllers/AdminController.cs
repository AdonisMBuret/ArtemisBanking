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
     
    [Authorize(Policy = "SoloAdministrador")]
    public class AdminController : Controller
    {
        private readonly IServicioUsuario _servicioUsuario;
        private readonly IServicioPrestamo _servicioPrestamo;
        private readonly IServicioTarjetaCredito _servicioTarjeta;
        private readonly IServicioCuentaAhorro _servicioCuenta;

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

            _repositorioPrestamo = repositorioPrestamo;
            _repositorioCuotaPrestamo = repositorioCuotaPrestamo;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioConsumoTarjeta = repositorioConsumoTarjeta;
            _repositorioCuenta = repositorioCuenta;
            _repositorioTransaccion = repositorioTransaccion;

            _mapper = mapper;
            _logger = logger;
        }

        //  DASHBOARD (HOME) 
        public async Task<IActionResult> Index()
        {
            var resultado = await _servicioUsuario.ObtenerDashboardAdminAsync();

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return View(new DashboardAdminViewModel());
            }

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

       //  GESTIÓN DE USUARIOS 

                  
        [HttpGet]
        public async Task<IActionResult> Usuarios(int pagina = 1, string filtroRol = null)
        {
            var usuarioActualId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

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
                    PuedeEditar = u.MontoInicial != 0 
                }),
                PaginaActual = resultado.Datos.PaginaActual,
                TotalPaginas = resultado.Datos.TotalPaginas,
                TotalRegistros = resultado.Datos.TotalRegistros,
                FiltroRol = filtroRol
            };

            return View(viewModel);
        }
         
         
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

            var dto = _mapper.Map<CrearUsuarioDTO>(model);

            var resultado = await _servicioUsuario.CrearUsuarioAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Usuarios));
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

         
        /// Muestra el formulario para editar un usuario
         
        [HttpGet]
        public async Task<IActionResult> EditarUsuario(string id)
        {
            var usuarioActualId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (id == usuarioActualId)
            {
                TempData["ErrorMessage"] = "No puede editar su propia cuenta";
                return RedirectToAction(nameof(Usuarios));
            }

            var resultado = await _servicioUsuario.ObtenerUsuarioPorIdAsync(id);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Usuarios));
            }

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

            var dto = _mapper.Map<ActualizarUsuarioDTO>(model);

            var resultado = await _servicioUsuario.ActualizarUsuarioAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Usuarios));
            }

            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

         
        /// Cambia el estado de un usuario (activar/desactivar)
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoUsuario(string id)
        {
            var usuarioActualId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

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


        //  GESTIÓN DE PRÉSTAMOS (COMPLETA CREO) 
         
        [HttpGet]
        public async Task<IActionResult> Prestamos(int pagina = 1, string cedula = null, bool? estado = null)
        {
            try
            {

                var (prestamos, total) = await _repositorioPrestamo.ObtenerPrestamosPaginadosAsync(
                    pagina,
                    Constantes.TamanoPaginaPorDefecto,
                    cedula,
                    estado
                );

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
         
        [HttpGet]
        public async Task<IActionResult> DetallePrestamo(int id)
        {
            try
            {
                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(id);

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "Préstamo no encontrado";
                    return RedirectToAction(nameof(Prestamos));
                }

                var cuotas = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(id);

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
                    NuevaTasaInteres = prestamo.TasaInteresAnual 
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

         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPrestamo(EditarPrestamoViewModel model)
        {
            _logger.LogInformation($"=== EDITANDO PRESTAMO ===");
            _logger.LogInformation($"Id: {model.Id}");
            _logger.LogInformation($"NuevaTasaInteres: {model.NuevaTasaInteres}");
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
                var dto = new ActualizarTasaPrestamoDTO
                {
                    PrestamoId = model.Id,
                    NuevaTasaInteres = model.NuevaTasaInteres
                };

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


        //  GESTIÓN DE TARJETAS DE CRÉDITO (COMPLETA CREO) 
       
        [HttpGet]
        public async Task<IActionResult> Tarjetas(int pagina = 1, string cedula = null, bool? estado = null)
        {
            try
            {
                var (tarjetas, total) = await _repositorioTarjeta.ObtenerTarjetasPaginadasAsync(
                    pagina,
                    Constantes.TamanoPaginaPorDefecto,
                    cedula,
                    estado
                );

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

        /// Muestra el detalle de una tarjeta con todos sus consumos
         
        [HttpGet]
        public async Task<IActionResult> DetalleTarjeta(int id)
        {
            try
            {
                var resultado = await _servicioTarjeta.ObtenerTarjetaPorIdAsync(id);

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Tarjetas));
                }

                var consumos = await _repositorioConsumoTarjeta.ObtenerConsumosDeTarjetaAsync(id);


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

        //  GESTIÓN DE CUENTAS DE AHORRO (COMPLETA CREO) 
      
        [HttpGet]
        public async Task<IActionResult> Cuentas(
            int pagina = 1,
            string cedula = null,
            bool? estado = null,
            bool? tipo = null) 
        {
            try
            {
                var (cuentas, total) = await _repositorioCuenta.ObtenerCuentasPaginadasAsync(
                    pagina,
                    Constantes.TamanoPaginaPorDefecto,
                    cedula,
                    estado,
                    tipo
                );

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

        
        /// Muestra el detalle de una cuenta con todas sus transacciones
         
        [HttpGet]
        public async Task<IActionResult> DetalleCuenta(int id)
        {
            try
            {
                var resultado = await _servicioCuenta.ObtenerCuentaPorIdAsync(id);

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Cuentas));
                }

                var transacciones = await _repositorioTransaccion.ObtenerTransaccionesDeCuentaAsync(id);


                var viewModel = new DetalleCuentaViewModel
                {
                    Id = resultado.Datos.Id,
                    NumeroCuenta = resultado.Datos.NumeroCuenta,
                    NombreCliente = $"{resultado.Datos.NombreCliente} {resultado.Datos.ApellidoCliente}",
                    Balance = resultado.Datos.Balance,
                    EsPrincipal = resultado.Datos.EsPrincipal,
                    EstaActiva = resultado.Datos.EstaActiva,
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

        /// Procesa la cancelación de una cuenta secundaria

         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCancelacionCuenta(int id)
        {
            try
            {
                
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


        //  ASIGNACIÓN DE PRÉSTAMOS (COMPLETO CREO) 
       
        [HttpGet]
        public async Task<IActionResult> AsignarPrestamo()
        {
            try
            {
                var resultado = await _servicioUsuario.ObtenerClientesSinPrestamoActivoAsync();

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Prestamos));
                }

                var deudaPromedioResult = await _servicioPrestamo.ObtenerDeudaPromedioAsync();

                var viewModel = new SeleccionarClientePrestamoViewModel
                {
                    Clientes = resultado.Datos.Select(c => new ClienteParaPrestamoViewModel
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

                var riesgoResult = await _servicioPrestamo.ValidarRiesgoClienteAsync(
                    model.ClienteId,
                    model.MontoCapital
                );

                if (riesgoResult.Datos) 
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

        //  ASIGNACIÓN DE TARJETAS 
         
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

        //  EDITAR TARJETA 

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
            _logger.LogInformation($"=== EDITANDO TARJETA ===");
            _logger.LogInformation($"Id: {model.Id}");
            _logger.LogInformation($"LimiteCredito: {model.LimiteCredito}");
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

        //  CANCELAR TARJETA 

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

        //  ASIGNACIÓN DE CUENTAS DE AHORRO 

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

        //  MÉTODO TEMPORAL PARA VER CUENTAS 

        [HttpGet]
        public async Task<IActionResult> ListarTodasLasCuentas()
        {
            try
            {
                var (cuentas, _) = await _repositorioCuenta.ObtenerCuentasPaginadasAsync(1, 1000, null, true, null);
                
                var cuentasInfo = cuentas.Select(c => new
                {
                    c.Id,
                    c.NumeroCuenta,
                    c.Balance,
                    Cliente = $"{c.Usuario.Nombre} {c.Usuario.Apellido}",
                    c.Usuario.Cedula,
                    Tipo = c.EsPrincipal ? "Principal" : "Secundaria",
                    Estado = c.EstaActiva ? "Activa" : "Inactiva"
                });

                return Json(cuentasInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar cuentas");
                return Json(new { error = "Error al obtener cuentas" });
            }
        }
    }
}
