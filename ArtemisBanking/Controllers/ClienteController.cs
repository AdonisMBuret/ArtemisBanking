using ArtemisBanking.Application.Common;
using ArtemisBanking.Application.DTOs;
using ArtemisBanking.Application.Interfaces;
using ArtemisBanking.Web.ViewModels.Cliente;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArtemisBanking.Web.Controllers
{
    /// <summary>
    /// Controlador para todas las funcionalidades del Cliente
    /// Solo accesible para usuarios con rol "Cliente"
    /// </summary>
    [Authorize(Policy = "SoloCliente")]
    public class ClienteController : Controller
    {
        // Servicios que usaremos
        private readonly IServicioTransaccion _servicioTransaccion;
        private readonly IServicioBeneficiario _servicioBeneficiario;
        private readonly IMapper _mapper;
        private readonly ILogger<ClienteController> _logger;

        public ClienteController(
            IServicioTransaccion servicioTransaccion,
            IServicioBeneficiario servicioBeneficiario,
            IMapper mapper,
            ILogger<ClienteController> logger)
        {
            _servicioTransaccion = servicioTransaccion;
            _servicioBeneficiario = servicioBeneficiario;
            _mapper = mapper;
            _logger = logger;
        }

        // Método helper para obtener el ID del usuario actual
        private string ObtenerUsuarioActualId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        }

        // ==================== HOME (LISTADO DE PRODUCTOS) ====================

        /// <summary>
        /// Página principal del cliente - muestra todos sus productos financieros
        /// (Cuentas de ahorro, Préstamos, Tarjetas de crédito)
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var usuarioId = ObtenerUsuarioActualId();

            // TODO: Aquí llamaremos a servicios para obtener:
            // - Cuentas de ahorro del cliente
            // - Préstamos activos del cliente
            // - Tarjetas de crédito activas del cliente

            // Por ahora retornamos una vista vacía
            var viewModel = new HomeClienteViewModel
            {
                CuentasAhorro = new List<CuentaClienteViewModel>(),
                Prestamos = new List<PrestamoClienteViewModel>(),
                TarjetasCredito = new List<TarjetaClienteViewModel>()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Muestra el detalle de una cuenta de ahorro (transacciones)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetalleCuenta(int id)
        {
            // TODO: Obtener cuenta con transacciones
            return View();
        }

        /// <summary>
        /// Muestra el detalle de un préstamo (tabla de amortización)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetallePrestamo(int id)
        {
            // TODO: Obtener préstamo con tabla de amortización
            return View();
        }

        /// <summary>
        /// Muestra el detalle de una tarjeta (consumos)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DetalleTarjeta(int id)
        {
            // TODO: Obtener tarjeta con consumos
            return View();
        }

        // ==================== GESTIÓN DE BENEFICIARIOS ====================

        /// <summary>
        /// Muestra el listado de beneficiarios del cliente
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Beneficiarios()
        {
            var usuarioId = ObtenerUsuarioActualId();

            // Obtener beneficiarios del servicio
            var resultado = await _servicioBeneficiario.ObtenerBeneficiariosAsync(usuarioId);

            if (!resultado.Exito)
            {
                TempData["ErrorMessage"] = resultado.Mensaje;
                return View(new ListaBeneficiariosViewModel
                {
                    Beneficiarios = new List<BeneficiarioItemViewModel>()
                });
            }

            // Mapear a ViewModel
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

        /// <summary>
        /// Procesa la creación de un nuevo beneficiario
        /// Se llama desde un modal en la vista
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarBeneficiario(AgregarBeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Debe ingresar un número de cuenta válido";
                return RedirectToAction(nameof(Beneficiarios));
            }

            var usuarioId = ObtenerUsuarioActualId();

            // Llamar al servicio
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

        /// <summary>
        /// Elimina un beneficiario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarBeneficiario(int id)
        {
            var usuarioId = ObtenerUsuarioActualId();

            // Llamar al servicio
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

        // ==================== MÉTODO HELPER PARA CARGAR SELECTS ====================

        /// <summary>
        /// Obtiene las cuentas activas del cliente para llenar selectores
        /// </summary>
        private async Task<IEnumerable<SelectListItem>> ObtenerCuentasActivasAsync()
        {
            var usuarioId = ObtenerUsuarioActualId();

            // TODO: Llamar al servicio para obtener cuentas activas
            // Por ahora retornamos lista vacía
            return new List<SelectListItem>();
        }

        /// <summary>
        /// Obtiene las tarjetas activas del cliente para llenar selectores
        /// </summary>
        private async Task<IEnumerable<SelectListItem>> ObtenerTarjetasActivasAsync()
        {
            var usuarioId = ObtenerUsuarioActualId();

            // TODO: Llamar al servicio para obtener tarjetas activas
            // Por ahora retornamos lista vacía
            return new List<SelectListItem>();
        }

        /// <summary>
        /// Obtiene los préstamos activos del cliente para llenar selectores
        /// </summary>
        private async Task<IEnumerable<SelectListItem>> ObtenerPrestamosActivosAsync()
        {
            var usuarioId = ObtenerUsuarioActualId();

            // TODO: Llamar al servicio para obtener préstamos activos
            // Por ahora retornamos lista vacía
            return new List<SelectListItem>();
        }

        /// <summary>
        /// Obtiene los beneficiarios del cliente para llenar selectores
        /// </summary>
        private async Task<IEnumerable<SelectListItem>> ObtenerBeneficiariosAsync()
        {
            var usuarioId = ObtenerUsuarioActualId();

            var resultado = await _servicioBeneficiario.ObtenerBeneficiariosAsync(usuarioId);

            if (!resultado.Exito)
                return new List<SelectListItem>();

            return resultado.Datos.Select(b => new SelectListItem
            {
                Value = b.Id.ToString(),
                Text = $"{b.NumeroCuentaBeneficiario} - {b.NombreBeneficiario} {b.ApellidoBeneficiario}"
            });
        }


        // ⚠️ AGREGAR ESTOS MÉTODOS AL ClienteController.cs (después de beneficiarios)

        // ==================== TRANSACCIÓN EXPRESS ====================

        /// <summary>
        /// Muestra el formulario para hacer una transacción express
        /// (Transferencia a cualquier cuenta)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TransaccionExpress()
        {
            var viewModel = new TransaccionExpressViewModel
            {
                CuentasDisponibles = await ObtenerCuentasActivasAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa la transacción express
        /// Primero valida y luego muestra pantalla de confirmación
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TransaccionExpress(TransaccionExpressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CuentasDisponibles = await ObtenerCuentasActivasAsync();
                return View(model);
            }

            // Obtener información de la cuenta destino para confirmar
            var infoDestino = await _servicioTransaccion.ObtenerInfoCuentaDestinoAsync(
                model.NumeroCuentaDestino);

            if (!infoDestino.Exito)
            {
                ModelState.AddModelError(string.Empty, infoDestino.Mensaje);
                model.CuentasDisponibles = await ObtenerCuentasActivasAsync();
                return View(model);
            }

            // Redirigir a pantalla de confirmación
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

        /// <summary>
        /// Confirma y ejecuta la transacción express
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarTransaccionExpress(ConfirmarTransaccionViewModel model)
        {
            var usuarioId = ObtenerUsuarioActualId();

            // Crear el DTO
            var dto = new TransaccionExpressDTO
            {
                CuentaOrigenId = model.CuentaOrigenId,
                NumeroCuentaDestino = model.NumeroCuentaDestino,
                Monto = model.Monto,
                UsuarioId = usuarioId
            };

            // Llamar al servicio
            var resultado = await _servicioTransaccion.RealizarTransaccionExpressAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = resultado.Mensaje;
            return RedirectToAction(nameof(TransaccionExpress));
        }

        // ==================== PAGO A TARJETA DE CRÉDITO ====================

        /// <summary>
        /// Muestra el formulario para pagar una tarjeta de crédito
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PagarTarjeta()
        {
            var viewModel = new PagoTarjetaViewModel
            {
                TarjetasDisponibles = await ObtenerTarjetasActivasAsync(),
                CuentasDisponibles = await ObtenerCuentasActivasAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa el pago a tarjeta de crédito
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarTarjeta(PagoTarjetaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.TarjetasDisponibles = await ObtenerTarjetasActivasAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasAsync();
                return View(model);
            }

            var usuarioId = ObtenerUsuarioActualId();

            // Crear el DTO
            var dto = new PagoTarjetaClienteDTO
            {
                TarjetaId = model.TarjetaId,
                CuentaOrigenId = model.CuentaOrigenId,
                Monto = model.Monto,
                UsuarioId = usuarioId
            };

            // Llamar al servicio
            var resultado = await _servicioTransaccion.PagarTarjetaCreditoClienteAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            // Si hubo error
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            model.TarjetasDisponibles = await ObtenerTarjetasActivasAsync();
            model.CuentasDisponibles = await ObtenerCuentasActivasAsync();
            return View(model);
        }

        // ==================== PAGO A PRÉSTAMO ====================

        /// <summary>
        /// Muestra el formulario para pagar un préstamo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PagarPrestamo()
        {
            var viewModel = new PagoPrestamoViewModel
            {
                PrestamosDisponibles = await ObtenerPrestamosActivosAsync(),
                CuentasDisponibles = await ObtenerCuentasActivasAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa el pago a préstamo
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarPrestamo(PagoPrestamoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.PrestamosDisponibles = await ObtenerPrestamosActivosAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasAsync();
                return View(model);
            }

            var usuarioId = ObtenerUsuarioActualId();

            // Crear el DTO
            var dto = new PagoPrestamoClienteDTO
            {
                PrestamoId = model.PrestamoId,
                CuentaOrigenId = model.CuentaOrigenId,
                Monto = model.Monto,
                UsuarioId = usuarioId
            };

            // Llamar al servicio
            var resultado = await _servicioTransaccion.PagarPrestamoClienteAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            // Si hubo error
            ModelState.AddModelError(string.Empty, resultado.Mensaje);
            model.PrestamosDisponibles = await ObtenerPrestamosActivosAsync();
            model.CuentasDisponibles = await ObtenerCuentasActivasAsync();
            return View(model);
        }

        // ==================== PAGO A BENEFICIARIO ====================

        /// <summary>
        /// Muestra el formulario para pagar a un beneficiario
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PagarBeneficiario()
        {
            var viewModel = new PagoBeneficiarioViewModel
            {
                BeneficiariosDisponibles = await ObtenerBeneficiariosAsync(),
                CuentasDisponibles = await ObtenerCuentasActivasAsync()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Procesa el pago a beneficiario
        /// Primero valida y luego muestra pantalla de confirmación
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PagarBeneficiario(PagoBeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.BeneficiariosDisponibles = await ObtenerBeneficiariosAsync();
                model.CuentasDisponibles = await ObtenerCuentasActivasAsync();
                return View(model);
            }

            var usuarioId = ObtenerUsuarioActualId();

            // Obtener datos del beneficiario
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

            // Guardar el ID del beneficiario en TempData para usarlo en la confirmación
            TempData["BeneficiarioId"] = model.BeneficiarioId;

            return View("ConfirmarPagoBeneficiario", confirmacionVM);
        }

        /// <summary>
        /// Confirma y ejecuta el pago a beneficiario
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarPagoBeneficiario(ConfirmarTransaccionViewModel model)
        {
            var usuarioId = ObtenerUsuarioActualId();
            var beneficiarioId = (int)TempData["BeneficiarioId"];

            // Crear el DTO
            var dto = new PagoBeneficiarioDTO
            {
                BeneficiarioId = beneficiarioId,
                CuentaOrigenId = model.CuentaOrigenId,
                Monto = model.Monto,
                UsuarioId = usuarioId
            };

            // Llamar al servicio
            var resultado = await _servicioTransaccion.PagarBeneficiarioAsync(dto);

            if (resultado.Exito)
            {
                TempData["SuccessMessage"] = resultado.Mensaje;
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = resultado.Mensaje;
            return RedirectToAction(nameof(PagarBeneficiario));
        }

        // ==================== continuar  ====================


    }
}