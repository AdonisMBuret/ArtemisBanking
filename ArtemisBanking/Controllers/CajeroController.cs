using ArtemisBanking.Domain.Common;
using ArtemisBanking.Domain.Entities;
using ArtemisBanking.Domain.Interfaces;
using ArtemisBanking.Web.ViewModels;
using ArtemisBanking.Web.ViewModels.Cajero;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
    /// <summary>
    /// Controlador para las operaciones del cajero
    /// Maneja depósitos, retiros, pagos a tarjetas, préstamos y transacciones entre terceros
    /// </summary>
    [Authorize(Policy = "SoloCajero")]
    public class CajeroController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IRepositorioCuentaAhorro _repositorioCuenta;
        private readonly IRepositorioPrestamo _repositorioPrestamo;
        private readonly IRepositorioTarjetaCredito _repositorioTarjeta;
        private readonly IRepositorioTransaccion _repositorioTransaccion;
        private readonly IRepositorioCuotaPrestamo _repositorioCuotaPrestamo;
        private readonly IServicioCorreo _servicioCorreo;
        private readonly ILogger<CajeroController> _logger;

        public CajeroController(
            UserManager<Usuario> userManager,
            IRepositorioCuentaAhorro repositorioCuenta,
            IRepositorioPrestamo repositorioPrestamo,
            IRepositorioTarjetaCredito repositorioTarjeta,
            IRepositorioTransaccion repositorioTransaccion,
            IRepositorioCuotaPrestamo repositorioCuotaPrestamo,
            IServicioCorreo servicioCorreo,
            ILogger<CajeroController> logger)
        {
            _userManager = userManager;
            _repositorioCuenta = repositorioCuenta;
            _repositorioPrestamo = repositorioPrestamo;
            _repositorioTarjeta = repositorioTarjeta;
            _repositorioTransaccion = repositorioTransaccion;
            _repositorioCuotaPrestamo = repositorioCuotaPrestamo;
            _servicioCorreo = servicioCorreo;
            _logger = logger;
        }

        #region Dashboard

        /// <summary>
        /// Dashboard del cajero con indicadores del día
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Obtener indicadores del cajero actual
            var cajeroId = _userManager.GetUserId(User);
            var hoy = DateTime.Now.Date;

            // Contar transacciones del día del cajero
            // Nota: En un sistema real, deberías tener un campo CajeroId en las transacciones
            // Por ahora, contamos todas las del día
            var transaccionesDelDia = await _repositorioTransaccion.ContarTransaccionesDelDiaAsync();
            var pagosDelDia = await _repositorioTransaccion.ContarPagosDelDiaAsync();

            // Contar depósitos y retiros del día
            // En el sistema actual, los diferenciamos por el origen/beneficiario
            var todasTransacciones = await _repositorioTransaccion.ObtenerPorCondicionAsync(
                t => t.FechaTransaccion.Date == hoy && t.EstadoTransaccion == Constantes.EstadoAprobada);

            var depositosDelDia = todasTransacciones.Count(t => t.Origen == Constantes.TextoDeposito);
            var retirosDelDia = todasTransacciones.Count(t => t.Beneficiario == Constantes.TextoRetiro);

            var viewModel = new DashboardCajeroViewModel
            {
                TransaccionesDelDia = transaccionesDelDia,
                PagosDelDia = pagosDelDia,
                DepositosDelDia = depositosDelDia,
                RetirosDelDia = retirosDelDia
            };

            return View(viewModel);
        }

        #endregion

        #region Depósito

        /// <summary>
        /// Muestra el formulario de depósito
        /// </summary>
        [HttpGet]
        public IActionResult Deposito()
        {
            return View();
        }

        /// <summary>
        /// Procesa el depósito (con confirmación)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposito(DepositoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que la cuenta existe y está activa
            var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaDestino);

            if (cuenta == null || !cuenta.EstaActiva)
            {
                ModelState.AddModelError("NumeroCuentaDestino", "El número de cuenta ingresado no es válido.");
                return View(model);
            }

            // Redirigir a confirmación
            var confirmacionViewModel = new ConfirmarDepositoViewModel
            {
                NombreTitular = cuenta.Usuario.Nombre,
                ApellidoTitular = cuenta.Usuario.Apellido,
                NumeroCuenta = cuenta.NumeroCuenta,
                Monto = model.Monto
            };

            return View("ConfirmarDeposito", confirmacionViewModel);
        }

        /// <summary>
        /// Confirma y procesa el depósito
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarDeposito(ConfirmarDepositoViewModel model)
        {
            var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuenta);

            if (cuenta == null)
            {
                TempData["MensajeError"] = "Cuenta no encontrada.";
                return RedirectToAction(nameof(Deposito));
            }

            // Acreditar el monto a la cuenta
            cuenta.Balance += model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuenta);
            await _repositorioCuenta.GuardarCambiosAsync();

            // Registrar la transacción
            var transaccion = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = model.Monto,
                TipoTransaccion = Constantes.TipoCredito,
                Beneficiario = cuenta.NumeroCuenta,
                Origen = Constantes.TextoDeposito,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuenta.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccion);
            await _repositorioTransaccion.GuardarCambiosAsync();

            // Enviar correo al cliente
            var cliente = await _userManager.FindByIdAsync(cuenta.UsuarioId);
            var ultimos4 = cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4);

            await _servicioCorreo.EnviarNotificacionDepositoAsync(
                cliente.Email,
                cliente.NombreCompleto,
                model.Monto,
                ultimos4,
                DateTime.Now);

            _logger.LogInformation($"Depósito de RD${model.Monto} realizado a cuenta {cuenta.NumeroCuenta}");

            TempData["MensajeExito"] = "Depósito realizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Retiro

        /// <summary>
        /// Muestra el formulario de retiro
        /// </summary>
        [HttpGet]
        public IActionResult Retiro()
        {
            return View();
        }

        /// <summary>
        /// Procesa el retiro (con confirmación)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retiro(RetiroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que la cuenta existe y está activa
            var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaOrigen);

            if (cuenta == null || !cuenta.EstaActiva)
            {
                ModelState.AddModelError("NumeroCuentaOrigen", "El número de cuenta ingresado no es válido.");
                return View(model);
            }

            // Verificar fondos suficientes
            if (cuenta.Balance < model.Monto)
            {
                ModelState.AddModelError("Monto", "El monto excede el saldo disponible.");
                return View(model);
            }

            // Redirigir a confirmación
            var confirmacionViewModel = new ConfirmarRetiroViewModel
            {
                NombreTitular = cuenta.Usuario.Nombre,
                ApellidoTitular = cuenta.Usuario.Apellido,
                NumeroCuenta = cuenta.NumeroCuenta,
                Monto = model.Monto,
                BalanceActual = cuenta.Balance
            };

            return View("ConfirmarRetiro", confirmacionViewModel);
        }

        /// <summary>
        /// Confirma y procesa el retiro
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarRetiro(ConfirmarRetiroViewModel model)
        {
            var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuenta);

            if (cuenta == null)
            {
                TempData["MensajeError"] = "Cuenta no encontrada.";
                return RedirectToAction(nameof(Retiro));
            }

            // Verificar nuevamente los fondos
            if (cuenta.Balance < model.Monto)
            {
                TempData["MensajeError"] = "Fondos insuficientes.";
                return RedirectToAction(nameof(Retiro));
            }

            // Debitar el monto de la cuenta
            cuenta.Balance -= model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuenta);
            await _repositorioCuenta.GuardarCambiosAsync();

            // Registrar la transacción
            var transaccion = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = model.Monto,
                TipoTransaccion = Constantes.TipoDebito,
                Beneficiario = Constantes.TextoRetiro,
                Origen = cuenta.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuenta.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccion);
            await _repositorioTransaccion.GuardarCambiosAsync();

            // Enviar correo al cliente
            var cliente = await _userManager.FindByIdAsync(cuenta.UsuarioId);
            var ultimos4 = cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4);

            await _servicioCorreo.EnviarNotificacionRetiroAsync(
                cliente.Email,
                cliente.NombreCompleto,
                model.Monto,
                ultimos4,
                DateTime.Now);

            _logger.LogInformation($"Retiro de RD${model.Monto} realizado de cuenta {cuenta.NumeroCuenta}");

            TempData["MensajeExito"] = "Retiro realizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Pago a Tarjeta de Crédito

        /// <summary>
        /// Muestra el formulario de pago a tarjeta de crédito
        /// </summary>
        [HttpGet]
        public IActionResult PagoTarjeta()
        {
            return View();
        }

        /// <summary>
        /// Procesa el pago a tarjeta (con confirmación)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagoTarjeta(PagoTarjetaCajeroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que la cuenta existe y está activa
            var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaOrigen);

            if (cuenta == null || !cuenta.EstaActiva)
            {
                ModelState.AddModelError("NumeroCuentaOrigen", "El número de cuenta ingresado no es válido.");
                return View(model);
            }

            // Verificar que la tarjeta existe y está activa
            var tarjeta = await _repositorioTarjeta.ObtenerPorNumeroTarjetaAsync(model.NumeroTarjeta);

            if (tarjeta == null || !tarjeta.EstaActiva)
            {
                ModelState.AddModelError("NumeroTarjeta", "El número de tarjeta ingresado no es válido.");
                return View(model);
            }

            // Verificar fondos suficientes
            if (cuenta.Balance < model.Monto)
            {
                ModelState.AddModelError("Monto", "El monto excede el saldo disponible.");
                return View(model);
            }

            // Redirigir a confirmación
            var confirmacionViewModel = new ConfirmarPagoTarjetaCajeroViewModel
            {
                NombreTitular = tarjeta.Cliente.Nombre,
                ApellidoTitular = tarjeta.Cliente.Apellido,
                NumeroCuentaOrigen = cuenta.NumeroCuenta,
                UltimosCuatroDigitosTarjeta = tarjeta.UltimosCuatroDigitos,
                Monto = model.Monto,
                DeudaActualTarjeta = tarjeta.DeudaActual
            };

            return View("ConfirmarPagoTarjeta", confirmacionViewModel);
        }

        /// <summary>
        /// Confirma y procesa el pago a tarjeta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPagoTarjetaCajero(ConfirmarPagoTarjetaCajeroViewModel model)
        {
            var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaOrigen);
            var tarjeta = await _repositorioTarjeta.ObtenerPorCondicionAsync(
                t => t.UltimosCuatroDigitos == model.UltimosCuatroDigitosTarjeta);
            var tarjetaReal = tarjeta.FirstOrDefault();

            if (cuenta == null || tarjetaReal == null)
            {
                TempData["MensajeError"] = "Datos no encontrados.";
                return RedirectToAction(nameof(PagoTarjeta));
            }

            // Si el monto excede la deuda, solo pagar la deuda
            var montoPagar = Math.Min(model.Monto, tarjetaReal.DeudaActual);

            // Descontar de la cuenta
            cuenta.Balance -= montoPagar;
            await _repositorioCuenta.ActualizarAsync(cuenta);

            // Reducir deuda de la tarjeta
            tarjetaReal.DeudaActual -= montoPagar;
            await _repositorioTarjeta.ActualizarAsync(tarjetaReal);

            await _repositorioTarjeta.GuardarCambiosAsync();

            // Registrar transacción
            var transaccion = new Transaccion
            {
                FechaTransaccion = DateTime.Now,
                Monto = montoPagar,
                TipoTransaccion = Constantes.TipoDebito,
                Beneficiario = tarjetaReal.NumeroTarjeta,
                Origen = cuenta.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuenta.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccion);
            await _repositorioTransaccion.GuardarCambiosAsync();

            // Enviar correo
            var cliente = await _userManager.FindByIdAsync(tarjetaReal.ClienteId);
            var ultimos4Tarjeta = tarjetaReal.UltimosCuatroDigitos;
            var ultimos4Cuenta = cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4);

            await _servicioCorreo.EnviarNotificacionPagoTarjetaAsync(
                cliente.Email,
                cliente.NombreCompleto,
                montoPagar,
                ultimos4Tarjeta,
                ultimos4Cuenta,
                DateTime.Now);

            _logger.LogInformation($"Pago de RD${montoPagar} a tarjeta **** {ultimos4Tarjeta}");

            TempData["MensajeExito"] = "Pago a tarjeta realizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Pago a Préstamo

        /// <summary>
        /// Muestra el formulario de pago a préstamo
        /// </summary>
        [HttpGet]
        public IActionResult PagoPrestamo()
        {
            return View();
        }

        /// <summary>
        /// Procesa el pago a préstamo (con confirmación)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagoPrestamo(PagoPrestamoCajeroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que la cuenta existe y está activa
            var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaOrigen);

            if (cuenta == null || !cuenta.EstaActiva)
            {
                ModelState.AddModelError("NumeroCuentaOrigen", "El número de cuenta ingresado no es válido.");
                return View(model);
            }

            // Verificar que el préstamo existe y está activo
            var prestamo = await _repositorioPrestamo.ObtenerPorNumeroPrestamoAsync(model.NumeroPrestamo);

            if (prestamo == null || !prestamo.EstaActivo)
            {
                ModelState.AddModelError("NumeroPrestamo", "El número de préstamo ingresado no es válido.");
                return View(model);
            }

            // Verificar fondos suficientes
            if (cuenta.Balance < model.Monto)
            {
                ModelState.AddModelError("Monto", "El monto excede el saldo disponible.");
                return View(model);
            }

            // Calcular monto pendiente del préstamo
            var cuotas = await _repositorioCuotaPrestamo.ObtenerCuotasDePrestamoAsync(prestamo.Id);
            var montoPendiente = cuotas.Where(c => !c.EstaPagada).Sum(c => c.MontoCuota);

            // Redirigir a confirmación
            var confirmacionViewModel = new ConfirmarPagoPrestamoCajeroViewModel
            {
                NombreTitular = prestamo.Cliente.Nombre,
                ApellidoTitular = prestamo.Cliente.Apellido,
                NumeroCuentaOrigen = cuenta.NumeroCuenta,
                NumeroPrestamo = prestamo.NumeroPrestamo,
                Monto = model.Monto,
                MontoPendientePrestamo = montoPendiente
            };

            return View("ConfirmarPagoPrestamo", confirmacionViewModel);
        }

        /// <summary>
        /// Confirma y procesa el pago a préstamo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPagoPrestamoCajero(ConfirmarPagoPrestamoCajeroViewModel model)
        {
            var cuenta = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaOrigen);
            var prestamo = await _repositorioPrestamo.ObtenerPorNumeroPrestamoAsync(model.NumeroPrestamo);

            if (cuenta == null || prestamo == null)
            {
                TempData["MensajeError"] = "Datos no encontrados.";
                return RedirectToAction(nameof(PagoPrestamo));
            }

            // Aplicar el pago a las cuotas pendientes
            decimal montoRestante = model.Monto;

            while (montoRestante > 0)
            {
                var cuotaPendiente = await _repositorioCuotaPrestamo.ObtenerPrimeraCuotaPendienteAsync(prestamo.Id);

                if (cuotaPendiente == null)
                    break;

                if (montoRestante >= cuotaPendiente.MontoCuota)
                {
                    montoRestante -= cuotaPendiente.MontoCuota;
                    cuotaPendiente.EstaPagada = true;
                    cuotaPendiente.EstaAtrasada = false;
                    await _repositorioCuotaPrestamo.ActualizarAsync(cuotaPendiente);
                }
                else
                {
                    break;
                }
            }

            await _repositorioCuotaPrestamo.GuardarCambiosAsync();

            // Descontar de la cuenta
            decimal montoUsado = model.Monto - montoRestante;
            cuenta.Balance -= montoUsado;

            if (montoRestante > 0)
            {
                cuenta.Balance += montoRestante;
            }

            await _repositorioCuenta.ActualizarAsync(cuenta);
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
                Origen = cuenta.NumeroCuenta,
                EstadoTransaccion = Constantes.EstadoAprobada,
                CuentaAhorroId = cuenta.Id,
                FechaCreacion = DateTime.Now
            };

            await _repositorioTransaccion.AgregarAsync(transaccion);
            await _repositorioTransaccion.GuardarCambiosAsync();

            // Enviar correo
            var cliente = await _userManager.FindByIdAsync(prestamo.ClienteId);
            var ultimos4Cuenta = cuenta.NumeroCuenta.Substring(cuenta.NumeroCuenta.Length - 4);

            await _servicioCorreo.EnviarNotificacionPagoPrestamoAsync(
                cliente.Email,
                cliente.NombreCompleto,
                montoUsado,
                prestamo.NumeroPrestamo,
                ultimos4Cuenta,
                DateTime.Now);

            _logger.LogInformation($"Pago de RD${montoUsado} a préstamo {prestamo.NumeroPrestamo}");

            TempData["MensajeExito"] = $"Pago realizado exitosamente. Monto aplicado: RD${montoUsado:N2}";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Transacciones a Cuentas de Terceros

        /// <summary>
        /// Muestra el formulario de transacción entre cuentas de terceros
        /// </summary>
        [HttpGet]
        public IActionResult TransaccionTerceros()
        {
            return View();
        }

        /// <summary>
        /// Procesa la transacción entre terceros (con confirmación)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransaccionTerceros(TransaccionTercerosCajeroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que la cuenta origen existe y está activa
            var cuentaOrigen = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaOrigen);

            if (cuentaOrigen == null || !cuentaOrigen.EstaActiva)
            {
                ModelState.AddModelError("NumeroCuentaOrigen", "El número de cuenta origen no es válido.");
                return View(model);
            }

            // Verificar fondos suficientes
            if (cuentaOrigen.Balance < model.Monto)
            {
                ModelState.AddModelError("Monto", "El monto excede el saldo disponible.");
                return View(model);
            }

            // Verificar que la cuenta destino existe y está activa
            var cuentaDestino = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaDestino);

            if (cuentaDestino == null || !cuentaDestino.EstaActiva)
            {
                ModelState.AddModelError("NumeroCuentaDestino", "El número de cuenta destino no es válido.");
                return View(model);
            }

            // Redirigir a confirmación
            var confirmacionViewModel = new ConfirmarTransaccionTercerosViewModel
            {
                NombreOrigen = cuentaOrigen.Usuario.Nombre,
                ApellidoOrigen = cuentaOrigen.Usuario.Apellido,
                NumeroCuentaOrigen = cuentaOrigen.NumeroCuenta,
                NombreDestino = cuentaDestino.Usuario.Nombre,
                ApellidoDestino = cuentaDestino.Usuario.Apellido,
                NumeroCuentaDestino = cuentaDestino.NumeroCuenta,
                Monto = model.Monto
            };

            return View("ConfirmarTransaccionTerceros", confirmacionViewModel);
        }

        /// <summary>
        /// Confirma y procesa la transacción entre terceros
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarTransaccionTerceros(ConfirmarTransaccionTercerosViewModel model)
        {
            var cuentaOrigen = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaOrigen);
            var cuentaDestino = await _repositorioCuenta.ObtenerPorNumeroCuentaAsync(model.NumeroCuentaDestino);

            if (cuentaOrigen == null || cuentaDestino == null)
            {
                TempData["MensajeError"] = "Cuentas no encontradas.";
                return RedirectToAction(nameof(TransaccionTerceros));
            }

            // Descontar de origen
            cuentaOrigen.Balance -= model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuentaOrigen);

            // Acreditar a destino
            cuentaDestino.Balance += model.Monto;
            await _repositorioCuenta.ActualizarAsync(cuentaDestino);

            await _repositorioCuenta.GuardarCambiosAsync();

            // Registrar transacciones en ambas cuentas
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

            _logger.LogInformation($"Transacción de RD${model.Monto} de {cuentaOrigen.NumeroCuenta} a {cuentaDestino.NumeroCuenta}");

            TempData["MensajeExito"] = "Transacción realizada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}