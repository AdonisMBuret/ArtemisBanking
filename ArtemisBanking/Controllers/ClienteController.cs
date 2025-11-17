using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces.Repositories;
using ArtemisBanking.Application.ViewModels.Cliente;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ArtemisBanking.Application.ViewModels.Prestamo;

namespace ArtemisBanking.Web.Controllers
{
   
    [Authorize(Policy = "SoloCliente")] 
    public class ClienteController : Controller
    {
        // Servicios que vamos a usar
        private readonly IServicioTransaccion _servicioTransaccion;
        private readonly IServicioBeneficiario _servicioBeneficiario;
        private readonly IServicioCuentaAhorro _servicioCuenta;

        // Repositorios para obtener datos directamente
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioCuotaPrestamo _repositorioCuotaPrestamo;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumoTarjeta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;

        private readonly IMapper _mapper;
        private readonly ILogger<ClienteController> _logger;

        // Constructor: recibe todas las dependencias
        public ClienteController(
            IServicioTransaccion servicioTransaccion,
            IServicioBeneficiario servicioBeneficiario,
            IServicioCuentaAhorro servicioCuenta,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioCuotaPrestamo repositorioCuotaPrestamo,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioConsumoTarjeta repositorioConsumoTarjeta,
            IRepositorioTransaccion repositorioTransaccion,
            IMapper mapper,
            ILogger<ClienteController> logger)
        {
            _servicioTransaccion = servicioTransaccion;
            _servicioBeneficiario = servicioBeneficiario;
            _servicioCuenta = servicioCuenta;
            _repositorioCuenta = repositorioCuenta;
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioCuotaPrestamo = repositorioCuotaPrestamo;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioConsumoTarjeta = repositorioConsumoTarjeta;
            _repositorioTransaccion = repositorioTransaccion;
            _mapper = mapper;
            _logger = logger;
        }

        // Método helper para obtener el ID del cliente que está logueado
        private string ObtenerUsuarioActualId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        // ==================== HOME (LISTADO DE PRODUCTOS) ====================

      
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                // 1. OBTENEMOS TODAS LAS CUENTAS DE AHORRO ACTIVAS
                var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(usuarioId);

                var cuentasVM = cuentas.Select(c => new CuentaClienteViewModel
                {
                    Id = c.Id,
                    NumeroCuenta = c.NumeroCuenta,
                    Balance = c.Balance,
                    TipoCuenta = c.EsPrincipal ? "Principal" : "Secundaria",
                    EsPrincipal = c.EsPrincipal
                }).ToList();

                // 2. OBTENEMOS TODOS LOS PRÉSTAMOS ACTIVOS
                var prestamos = await _repositorioPrestamo.ObtenerPrestamosDeUsuarioAsync(usuarioId);
                var prestamosActivos = prestamos.Where(p => p.EstaActivo).ToList();

                var prestamosVM = new List<PrestamoClienteViewModel>();

                foreach (var prestamo in prestamosActivos)
                {
                    // Obtenemos las cuotas para calcular el monto pendiente
                    var cuotas = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(prestamo.Id);
                    var cuotasPendientes = cuotas.Where(c => !c.EstaPagada).ToList();

                    prestamosVM.Add(new PrestamoClienteViewModel
                    {
                        Id = prestamo.Id,
                        NumeroPrestamo = prestamo.NumeroPrestamo,
                        MontoCapital = prestamo.MontoCapital,
                        TotalCuotas = prestamo.PlazoMeses,
                        CuotasPagadas = prestamo.CuotasPagadas,
                        MontoPendiente = cuotasPendientes.Sum(c => c.MontoCuota),
                        TasaInteresAnual = prestamo.TasaInteresAnual,
                        PlazoMeses = prestamo.PlazoMeses,
                        Estado = prestamo.EstaAlDia ? "Al día" : "En mora",
                        EstaAlDia = prestamo.EstaAlDia
                    });
                }

