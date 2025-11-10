using ArtemisBanking.Domain.Common;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces;
using ArtemisBanking.Web.ViewModels.Cliente;
using ArtemisBanking.Web.ViewModels.CuentaAhorro;
using ArtemisBanking.Web.ViewModels.Prestamo;
using ArtemisBanking.Web.ViewModels.TarjetaCredito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Web.Controllers
{
    /// <summary>
    /// Controlador principal del cliente
    /// Maneja el listado de productos, beneficiarios, transacciones y avances de efectivo
    /// </summary>
    [Authorize(Policy = "SoloCliente")]
    public class ClienteController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly IRepositorioBeneficiario _repositorioBeneficiario;
        private readonly IRepositorioCuotaPrestamo _repositorioCuotaPrestamo;
        private readonly IRepositorioConsumoTarjeta _repositorioConsumoTarjeta;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly ILogger<ClienteController> _logger;

        public ClienteController(
            UserManager<Usuario> userManager,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioTransaccion repositorioTransaccion,
            IRepositorioBeneficiario repositorioBeneficiario,
            IRepositorioCuotaPrestamo repositorioCuotaPrestamo,
            IRepositorioConsumoTarjeta repositorioConsumoTarjeta,
            IServicioCorreo servicioCorreo,
            ILogger<ClienteController> logger)
        {
            _userManager = userManager;
            _repositorioCuenta = repositorioCuenta;
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioTransaccion = repositorioTransaccion;
            _repositorioBeneficiario = repositorioBeneficiario;
            _repositorioCuotaPrestamo = repositorioCuotaPrestamo;
            _repositorioConsumoTarjeta = repositorioConsumoTarjeta;
            _servicioCorreo = servicioCorreo;
            _logger = logger;
        }

        #region Home - Listado de Productos

        /// <summary>
        /// Dashboard del cliente con todos sus productos financieros
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var clienteId = _userManager.GetUserId(User);

            // Obtener todas las cuentas de ahorro del cliente
            var cuentas = await _repositorioCuenta.ObtenerCuentasDeUsuarioAsync(clienteId);

            // Obtener todos los préstamos del cliente
            var prestamos = await _repositorioPrestamo.ObtenerPrestamosDeUsuarioAsync(clienteId);

            // Obtener todas las tarjetas del cliente
            var tarjetas = await _repositorioTarjeta.ObtenerTarjetasDeUsuarioAsync(clienteId);

            var viewModel = new HomeClienteViewModel
            {
                CuentasAhorro = cuentas.Where(c => c.EstaActiva).Select(c => new CuentaClienteViewModel
                {
                    Id = c.Id,
                    NumeroCuenta = c.NumeroCuenta,
                    Balance = c.Balance,
                    EsPrincipal = c.EsPrincipal
                }),
                Prestamos = prestamos.Where(p => p.EstaActivo).Select(p => new PrestamoClienteViewModel
                {
                    Id = p.Id,
                    NumeroPrestamo = p.NumeroPrestamo,
                    MontoCapital = p.MontoCapital,
                    TotalCuotas = p.PlazoMeses,
                    CuotasPagadas = p.CuotasPagadas,
                    MontoPendiente = p.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota),
                    TasaInteresAnual = p.TasaInteresAnual,
                    PlazoMeses = p.PlazoMeses,
                    EstaAlDia = p.EstaAlDia
                }),
                TarjetasCredito = tarjetas.Where(t => t.EstaActiva).Select(t => new TarjetaClienteViewModel
                {
                    Id = t.Id,
                    NumeroTarjeta = t.NumeroTarjeta,
                    UltimosCuatroDigitos = t.UltimosCuatroDigitos,
                    LimiteCredito = t.LimiteCredito,
                    FechaExpiracion = t.FechaExpiracion,
                    DeudaActual = t.DeudaActual
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Ver detalle de cuenta de ahorro (transacciones)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetalleCuenta(int id)
        {
            var clienteId = _userManager.GetUserId(User);
            var cuenta = await _repositorioCuenta.ObtenerPorIdAsync(id);

            if (cuenta == null || cuenta.UsuarioId != clienteId)
            {
                return NotFound();
            }

            cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(cuenta.NumeroCuenta);
            var transacciones = await _repositorioTransaccion.ObtenerTransaccionesDeCuentaAsync(id);

            var viewModel = new DetalleCuentaViewModel
            {
                Id = cuenta.Id,
                NumeroCuenta = cuenta.NumeroCuenta,
                NombreCliente = cuenta.Usuario.NombreCompleto,
                Balance = cuenta.Balance,
                EsPrincipal = cuenta.EsPrincipal,
                EstaActiva = cuenta.EstaActiva,
                Transacciones = transacciones.Select(t => new TransaccionViewModel
                {
                    Id = t.Id,
                    FechaTransaccion = t.FechaTransaccion.ToString("dd/MM/yyyy HH:mm"),
                    Monto = t.Monto,
                    TipoTransaccion = t.TipoTransaccion,
                    Beneficiario = t.Beneficiario,
                    Origen = t.Origen,
                    EstadoTransaccion = t.EstadoTransaccion
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Ver detalle de préstamo (tabla de amortización)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetallePrestamo(int id)
        {
            var clienteId = _userManager.GetUserId(User);
            var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(id);

            if (prestamo == null || prestamo.ClienteId != clienteId)
            {
                return NotFound();
            }

            prestamo = await _repositorioPrestamo.ObtenerPorNumeroPrestamoAsync(prestamo.NumeroPrestamo);
            var cuotas = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(id);

            var viewModel = new DetallePrestamoViewModel
            {
                Id = prestamo.Id,
                NumeroPrestamo = prestamo.NumeroPrestamo,
                NombreCliente = prestamo.Cliente.NombreCompleto,
                MontoCapital = prestamo.MontoCapital,
                TasaInteresAnual = prestamo.TasaInteresAnual,
                PlazoMeses = prestamo.PlazoMeses,
                CuotaMensual = prestamo.CuotaMensual,
                EstaActivo = prestamo.EstaActivo,
                TablaAmortizacion = cuotas.Select(c => new CuotaPrestamoViewModel
                {
                    Id = c.Id,
                    FechaPago = c.FechaPago.ToString("dd/MM/yyyy"),
                    MontoCuota = c.MontoCuota,
                    EstaPagada = c.EstaPagada,
                    EstaAtrasada = c.EstaAtrasada
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Ver detalle de tarjeta de crédito (consumos)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetalleTarjeta(int id)
        {
            var clienteId = _userManager.GetUserId(User);
            var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(id);

            if (tarjeta == null || tarjeta.ClienteId != clienteId)
            {
                return NotFound();
            }

            tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(tarjeta.NumeroTarjeta);
            var consumos = await _repositorioConsumoTarjeta.ObtenerConsumosDeTarjetaAsync(id);

            var viewModel = new DetalleTarjetaViewModel
            {
                Id = tarjeta.Id,
                NumeroTarjeta = tarjeta.NumeroTarjeta,
                UltimosCuatroDigitos = tarjeta.UltimosCuatroDigitos,
                NombreCliente = tarjeta.Cliente.NombreCompleto,
                LimiteCredito = tarjeta.LimiteCredito,
                DeudaActual = tarjeta.DeudaActual,
                CreditoDisponible = tarjeta.CreditoDisponible,
                FechaExpiracion = tarjeta.FechaExpiracion,
                EstaActiva = tarjeta.EstaActiva,
                Consumos = consumos.Select(c => new ConsumoTarjetaViewModel
                {
                    Id = c.Id,
                    FechaConsumo = c.FechaConsumo.ToString("dd/MM/yyyy HH:mm"),
                    Monto = c.Monto,
                    NombreComercio = c.NombreComercio,
                    EstadoConsumo = c.EstadoConsumo
                })
            };

            return View(viewModel);
        }

        #endregion

        #region Gestión de Beneficiarios

        /// <summary>
        /// Muestra la lista de beneficiarios del cliente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Beneficiarios()
        {
            var clienteId = _userManager.GetUserId(User);
            var beneficiarios = await _repositorioBeneficiario.ObtenerBeneficiariosDeUsuarioAsync(clienteId);

            var viewModel = new ListaBeneficiariosViewModel
            {
                Beneficiarios = beneficiarios.Select(b => new BeneficiarioItemViewModel
                {
                    Id = b.Id,
                    NombreBeneficiario = b.CuentaAhorro.Usuario.Nombre,
                    ApellidoBeneficiario = b.CuentaAhorro.Usuario.Apellido,
                    NumeroCuentaBeneficiario = b.NumeroCuentaBeneficiario
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Agrega un nuevo beneficiario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarBeneficiario(AgregarBeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { exito = false, mensaje = "Datos inválidos" });
            }

            var clienteId = _userManager.GetUserId(User);

            // Verificar que la cuenta existe
            var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuenta);

            if (cuenta == null || !cuenta.EstaActiva)
            {
                return Json(new { exito = false, mensaje = "El número ingresado no corresponde a ninguna cuenta válida." });
            }

            // Verificar que no sea la propia cuenta del cliente
            if (cuenta.UsuarioId == clienteId)
            {
                return Json(new { exito = false, mensaje = "No puede agregar su propia cuenta como beneficiario." });
            }

            // Verificar que no exista ya como beneficiario
            var existeBeneficiario = await _repositorioBeneficiario.ExisteBeneficiarioAsync(clienteId, model.NumeroCuenta);

            if (existeBeneficiario)
            {
                return Json(new { exito = false, mensaje = "Esta cuenta ya está registrada como beneficiario." });
            }

            // Crear el beneficiario
            var nuevoBeneficiario = new Beneficiario
            {
                NumeroCuentaBeneficiario = model.NumeroCuenta,
                UsuarioId = clienteId,
                CuentaAhorroId = cuenta.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioBeneficiario.AgregarAsync(nuevoBeneficiario);
            await _repositorioBeneficiario.GuardarCambiosAsync();

            _logger.LogInformation($"Beneficiario {model.NumeroCuenta} agregado por cliente {clienteId}");

            return Json(new { exito = true, mensaje = "Beneficiario agregado exitosamente." });
        }

        /// <summary>
        /// Elimina un beneficiario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarBeneficiario(int id)
        {
            var clienteId = _userManager.GetUserId(User);
            var beneficiario = await _repositorioBeneficiario.ObtenerPorIdAsync(id);

            if (beneficiario == null || beneficiario.UsuarioId != clienteId)
            {
                return Json(new { exito = false, mensaje = "Beneficiario no encontrado." });
            }

            await _repositorioBeneficiario.EliminarAsync(beneficiario);
            await _repositorioBeneficiario.GuardarCambiosAsync();

            _logger.LogInformation($"Beneficiario {id} eliminado por cliente {clienteId}");

            return Json(new { exito = true, mensaje = "Beneficiario eliminado exitosamente." });
        }

        #endregion

        #region Transacción Express

        /// <summary>
        /// Muestra el formulario de transacción express (a cualquier cuenta)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TransaccionExpress()
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);

            var viewModel = new TransaccionExpressViewModel
            {
                CuentasDisponibles = cuentas.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la transacción express (validaciones y confirmación)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransaccionExpress(TransaccionExpressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var clienteId = _userManager.GetUserId(User);
                var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);
                model.CuentasDisponibles = cuentas.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
                });
                return View(model);
            }

            // Verificar que la cuenta destino existe
            var cuentaDestino = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaDestino);

            if (cuentaDestino == null || !cuentaDestino.EstaActiva)
            {
                ModelState.AddModelError("NumeroCuentaDestino", "El número de cuenta ingresado no es válido.");
                return await CargarCuentasYMostrarVistaAsync(model, nameof(TransaccionExpress));
            }

            // Verificar que la cuenta origen tiene fondos suficientes
            var cuentaOrigen = await _repositorioCuenta.ObtenerPorIdAsync(model.CuentaOrigenId);

            if (cuentaOrigen.Balance < model.Monto)
            {
                ModelState.AddModelError("Monto", "El monto excede el saldo disponible.");
                return await CargarCuentasYMostrarVistaAsync(model, nameof(TransaccionExpress));
            }

            // Redirigir a confirmación
            var confirmacionViewModel = new ConfirmarTransaccionViewModel
            {
                NombreDestinatario = cuentaDestino.Usuario.Nombre,
                ApellidoDestinatario = cuentaDestino.Usuario.Apellido,
                NumeroCuentaDestino = cuentaDestino.NumeroCuenta,
                Monto = model.Monto,
                CuentaOrigenId = model.CuentaOrigenId,
                NumeroCuentaOrigen = cuentaOrigen.NumeroCuenta
            };

            return View("ConfirmarTransaccionExpress", confirmacionViewModel);
        }

        /// <summary>
        /// Confirma y procesa la transacción express
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarTransaccionExpress(ConfirmarTransaccionViewModel model)
        {
            var cuentaOrigen = await _repositorioCuenta.ObtenerPorIdAsync(model.CuentaOrigenId);
            var cuentaDestino = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaDestino);

            // Descontar de origen
            cuentaOrigen.Balance -= model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuentaOrigen);

            // Acreditar a destino
            cuentaDestino.Balance += model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuentaDestino);

            await _repositorioCuenta.GuardarCambiosAsync();

            // Registrar transacciones
            var transaccionOrigen = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = model.Monto,
                TipoTransaccion = Constantes.TipoDebito,
                Beneficiario = cuentaDestino.NumeroCuenta,
                Origen = cuentaOrigen.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuentaOrigen.Id,
                FechaCreacion = DateTime.Now
            };

            var transaccionDestino = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = model.Monto,
                TipoTransaccion = Constantes.TipoCredito,
                Beneficiario = cuentaDestino.NumeroCuenta,
                Origen = cuentaOrigen.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuentaDestino.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccionOrigen);
            await _repositorioTransaccion.AgregarAsync(transaccionDestino);
            await _repositorioTransaccion.GuardarCambiosAsync();

            // Enviar correos
            var clienteOrigen = await _userManager.FindByIdAsync(cuentaOrigen.UsuarioId);
            var clienteDestino = await _userManager.FindByIdAsync(cuentaDestino.UsuarioId);

            var ultimos4Destino = cuentaDestino.NumeroCuenta.Substring(cuentaDestino.NumeroCuenta.Length - 4);
            var ultimos4Origen = cuentaOrigen.NumeroCuenta.Substring(cuentaOrigen.NumeroCuenta.Length - 4);

            await _servicioCorreo.EnviarNotificacionTransaccionRealizadaAsync(
                clienteOrigen.Email,
                clienteOrigen.NombreCompleto,
                model.Monto,
                ultimos4Destino,
                DateTime.Now);

            await _servicioCorreo.EnviarNotificacionTransaccionRecibidaAsync(
                clienteDestino.Email,
                clienteDestino.NombreCompleto,
                model.Monto,
                ultimos4Origen,
                DateTime.Now);

            TempData["MensajeExito"] = "Transacción realizada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para cargar cuentas en caso de error
        private async Task<IActionResult> CargarCuentasYMostrarVistaAsync(TransaccionExpressViewModel model, string vistaName)
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);

            model.CuentasDisponibles = cuentas.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
            });

            return View(vistaName, model);
        }

        #endregion

        #region Pago a Tarjeta de Crédito

        /// <summary>
        /// Muestra el formulario de pago a tarjeta de crédito
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PagoTarjeta()
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);
            var tarjetas = await _repositorioTarjeta.ObtenerTarjetasActivasDeUsuarioAsync(clienteId);

            var viewModel = new PagoTarjetaViewModel
            {
                TarjetasDisponibles = tarjetas.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"**** {t.UltimosCuatroDigitos} - Deuda: RD${t.DeudaActual:N2}"
                }),
                CuentasDisponibles = cuentas.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa el pago a tarjeta de crédito
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagoTarjeta(PagoTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await CargarDatosYMostrarVistaPagoTarjetaAsync(model);
            }

            var cuentaOrigen = await _repositorioCuenta.ObtenerPorIdAsync(model.CuentaOrigenId);
            var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(model.TarjetaId);

            // Validar fondos suficientes
            if (cuentaOrigen.Balance < model.Monto)
            {
                ModelState.AddModelError("Monto", "No dispone del monto requerido.");
                return await CargarDatosYMostrarVistaPagoTarjetaAsync(model);
            }

            // Si el monto excede la deuda, solo pagar la deuda
            var montoPagar = Math.Min(model.Monto, tarjeta.DeudaActual);

            // Descontar de la cuenta
            cuentaOrigen.Balance -= montoPagar;
            await _repositorioCuenta.ActualizarAsync(cuentaOrigen);

            // Reducir deuda de la tarjeta
            tarjeta.DeudaActual -= montoPagar;
            await _repositorioTarjeta.ActualizarAsync(tarjeta);

            await _repositorioTarjeta.GuardarCambiosAsync();

            // Registrar transacción
            var transaccion = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = montoPagar,
                TipoTransaccion = Constantes.TipoDebito,
                Beneficiario = tarjeta.NumeroTarjeta,
                Origen = cuentaOrigen.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuentaOrigen.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccion);
            await _repositorioTransaccion.GuardarCambiosAsync();

            // Enviar correo
            var cliente = await _userManager.GetUserAsync(User);
            var ultimos4Tarjeta = tarjeta.UltimosCuatroDigitos;
            var ultimos4Cuenta = cuentaOrigen.NumeroCuenta.Substring(cuentaOrigen.NumeroCuenta.Length - 4);

            await _servicioCorreo.EnviarNotificacionPagoTarjetaAsync(
                cliente.Email,
                cliente.NombreCompleto,
                montoPagar,
                ultimos4Tarjeta,
                ultimos4Cuenta,
                DateTime.Now);

            TempData["MensajeExito"] = "Pago realizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> CargarDatosYMostrarVistaPagoTarjetaAsync(PagoTarjetaViewModel model)
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);
            var tarjetas = await _repositorioTarjeta.ObtenerTarjetasActivasDeUsuarioAsync(clienteId);

            model.TarjetasDisponibles = tarjetas.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"**** {t.UltimosCuatroDigitos} - Deuda: RD${t.DeudaActual:N2}"
            });
            model.CuentasDisponibles = cuentas.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
            });

            return View(model);
        }

        #endregion

        #region Pago a Préstamo

        /// <summary>
        /// Muestra el formulario de pago a préstamo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PagoPrestamo()
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);
            var prestamos = await _repositorioPrestamo.ObtenerPrestamosDeUsuarioAsync(clienteId);
            var prestamosActivos = prestamos.Where(p => p.EstaActivo).ToList();

            var viewModel = new PagoPrestamoViewModel
            {
                PrestamosDisponibles = prestamosActivos.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.NumeroPrestamo} - Pendiente: RD${p.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota):N2}"
                }),
                CuentasDisponibles = cuentas.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa el pago a préstamo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagoPrestamo(PagoPrestamoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await CargarDatosYMostrarVistaPagoPrestamoAsync(model);
            }

            var cuentaOrigen = await _repositorioCuenta.ObtenerPorIdAsync(model.CuentaOrigenId);
            var prestamo = await _repositorioPrestamo.ObtenerPorIdAsync(model.PrestamoId);

            // Validar fondos suficientes
            if (cuentaOrigen.Balance < model.Monto)
            {
                ModelState.AddModelError("Monto", "No dispone del monto requerido.");
                return await CargarDatosYMostrarVistaPagoPrestamoAsync(model);
            }

            // Aplicar el pago a las cuotas pendientes
            decimal montoRestante = model.Monto;

            while (montoRestante > 0)
            {
                var cuotaPendiente = await _repositorioCuotaPrestamo.ObtenerPrimeraCuotaPendienteAsync(prestamo.Id);

                if (cuotaPendiente == null)
                    break; // No hay más cuotas pendientes

                if (montoRestante >= cuotaPendiente.MontoCuota)
                {
                    // Pagar la cuota completa
                    montoRestante -= cuotaPendiente.MontoCuota;
                    cuotaPendiente.EstaPagada = true;
                    cuotaPendiente.EstaAtrasada = false;
                    await _repositorioCuotaPrestamo.ActualizarAsync(cuotaPendiente);
                }
                else
                {
                    // Pago parcial - no se implementa en este sistema
                    // El dinero restante se devuelve a la cuenta
                    break;
                }
            }

            await _repositorioCuotaPrestamo.GuardarCambiosAsync();

            // Descontar de la cuenta (solo lo que realmente se usó)
            decimal montoUsado = model.Monto - montoRestante;
            cuentaOrigen.Balance -= montoUsado;

            // Si sobró dinero, devolverlo
            if (montoRestante > 0)
            {
                cuentaOrigen.Balance += montoRestante;
            }

            await _repositorioCuenta.ActualizarAsync(cuentaOrigen);
            await _repositorioCuenta.GuardarCambiosAsync();

            // Verificar si el préstamo está completamente pagado
            var cuotasPendientes = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(prestamo.Id);
            if (cuotasPendientes.All(c => c.EstaPagada))
            {
                prestamo.EstaActivo = false;
                await _repositorioPrestamo.ActualizarAsync(prestamo);
                await _repositorioPrestamo.GuardarCambiosAsync();
            }

            // Registrar transacción
            var transaccion = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = montoUsado,
                TipoTransaccion = Constantes.TipoDebito,
                Beneficiario = prestamo.NumeroPrestamo,
                Origen = cuentaOrigen.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuentaOrigen.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccion);
            await _repositorioTransaccion.GuardarCambiosAsync();

            // Enviar correo
            var cliente = await _userManager.GetUserAsync(User);
            var ultimos4Cuenta = cuentaOrigen.NumeroCuenta.Substring(cuentaOrigen.NumeroCuenta.Length - 4);

            await _servicioCorreo.EnviarNotificacionPagoPrestamoAsync(
                cliente.Email,
                cliente.NombreCompleto,
                montoUsado,
                prestamo.NumeroPrestamo,
                ultimos4Cuenta,
                DateTime.Now);

            TempData["MensajeExito"] = $"Pago realizado exitosamente. Monto aplicado: RD${montoUsado:N2}";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> CargarDatosYMostrarVistaPagoPrestamoAsync(PagoPrestamoViewModel model)
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);
            var prestamos = await _repositorioPrestamo.ObtenerPrestamosDeUsuarioAsync(clienteId);
            var prestamosActivos = prestamos.Where(p => p.EstaActivo).ToList();

            model.PrestamosDisponibles = prestamosActivos.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.NumeroPrestamo} - Pendiente: RD${p.TablaAmortizacion.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota):N2}"
            });
            model.CuentasDisponibles = cuentas.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
            });

            return View(model);
        }

        #endregion

        #region Pago a Beneficiario

        /// <summary>
        /// Muestra el formulario de pago a beneficiario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PagoBeneficiario()
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);
            var beneficiarios = await _repositorioBeneficiario.ObtenerBeneficiariosDeUsuarioAsync(clienteId);

            var viewModel = new PagoBeneficiarioViewModel
            {
                BeneficiariosDisponibles = beneficiarios.Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = $"{b.NumeroCuentaBeneficiario} - {b.CuentaAhorro.Usuario.NombreCompleto}"
                }),
                CuentasDisponibles = cuentas.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa el pago a beneficiario (con confirmación)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagoBeneficiario(PagoBeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await CargarDatosYMostrarVistaPagoBeneficiarioAsync(model);
            }

            var beneficiario = await _repositorioBeneficiario.ObtenerPorIdAsync(model.BeneficiarioId);
            var cuentaOrigen = await _repositorioCuenta.ObtenerPorIdAsync(model.CuentaOrigenId);
            var cuentaDestino = await _repositorioCuenta.ObtenerPorIdAsync(beneficiario.CuentaAhorroId);

            // Validar fondos suficientes
            if (cuentaOrigen.Balance < model.Monto)
            {
                ModelState.AddModelError("Monto", "No dispone de fondos suficientes.");
                return await CargarDatosYMostrarVistaPagoBeneficiarioAsync(model);
            }

            // Redirigir a confirmación
            var confirmacionViewModel = new ConfirmarTransaccionViewModel
            {
                NombreDestinatario = cuentaDestino.Usuario.Nombre,
                ApellidoDestinatario = cuentaDestino.Usuario.Apellido,
                NumeroCuentaDestino = cuentaDestino.NumeroCuenta,
                Monto = model.Monto,
                CuentaOrigenId = model.CuentaOrigenId,
                NumeroCuentaOrigen = cuentaOrigen.NumeroCuenta
            };

            return View("ConfirmarPagoBeneficiario", confirmacionViewModel);
        }

        /// <summary>
        /// Confirma y procesa el pago a beneficiario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPagoBeneficiario(ConfirmarTransaccionViewModel model)
        {
            var cuentaOrigen = await _repositorioCuenta.ObtenerPorIdAsync(model.CuentaOrigenId);
            var cuentaDestino = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaDestino);

            // Descontar de origen
            cuentaOrigen.Balance -= model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuentaOrigen);

            // Acreditar a destino
            cuentaDestino.Balance += model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuentaDestino);

            await _repositorioCuenta.GuardarCambiosAsync();

            // Registrar transacciones
            var transaccionOrigen = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = model.Monto,
                TipoTransaccion = Constantes.TipoDebito,
                Beneficiario = cuentaDestino.NumeroCuenta,
                Origen = cuentaOrigen.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuentaOrigen.Id,
                FechaCreacion = DateTime.Now
            };

            var transaccionDestino = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = model.Monto,
                TipoTransaccion = Constantes.TipoCredito,
                Beneficiario = cuentaDestino.NumeroCuenta,
                Origen = cuentaOrigen.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuentaDestino.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccionOrigen);
            await _repositorioTransaccion.AgregarAsync(transaccionDestino);
            await _repositorioTransaccion.GuardarCambiosAsync();

            // Enviar correos
            var clienteOrigen = await _userManager.FindByIdAsync(cuentaOrigen.UsuarioId);
            var clienteDestino = await _userManager.FindByIdAsync(cuentaDestino.UsuarioId);

            var ultimos4Destino = cuentaDestino.NumeroCuenta.Substring(cuentaDestino.NumeroCuenta.Length - 4);
            var ultimos4Origen = cuentaOrigen.NumeroCuenta.Substring(cuentaOrigen.NumeroCuenta.Length - 4);

            await _servicioCorreo.EnviarNotificacionTransaccionRealizadaAsync(
                clienteOrigen.Email,
                clienteOrigen.NombreCompleto,
                model.Monto,
                ultimos4Destino,
                DateTime.Now);

            await _servicioCorreo.EnviarNotificacionTransaccionRecibidaAsync(
                clienteDestino.Email,
                clienteDestino.NombreCompleto,
                model.Monto,
                ultimos4Origen,
                DateTime.Now);

            TempData["MensajeExito"] = "Pago realizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> CargarDatosYMostrarVistaPagoBeneficiarioAsync(PagoBeneficiarioViewModel model)
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);
            var beneficiarios = await _repositorioBeneficiario.ObtenerBeneficiariosDeUsuarioAsync(clienteId);

            model.BeneficiariosDisponibles = beneficiarios.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = $"{b.NumeroCuentaBeneficiario} - {b.CuentaAhorro.Usuario.NombreCompleto}"
            });
            model.CuentasDisponibles = cuentas.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
            });

            return View(model);
        }

        #endregion

        #region Avance de Efectivo

        /// <summary>
        /// Muestra el formulario de avance de efectivo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> AvanceEfectivo()
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);
            var tarjetas = await _repositorioTarjeta.ObtenerTarjetasActivasDeUsuarioAsync(clienteId);

            var viewModel = new AvanceEfectivoViewModel
            {
                TarjetasDisponibles = tarjetas.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"**** {t.UltimosCuatroDigitos} - Disponible: RD${t.CreditoDisponible:N2}"
                }),
                CuentasDisponibles = cuentas.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa el avance de efectivo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvanceEfectivo(AvanceEfectivoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await CargarDatosYMostrarVistaAvanceEfectivoAsync(model);
            }

            var tarjeta = await _repositorioTarjeta.ObtenerPorIdAsync(model.TarjetaId);
            var cuentaDestino = await _repositorioCuenta.ObtenerPorIdAsync(model.CuentaDestinoId);

            // Validar que el monto no exceda el crédito disponible
            if (model.Monto > tarjeta.CreditoDisponible)
            {
                ModelState.AddModelError("Monto", $"El monto no puede exceder el crédito disponible (RD${tarjeta.CreditoDisponible:N2}).");
                return await CargarDatosYMostrarVistaAvanceEfectivoAsync(model);
            }

            // Calcular interés del 6.25%
            decimal interes = model.Monto * (Constantes.InteresAvanceEfectivo / 100);
            decimal deudaTotal = model.Monto + interes;

            // Acreditar a la cuenta
            cuentaDestino.Balance += model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuentaDestino);

            // Aumentar deuda de la tarjeta (monto + interés)
            tarjeta.DeudaActual += deudaTotal;
            await _repositorioTarjeta.ActualizarAsync(tarjeta);

            await _repositorioTarjeta.GuardarCambiosAsync();

            // Registrar transacción en cuenta
            var transaccion = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = model.Monto,
                TipoTransaccion = Constantes.TipoCredito,
                Beneficiario = cuentaDestino.NumeroCuenta,
                Origen = tarjeta.UltimosCuatroDigitos,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuentaDestino.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccion);

            // Registrar consumo en tarjeta
            var consumo = new ConsumoTarjeta
            {
                FechaConsumo = DateTime.Now,
                Monto = deudaTotal,
                NombreComercio = Constantes.TextoAvance,
                EstadoConsumo = Constantes.ConsumoAprobado,
                TarjetaId = tarjeta.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioConsumoTarjeta.AgregarAsync(consumo);
            await _repositorioConsumoTarjeta.GuardarCambiosAsync();

            // Enviar correo
            var cliente = await _userManager.GetUserAsync(User);
            var ultimos4Tarjeta = tarjeta.UltimosCuatroDigitos;
            var ultimos4Cuenta = cuentaDestino.NumeroCuenta.Substring(cuentaDestino.NumeroCuenta.Length - 4);

            await _servicioCorreo.EnviarNotificacionAvanceEfectivoAsync(
                cliente.Email,
                cliente.NombreCompleto,
                model.Monto,
                ultimos4Tarjeta,
                ultimos4Cuenta,
                DateTime.Now);

            TempData["MensajeExito"] = $"Avance de efectivo realizado exitosamente. Interés aplicado: RD${interes:N2}";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> CargarDatosYMostrarVistaAvanceEfectivoAsync(AvanceEfectivoViewModel model)
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);
            var tarjetas = await _repositorioTarjeta.ObtenerTarjetasActivasDeUsuarioAsync(clienteId);

            model.TarjetasDisponibles = tarjetas.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"**** {t.UltimosCuatroDigitos} - Disponible: RD${t.CreditoDisponible:N2}"
            });
            model.CuentasDisponibles = cuentas.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
            });

            return View(model);
        }

        #endregion

        #region Transferencia entre Cuentas

        /// <summary>
        /// Muestra el formulario de transferencia entre cuentas propias
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TransferenciaEntreCuentas()
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);

            var viewModel = new TransferenciaEntreCuentasViewModel
            {
                CuentasDisponibles = cuentas.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
                })
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la transferencia entre cuentas propias
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransferenciaEntreCuentas(TransferenciaEntreCuentasViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return await CargarDatosYMostrarVistaTransferenciaAsync(model);
            }

            // Validar que no sean la misma cuenta
            if (model.CuentaOrigenId == model.CuentaDestinoId)
            {
                ModelState.AddModelError("", "No puede transferir a la misma cuenta.");
                return await CargarDatosYMostrarVistaTransferenciaAsync(model);
            }

            var cuentaOrigen = await _repositorioCuenta.ObtenerPorIdAsync(model.CuentaOrigenId);
            var cuentaDestino = await _repositorioCuenta.ObtenerPorIdAsync(model.CuentaDestinoId);

            // Validar fondos suficientes
            if (cuentaOrigen.Balance < model.Monto)
            {
                ModelState.AddModelError("Monto", "No cuenta con saldo suficiente.");
                return await CargarDatosYMostrarVistaTransferenciaAsync(model);
            }

            // Descontar de origen
            cuentaOrigen.Balance -= model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuentaOrigen);

            // Acreditar a destino
            cuentaDestino.Balance += model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuentaDestino);

            await _repositorioCuenta.GuardarCambiosAsync();

            // Registrar transacciones
            var transaccionOrigen = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = model.Monto,
                TipoTransaccion = Constantes.TipoDebito,
                Beneficiario = cuentaDestino.NumeroCuenta,
                Origen = cuentaOrigen.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuentaOrigen.Id,
                FechaCreacion = DateTime.Now
            };

            var transaccionDestino = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = model.Monto,
                TipoTransaccion = Constantes.TipoCredito,
                Beneficiario = cuentaDestino.NumeroCuenta,
                Origen = cuentaOrigen.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuentaDestino.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccionOrigen);
            await _repositorioTransaccion.AgregarAsync(transaccionDestino);
            await _repositorioTransaccion.GuardarCambiosAsync();

            TempData["MensajeExito"] = "Transferencia realizada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> CargarDatosYMostrarVistaTransferenciaAsync(TransferenciaEntreCuentasViewModel model)
        {
            var clienteId = _userManager.GetUserId(User);
            var cuentas = await _repositorioCuenta.ObtenerCuentasActivasDeUsuarioAsync(clienteId);

            model.CuentasDisponibles = cuentas.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.NumeroCuenta} - Balance: RD${c.Balance:N2}"
            });

            return View(model);
        }

        #endregion
    }
}