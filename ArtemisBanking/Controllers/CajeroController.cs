using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Application.ViewModels;
using ArtemisBanking.Application.ViewModels.Cajero;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBanking.Web.Controllers
{
 
    [Authorize(Policy = "SoloCajero")] // Solo los cajeros pueden acceder a este controlador
    public class CajeroController : Controller
    {
        // Inyectamos el servicio del cajero que tiene toda la lógica de negocio
        private readonly IServicioCajero _servicioCajero;
        private readonly IMapper _mapper;
        private readonly ILogger<CajeroController> _logger;

        // Constructor: aquí recibimos las dependencias que necesitamos
        public CajeroController(
            IServicioCajero servicioCajero,
            IMapper mapper,
            ILogger<CajeroController> logger)
        {
            _servicioCajero = servicioCajero;
            _mapper = mapper;
            _logger = logger;
        }

        // Método helper para obtener el ID del cajero que está logueado
        // Lo usaremos en varios métodos para saber quién está haciendo las operaciones
        private string ObtenerCajeroActualId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        // ==================== HOME (DASHBOARD DEL CAJERO) ====================

        /// <summary>
        /// Página principal del cajero - muestra estadísticas del día
        /// Aquí el cajero puede ver:
        /// - Cuántas transacciones hizo hoy
        /// - Cuántos pagos procesó hoy
        /// - Cuántos depósitos hizo hoy
        /// - Cuántos retiros hizo hoy
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Obtenemos el ID del cajero que está logueado
            var cajeroId = ObtenerCajeroActualId();

            // Llamamos al servicio para obtener las estadísticas del día
            var resultado = await _servicioCajero.ObtenerDashboardAsync(cajeroId);

            // Si algo salió mal, mostramos un error y devolvemos un dashboard vacío
            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return View(new DashboardCajeroViewModel());
            }

            // Mapeamos el DTO que nos devolvió el servicio a un ViewModel para la vista
            var viewModel = _mapper.Map<DashboardCajeroViewModel>(resultado.Datos);

            // Devolvemos la vista con los datos del dashboard
            return View(viewModel);
        }

        // ==================== DEPÓSITO ====================

        /// <summary>
        /// Muestra el formulario para realizar un depósito
        /// El cajero debe ingresar:
        /// - Número de cuenta destino (a quién le va a depositar)
        /// - Monto a depositar
        /// </summary>
        [HttpGet]
        public IActionResult Deposito()
        {
            // Simplemente mostramos la vista con el formulario vacío
            return View(new DepositoCajeroViewModel());
        }

        /// <summary>
        /// Procesa el depósito (POST del formulario)
        /// Primero valida los datos y luego muestra una pantalla de confirmación
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra ataques CSRF
        public async Task<IActionResult> Deposito(DepositoCajeroViewModel model)
        {
            // Verificamos que todos los campos del formulario sean válidos
            if (!ModelState.IsValid)
            {
                // Si hay errores, mostramos el formulario de nuevo con los mensajes de error
                return View(model);
            }

            // Aquí necesitamos obtener los datos de la cuenta destino para mostrar la confirmación
            // Para esto, podemos usar el servicio de transacciones o crear un método en el servicio de cajero
            // Por ahora, vamos a redirigir directamente a la confirmación
            // En una implementación real, deberías verificar primero que la cuenta existe

            var cajeroId = ObtenerCajeroActualId();

            // Creamos el DTO con los datos del formulario
            var dto = new DepositoCajeroDTO
            {
                NumeroCuentaDestino = model.NumeroCuentaDestino,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

            // Llamamos al servicio para procesar el depósito
            var resultado = await _servicioCajero.RealizarDepositoAsync(dto);

            // Si todo salió bien, mostramos mensaje de éxito y volvemos al home
            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            // Si hubo algún error, mostramos el mensaje y devolvemos el formulario
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        // Nota: En una implementación más completa, deberías tener una pantalla de confirmación
        // intermedia que muestre el nombre del cliente antes de confirmar el depósito.
        // Por simplicidad, aquí lo procesamos directamente.

        // ==================== RETIRO ====================

        /// <summary>
        /// Muestra el formulario para realizar un retiro
        /// El cajero debe ingresar:
        /// - Número de cuenta origen (de quién se va a retirar)
        /// - Monto a retirar
        /// </summary>
        [HttpGet]
        public IActionResult Retiro()
        {
            // Mostramos la vista con el formulario vacío
            return View(new RetiroCajeroViewModel());
        }

        /// <summary>
        /// Procesa el retiro (POST del formulario)
        /// Valida que la cuenta exista y tenga fondos suficientes
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retiro(RetiroCajeroViewModel model)
        {
            // Verificamos que todos los campos del formulario sean válidos
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cajeroId = ObtenerCajeroActualId();

            // Creamos el DTO con los datos del formulario
            var dto = new RetiroCajeroDTO
            {
                NumeroCuentaOrigen = model.NumeroCuentaOrigen,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

            // Llamamos al servicio para procesar el retiro
            // El servicio se encarga de:
            // 1. Validar que la cuenta existe y está activa
            // 2. Validar que tenga fondos suficientes
            // 3. Descontar el monto de la cuenta
            // 4. Registrar la transacción
            // 5. Enviar correo al cliente
            var resultado = await _servicioCajero.RealizarRetiroAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            // Si hubo error (cuenta no existe, fondos insuficientes, etc.)
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        // ==================== PAGO A TARJETA DE CRÉDITO ====================

        /// <summary>
        /// Muestra el formulario para realizar un pago a tarjeta de crédito
        /// El cajero debe ingresar:
        /// - Número de cuenta origen (de dónde se toma el dinero)
        /// - Número de tarjeta (16 dígitos)
        /// - Monto a pagar
        /// </summary>
        [HttpGet]
        public IActionResult PagarTarjeta()
        {
            return View(new PagoTarjetaCajeroViewModel());
        }

        /// <summary>
        /// Procesa el pago a tarjeta de crédito
        /// IMPORTANTE: Si el monto es mayor a la deuda, solo se paga hasta la deuda
        /// No se puede pagar de más a una tarjeta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarTarjeta(PagoTarjetaCajeroViewModel model)
        {
            // Validar el formulario
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cajeroId = ObtenerCajeroActualId();

            // Creamos el DTO
            var dto = new PagoTarjetaCajeroDTO
            {
                NumeroCuentaOrigen = model.NumeroCuentaOrigen,
                NumeroTarjeta = model.NumeroTarjeta,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

            // Llamamos al servicio que se encarga de:
            // 1. Validar que la cuenta y tarjeta existen y están activas
            // 2. Validar que la cuenta tenga fondos suficientes
            // 3. Calcular el monto real a pagar (si paga más que la deuda, solo se paga la deuda)
            // 4. Descontar de la cuenta y reducir la deuda de la tarjeta
            // 5. Registrar la transacción
            // 6. Enviar correo al cliente
            var resultado = await _servicioCajero.PagarTarjetaCreditoAsync(dto);

            if (resultado.Exito)
            {
                // El mensaje ya incluye el monto real pagado
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            // Posibles errores:
            // - Cuenta o tarjeta no válida
            // - Fondos insuficientes
            // - Tarjeta sin deuda
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        // ==================== PAGO A PRÉSTAMO ====================

        /// <summary>
        /// Muestra el formulario para realizar un pago a préstamo
        /// El cajero debe ingresar:
        /// - Número de cuenta origen (de dónde se toma el dinero)
        /// - Número de préstamo (9 dígitos)
        /// - Monto a pagar
        /// </summary>
        [HttpGet]
        public IActionResult PagarPrestamo()
        {
            return View(new PagoPrestamoCajeroViewModel());
        }

        /// <summary>
        /// Procesa el pago a préstamo
        /// IMPORTANTE: El pago se aplica a las cuotas de forma secuencial
        /// Si sobra dinero después de pagar todas las cuotas, se devuelve a la cuenta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarPrestamo(PagoPrestamoCajeroViewModel model)
        {
            // Validar el formulario
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cajeroId = ObtenerCajeroActualId();

            // Creamos el DTO
            var dto = new PagoPrestamoCajeroDTO
            {
                NumeroCuentaOrigen = model.NumeroCuentaOrigen,
                NumeroPrestamo = model.NumeroPrestamo,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

            // Llamamos al servicio que hace el trabajo pesado:
            // 1. Valida cuenta y préstamo
            // 2. Valida fondos suficientes
            // 3. Aplica el pago a las cuotas pendientes de forma secuencial
            //    - Paga la primera cuota pendiente
            //    - Si sobra, paga la siguiente
            //    - Y así sucesivamente
            // 4. Si sobra dinero, lo devuelve a la cuenta
            // 5. Marca el préstamo como completado si se pagaron todas las cuotas
            // 6. Registra la transacción
            // 7. Envía correo al cliente
            var resultado = await _servicioCajero.PagarPrestamoAsync(dto);

            if (resultado.Exito)
            {
                // El mensaje incluye cuántas cuotas se pagaron y si sobró dinero
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            // Posibles errores:
            // - Cuenta o préstamo no válido
            // - Fondos insuficientes
            // - No se pudo aplicar el pago a ninguna cuota
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

        // ==================== TRANSACCIÓN ENTRE CUENTAS DE TERCEROS ====================

        /// <summary>
        /// Muestra el formulario para realizar una transacción entre dos cuentas
        /// (ambas cuentas son de clientes diferentes)
        /// El cajero debe ingresar:
        /// - Número de cuenta origen (de quién se toma el dinero)
        /// - Número de cuenta destino (a quién se le envía)
        /// - Monto a transferir
        /// </summary>
        [HttpGet]
        public IActionResult TransaccionTerceros()
        {
            return View(new TransaccionTercerosCajeroViewModel());
        }

        /// <summary>
        /// Procesa la transacción entre terceros
        /// Es como una transferencia pero hecha por el cajero entre dos clientes
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransaccionTerceros(TransaccionTercerosCajeroViewModel model)
        {
            // Validar el formulario
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var cajeroId = ObtenerCajeroActualId();

            // Creamos el DTO
            var dto = new TransaccionTercerosCajeroDTO
            {
                NumeroCuentaOrigen = model.NumeroCuentaOrigen,
                NumeroCuentaDestino = model.NumeroCuentaDestino,
                Monto = model.Monto,
                CajeroId = cajeroId
            };

            // El servicio se encarga de:
            // 1. Validar que ambas cuentas existan y estén activas
            // 2. Validar que la cuenta origen tenga fondos suficientes
            // 3. Descontar de la cuenta origen
            // 4. Acreditar a la cuenta destino
            // 5. Registrar dos transacciones (una en cada cuenta)
            //    - DÉBITO en cuenta origen
            //    - CRÉDITO en cuenta destino
            // 6. Enviar correos a AMBOS clientes
            //    - Al que envió: "Transacción realizada a cuenta XXXX"
            //    - Al que recibió: "Transacción recibida desde cuenta XXXX"
            var resultado = await _servicioCajero.TransaccionEntreTercerosAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            // Posibles errores:
            // - Alguna cuenta no válida
            // - Fondos insuficientes
            // - Cuentas iguales (aunque esto ya se valida en el frontend)
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            return View(model);
        }

    }
}