                // 3. OBTENEMOS TODAS LAS TARJETAS DE CRÉDITO ACTIVAS
                var tarjetas = await _repositorioTarjeta.ObtenerTarjetasActivasDeUsuarioAsync(usuarioId);

                var tarjetasVM = tarjetas.Select(t => new TarjetaClienteViewModel
                {
                    Id = t.Id,
                    NumeroTarjeta = t.NumeroTarjeta,
                    LimiteCredito = t.LimiteCredito,
                    DeudaActual = t.DeudaActual,
                    CreditoDisponible = t.CreditoDisponible,
                    FechaExpiracion = t.FechaExpiracion
                }).ToList();

                // 4. CREAMOS EL VIEWMODEL CON TODOS LOS PRODUCTOS
                var viewModel = new HomeClienteViewModel
                {
                    CuentasAhorro = cuentasVM,
                    Prestamos = prestamosVM,
                    TarjetasCredito = tarjetasVM
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el home del cliente");
                TempData["ErrorMessage"] = "Error al cargar tus productos financieros";
                return View(new HomeClienteViewModel
                {
                    CuentasAhorro = new List<CuentaClienteViewModel>(),
                    Prestamos = new List<PrestamoClienteViewModel>(),
                    TarjetasCredito = new List<TarjetaClienteViewModel>()
                });
            }
        }

        // ==================== DETALLES DE PRODUCTOS ====================
                 
        /// Muestra el detalle de una cuenta de ahorro
        /// Lista todas las transacciones de esa cuenta ordenadas de la más reciente a la más antigua
         
        [HttpGet]
        public async Task<IActionResult> DetalleCuenta(int id)
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                // Obtenemos la cuenta
                var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(id);

                // Validamos que la cuenta pertenezca al usuario logueado
                if (cuenta == null || cuenta.UsuarioId != usuarioId)
                {
                    TempData["ErrorMessage"] = "Cuenta no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // Obtenemos todas las transacciones de esta cuenta
                var transacciones = await _repositorioTransaccion.ObtenerTransaccionesDeCuentaAsync(id);

                var viewModel = new DetalleCuentaClienteViewModel
                {
                    Id = cuenta.Id,
                    NumeroCuenta = cuenta.NumeroCuenta,
                    Balance = cuenta.Balance,
                    TipoCuenta = cuenta.EsPrincipal ? "Principal" : "Secundaria",
                    EsPrincipal = cuenta.EsPrincipal,
                    // Mapeamos todas las transacciones
                    Transacciones = transacciones.Select(t => new TransaccionClienteViewModel
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
                return RedirectToAction(nameof(Index));
            }
        }

         
        /// Muestra el detalle de un préstamo
        /// Lista la tabla de amortización con todas las cuotas
         
        [HttpGet]
        public async Task<IActionResult> DetallePrestamo(int id)
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                // Obtenemos el préstamo
                var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(id);

                // Validamos que el préstamo pertenezca al usuario logueado
                if (prestamo == null || prestamo.ClienteId != usuarioId)
                {
                    TempData["ErrorMessage"] = "Préstamo no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Obtenemos todas las cuotas
                var cuotas = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(id);

                var viewModel = new DetallePrestamoClienteViewModel
                {
                    Id = prestamo.Id,
                    NumeroPrestamo = prestamo.NumeroPrestamo,
                    MontoCapital = prestamo.MontoCapital,
                    TasaInteresAnual = prestamo.TasaInteresAnual,
                    // Mapeamos todas las cuotas
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
                _logger.LogError(ex, $"Error al obtener detalle de préstamo {id}");
                TempData["ErrorMessage"] = "Error al cargar el detalle del préstamo";
                return RedirectToAction(nameof(Index));
            }
        }

         
        /// Muestra el detalle de una tarjeta de crédito
        /// Lista todos los consumos realizados con esa tarjeta
         
        [HttpGet]
        public async Task<IActionResult> DetalleTarjeta(int id)
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                // Obtenemos la tarjeta
                var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(id);

                // Validamos que la tarjeta pertenezca al usuario logueado
                if (tarjeta == null || tarjeta.ClienteId != usuarioId)
                {
                    TempData["ErrorMessage"] = "Tarjeta no encontrada";
                    return RedirectToAction(nameof(Index));
                }

                // Obtenemos todos los consumos
                var consumos = await _repositorioConsumoTarjeta.ObtenerConsumosDeTarjetaAsync(id);

                var viewModel = new DetalleTarjetaClienteViewModel
                {
                    Id = tarjeta.Id,
                    NumeroTarjeta = tarjeta.NumeroTarjeta,
                    LimiteCredito = tarjeta.LimiteCredito,
                    DeudaActual = tarjeta.DeudaActual,
                    CreditoDisponible = tarjeta.CreditoDisponible,
                    FechaExpiracion = tarjeta.FechaExpiracion,
                    // Mapeamos todos los consumos
                    Consumos = consumos.Select(c => new ConsumoTarjetaClienteViewModel
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
                return RedirectToAction(nameof(Index));
            }
        }

        // ==================== GESTIÓN DE BENEFICIARIOS ====================

         
        /// Muestra el listado de beneficiarios del cliente
        /// Un beneficiario es una cuenta a la que el cliente transfiere con frecuencia
        /// Esto evita tener que escribir el número de cuenta cada vez
         
        [HttpGet]
        public async Task<IActionResult> Beneficiarios()
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                // Obtenemos todos los beneficiarios del cliente
                var resultado = await _servicioBeneficiario.ObtenerBeneficiariosAsync(usuarioId);

                if (!resultado.Exito)
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                    return View(new ListaBeneficiariosViewModel
                    {
                        Beneficiarios = new List<BeneficiarioItemViewModel>()
                    });
                }

                // Mapeamos a ViewModel
                var viewModel = new ListaBeneficiariosViewModel
                {
                    Beneficiarios = resultado.Datos.Select(b => new BeneficiarioItemViewModel
                    {
                        Id = b.Id,
                        NombreBeneficiario = b.NombreBeneficiario,
                        ApellidoBeneficiario = b.ApellidoBeneficiario,
                        NumeroCuentaBeneficiario = b.NumeroCuentaBeneficiario
                    })
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener beneficiarios");
                TempData["ErrorMessage"] = "Error al cargar los beneficiarios";
                return View(new ListaBeneficiariosViewModel
                {
                    Beneficiarios = new List<BeneficiarioItemViewModel>()
                });
            }
        }

         
        /// Agrega un nuevo beneficiario
        /// El cliente solo necesita ingresar el número de cuenta
        /// El sistema busca a quién pertenece y lo agrega a la lista
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarBeneficiario(AgregarBeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Debe ingresar un número de cuenta válido";
                return RedirectToAction(nameof(Beneficiarios));
            }

            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                // El servicio se encarga de:
                // 1. Validar que la cuenta existe y está activa
                // 2. Validar que no sea la propia cuenta del cliente
                // 3. Validar que no esté ya registrada como beneficiario
                // 4. Agregar el beneficiario
                var resultado = await _servicioBeneficiario.AgregarBeneficiarioAsync(
                    usuarioId,
                    model.NumeroCuenta);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                }
                else
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                }

                return RedirectToAction(nameof(Beneficiarios));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar beneficiario");
                TempData["ErrorMessage"] = "Error al agregar el beneficiario";
                return RedirectToAction(nameof(Beneficiarios));
            }
        }

         
        /// Elimina un beneficiario de la lista
        /// Solo se puede eliminar si le pertenece al cliente logueado
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarBeneficiario(int id)
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                // El servicio valida que el beneficiario pertenezca al usuario
                var resultado = await _servicioBeneficiario.EliminarBeneficiarioAsync(id, usuarioId);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                }
                else
                {
                    TempData["ErrorMessage"] = resultado.Mensaje;
                }

                return RedirectToAction(nameof(Beneficiarios));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar beneficiario {id}");
                TempData["ErrorMessage"] = "Error al eliminar el beneficiario";
                return RedirectToAction(nameof(Beneficiarios));
            }
        }


         
        /// Muestra el formulario para hacer una transacción express
        /// Una transacción express es transferir dinero a cualquier cuenta
        /// sin necesidad de tenerla registrada como beneficiario
         
        [HttpGet]
        public async Task<IActionResult> TransaccionExpress()
        {
            var viewModel = new TransaccionExpressViewModel
            {
                CuentasDisponibles = await ObtenerCuentasActivasSelectAsync()
            };

            return View(viewModel);
        }

         
        /// Procesa la transacción express
        /// Primero valida y luego muestra una confirmación con el nombre del destinatario
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransaccionExpress(TransaccionExpressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }

            try
            {
                // Obtenemos información de la cuenta destino para confirmar
                var infoDestino = await _servicioTransaccion.ObtenerInfoCuentaDestinoAsync(
                    model.NumeroCuentaDestino);

                if (!infoDestino.Exito)
                {
                    ModelState.AddModelError(string.Empty, infoDestino.Mensaje);
                    model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                    return View(model);
                }

                // Redirigimos a pantalla de confirmación
                var confirmacionVM = new ConfirmarTransaccionViewModel
                {
                    NombreDestinatario = infoDestino.Datos.nombre,
                    ApellidoDestinatario = infoDestino.Datos.apellido,
                    NumeroCuentaDestino = model.NumeroCuentaDestino,
                    Monto = model.Monto,
                    CuentaOrigenId = model.CuentaOrigenId
                };

                return View("ConfirmarTransaccionExpress", confirmacionVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar transacción express");
                ModelState.AddModelError(string.Empty, "Error al procesar la transacción");
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }
        }

         
        /// Confirma y ejecuta la transacción express
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarTransaccionExpress(ConfirmarTransaccionViewModel model)
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                var dto = new TransaccionExpressDTO
                {
                    CuentaOrigenId = model.CuentaOrigenId,
                    NumeroCuentaDestino = model.NumeroCuentaDestino,
                    Monto = model.Monto,
                    UsuarioId = usuarioId
                };

                // El servicio se encarga de:
                // 1. Validar que ambas cuentas existen
                // 2. Validar fondos suficientes
                // 3. Descontar de origen y acreditar a destino
                // 4. Registrar dos transacciones (débito y crédito)
                // 5. Enviar correos a ambos clientes
                var resultado = await _servicioTransaccion.RealizarTransaccionExpressAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(TransaccionExpress));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar transacción express");
                TempData["ErrorMessage"] = "Error al procesar la transacción";
                return RedirectToAction(nameof(TransaccionExpress));
            }
        }

        // ==================== PAGO A TARJETA DE CRÉDITO ====================

         
        /// Muestra el formulario para pagar una tarjeta de crédito
        /// El cliente selecciona la tarjeta y la cuenta desde donde pagará
         
        [HttpGet]
        public async Task<IActionResult> PagarTarjeta()
        {
            var viewModel = new PagoTarjetaViewModel
            {
                TarjetasDisponibles = await ObtenerTarjetasActivasSelectAsync(),
                CuentasDisponibles = await ObtenerCuentasActivasSelectAsync()
            };

            return View(viewModel);
        }

         
        /// Procesa el pago a tarjeta de crédito
        /// IMPORTANTE: Si el monto es mayor a la deuda, solo se paga hasta la deuda
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarTarjeta(PagoTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TarjetasDisponibles = await ObtenerTarjetasActivasSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }

            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                var dto = new PagoTarjetaClienteDTO
                {
                    TarjetaId = model.TarjetaId,
                    CuentaOrigenId = model.CuentaOrigenId,
                    Monto = model.Monto,
                    UsuarioId = usuarioId
                };

                // El servicio se encarga de:
                // 1. Validar fondos suficientes en la cuenta
                // 2. Calcular monto real a pagar (máximo la deuda)
                // 3. Descontar de cuenta y reducir deuda de tarjeta
                // 4. Registrar transacción
                // 5. Enviar correo al cliente
                var resultado = await _servicioTransaccion.PagarTarjetaCreditoClienteAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                model.TarjetasDisponibles = await ObtenerTarjetasActivasSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar tarjeta");
                ModelState.AddModelError(string.Empty, "Error al procesar el pago");
                model.TarjetasDisponibles = await ObtenerTarjetasActivasSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }
        }

        // ==================== PAGO A PRÉSTAMO ====================

         
        /// Muestra el formulario para pagar un préstamo
        /// El cliente selecciona el préstamo y la cuenta desde donde pagará
         
        [HttpGet]
        public async Task<IActionResult> PagarPrestamo()
        {
            var viewModel = new PagoPrestamoViewModel
            {
                PrestamosDisponibles = await ObtenerPrestamosActivosSelectAsync(),
                CuentasDisponibles = await ObtenerCuentasActivasSelectAsync()
            };

            return View(viewModel);
        }

         
        /// Procesa el pago a préstamo
        /// El pago se aplica a las cuotas de forma secuencial
        /// Si sobra dinero, se devuelve a la cuenta
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarPrestamo(PagoPrestamoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.PrestamosDisponibles = await ObtenerPrestamosActivosSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }

            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                var dto = new PagoPrestamoClienteDTO
                {
                    PrestamoId = model.PrestamoId,
                    CuentaOrigenId = model.CuentaOrigenId,
                    Monto = model.Monto,
                    UsuarioId = usuarioId
                };

                // El servicio aplica el pago secuencialmente a las cuotas
                var resultado = await _servicioTransaccion.PagarPrestamoClienteAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                model.PrestamosDisponibles = await ObtenerPrestamosActivosSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al pagar préstamo");
                ModelState.AddModelError(string.Empty, "Error al procesar el pago");
                model.PrestamosDisponibles = await ObtenerPrestamosActivosSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }
        }


        // ==================== PAGO A BENEFICIARIO (PARTE 4/5) ====================
        // Agregar estos métodos al ClienteController DESPUÉS de PagarPrestamo

         
        /// Muestra el formulario para pagar a un beneficiario
        /// Es similar a la transacción express pero usando un beneficiario registrado
         
        [HttpGet]
        public async Task<IActionResult> PagarBeneficiario()
        {
            var viewModel = new PagoBeneficiarioViewModel
            {
                BeneficiariosDisponibles = await ObtenerBeneficiariosSelectAsync(),
                CuentasDisponibles = await ObtenerCuentasActivasSelectAsync()
            };

            return View(viewModel);
        }

         
        /// Procesa el pago a beneficiario
        /// Primero valida y luego muestra confirmación con el nombre del beneficiario
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarBeneficiario(PagoBeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.BeneficiariosDisponibles = await ObtenerBeneficiariosSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }

            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                // Obtenemos datos del beneficiario para la confirmación
                var beneficiariosResult = await _servicioBeneficiario.ObtenerBeneficiariosAsync(usuarioId);

                if (!beneficiariosResult.Exito)
                {
                    TempData["ErrorMessage"] = "Error al obtener beneficiario";
                    return RedirectToAction(nameof(PagarBeneficiario));
                }

                var beneficiario = beneficiariosResult.Datos.FirstOrDefault(b => b.Id == model.BeneficiarioId);

                if (beneficiario == null)
                {
                    TempData["ErrorMessage"] = "Beneficiario no encontrado";
                    return RedirectToAction(nameof(PagarBeneficiario));
                }

                // Redirigir a pantalla de confirmación
                var confirmacionVM = new ConfirmarTransaccionViewModel
                {
                    NombreDestinatario = beneficiario.NombreBeneficiario,
                    ApellidoDestinatario = beneficiario.ApellidoBeneficiario,
                    NumeroCuentaDestino = beneficiario.NumeroCuentaBeneficiario,
                    Monto = model.Monto,
                    CuentaOrigenId = model.CuentaOrigenId
                };

                // Guardamos el ID del beneficiario en TempData para usarlo en la confirmación
                TempData["BeneficiarioId"] = model.BeneficiarioId;

                return View("ConfirmarPagoBeneficiario", confirmacionVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar pago a beneficiario");
                TempData["ErrorMessage"] = "Error al procesar el pago";
                return RedirectToAction(nameof(PagarBeneficiario));
            }
        }

         
        /// Confirma y ejecuta el pago a beneficiario
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPagoBeneficiario(ConfirmarTransaccionViewModel model)
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();
                var beneficiarioId = (int)TempData["BeneficiarioId"];

                var dto = new PagoBeneficiarioDTO
                {
                    BeneficiarioId = beneficiarioId,
                    CuentaOrigenId = model.CuentaOrigenId,
                    Monto = model.Monto,
                    UsuarioId = usuarioId
                };

                // El servicio procesa la transacción al beneficiario
                var resultado = await _servicioTransaccion.PagarBeneficiarioAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Index));
                }

                TempData["ErrorMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(PagarBeneficiario));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al confirmar pago a beneficiario");
                TempData["ErrorMessage"] = "Error al procesar el pago";
                return RedirectToAction(nameof(PagarBeneficiario));
            }
        }

        // ==================== AVANCE DE EFECTIVO ====================

         
        /// Muestra el formulario para hacer un avance de efectivo
        /// Un avance de efectivo toma dinero de la tarjeta y lo deposita en una cuenta
        /// Se cobra un interés del 6.25% sobre el monto
         
        [HttpGet]
        public async Task<IActionResult> AvanceEfectivo()
        {
            var viewModel = new AvanceEfectivoViewModel
            {
                TarjetasDisponibles = await ObtenerTarjetasActivasSelectAsync(),
                CuentasDisponibles = await ObtenerCuentasActivasSelectAsync()
            };

            return View(viewModel);
        }

         
        /// Procesa el avance de efectivo
        /// Valida que no exceda el crédito disponible
        /// Aplica un interés del 6.25%
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvanceEfectivo(AvanceEfectivoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TarjetasDisponibles = await ObtenerTarjetasActivasSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }

            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                var dto = new AvanceEfectivoDTO
                {
                    TarjetaId = model.TarjetaId,
                    CuentaDestinoId = model.CuentaDestinoId,
                    Monto = model.Monto,
                    UsuarioId = usuarioId
                };

                // El servicio se encarga de:
                // 1. Validar que el monto no exceda el crédito disponible
                // 2. Acreditar el monto a la cuenta
                // 3. Aumentar la deuda de la tarjeta (monto + 6.25% de interés)
                // 4. Registrar la transacción en la cuenta
                // 5. Registrar el consumo en la tarjeta como "AVANCE"
                // 6. Enviar correo al cliente
                var resultado = await _servicioTransaccion.RealizarAvanceEfectivoClienteAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                model.TarjetasDisponibles = await ObtenerTarjetasActivasSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al realizar avance de efectivo");
                ModelState.AddModelError(string.Empty, "Error al procesar el avance");
                model.TarjetasDisponibles = await ObtenerTarjetasActivasSelectAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }
        }

        // ==================== TRANSFERENCIA ENTRE CUENTAS PROPIAS ====================

         
        /// Muestra el formulario para transferir entre cuentas propias
        /// El cliente puede mover dinero entre su cuenta principal y secundarias
         
        [HttpGet]
        public async Task<IActionResult> TransferirEntreCuentas()
        {
            var viewModel = new TransferenciaEntreCuentasViewModel
            {
                CuentasDisponibles = await ObtenerCuentasActivasSelectAsync()
            };

            return View(viewModel);
        }

         
        /// Procesa la transferencia entre cuentas propias
        /// Valida que ambas cuentas sean del mismo cliente
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferirEntreCuentas(TransferenciaEntreCuentasViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }

            try
            {
                var usuarioId = ObtenerUsuarioActualId();

                var dto = new TransferirEntrePropiasDTO
                {
                    CuentaOrigenId = model.CuentaOrigenId,
                    CuentaDestinoId = model.CuentaDestinoId,
                    Monto = model.Monto,
                    UsuarioId = usuarioId
                };

                // El servicio se encarga de:
                // 1. Validar que ambas cuentas pertenezcan al usuario
                // 2. Validar que no sean la misma cuenta
                // 3. Validar fondos suficientes
                // 4. Descontar de origen y acreditar a destino
                // 5. Registrar dos transacciones (débito y crédito)
                var resultado = await _servicioCuenta.TransferirEntreCuentasPropiasAsync(dto);

                if (resultado.Exito)
                {
                    TempData["SuccessMessage"] = resultado.Mensaje;
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al transferir entre cuentas");
                ModelState.AddModelError(string.Empty, "Error al realizar la transferencia");
                model.CuentasDisponibles = await ObtenerCuentasActivasSelectAsync();
                return View(model);
            }
        }


        // ==================== MÉTODOS HELPER PRIVADOS ====================

         
        /// Obtiene las cuentas activas del usuario para poblar selectores
         
        private async Task<IEnumerable<SelectListItem>> ObtenerCuentasActivasSelectAsync()
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();
                var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(usuarioId);

                return cuentas.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cuentas activas para selector");
                return Enumerable.Empty<SelectListItem>();
            }
        }

         
        /// Obtiene las tarjetas activas del usuario para poblar selectores
         
        private async Task<IEnumerable<SelectListItem>> ObtenerTarjetasActivasSelectAsync()
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();
                var tarjetas = await _repositorioTarjeta.ObtenerTarjetasActivasDeUsuarioAsync(usuarioId);

                return tarjetas.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"****{t.UltimosCuatroDigitos} - Disponible: RD${t.CreditoDisponible:N2}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tarjetas activas para selector");
                return Enumerable.Empty<SelectListItem>();
            }
        }

         
        /// Obtiene los préstamos activos del usuario para poblar selectores
         
        private async Task<IEnumerable<SelectListItem>> ObtenerPrestamosActivosSelectAsync()
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();
                var prestamos = await _repositorioPrestamo.ObtenerPrestamosDeUsuarioAsync(usuarioId);
                var prestamosActivos = prestamos.Where(p => p.EstaActivo);

                return prestamosActivos.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.NumeroPrestamo} - Pendiente: {CalcularMontoPendiente(p):N2}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener préstamos activos para selector");
                return Enumerable.Empty<SelectListItem>();
            }
        }

         
        /// Obtiene los beneficiarios del usuario para poblar selectores
         
        private async Task<IEnumerable<SelectListItem>> ObtenerBeneficiariosSelectAsync()
        {
            try
            {
                var usuarioId = ObtenerUsuarioActualId();
                var resultado = await _servicioBeneficiario.ObtenerBeneficiariosAsync(usuarioId);

                if (!resultado.Exito)
                {
                    return Enumerable.Empty<SelectListItem>();
                }

                return resultado.Datos.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.NombreBeneficiario} {b.ApellidoBeneficiario} - {b.NumeroCuentaBeneficiario}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener beneficiarios para selector");
                return Enumerable.Empty<SelectListItem>();
            }
        }

         
        /// Calcula el monto pendiente de un préstamo sumando las cuotas no pagadas
         
        private decimal CalcularMontoPendiente(Prestamo prestamo)
        {
            if (prestamo?.TablaAmortizacion == null)
                return 0;

            return prestamo.TablaAmortizacion
                .Where(c => !c.EstaPagada)
                .Sum(c => c.MontoCuota);
        }

    }
}